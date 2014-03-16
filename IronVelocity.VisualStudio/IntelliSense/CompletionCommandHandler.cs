using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using IServiceProvider = System.IServiceProvider;

namespace IronVelocity.VisualStudio.IntelliSense
{
    public class CompletionCommandHandler : IOleCommandTarget
    {
        private readonly IVsTextView _vsTextView;
        private readonly ITextView _textView;

        private readonly IOleCommandTarget _nextCommandHandler;
        private ICompletionSession _completionSession;
        private readonly ICompletionBroker _completionBroker;
        private readonly IServiceProvider _serviceProvider;

        public CompletionCommandHandler(IVsTextView vsTextView, ITextView textView, ICompletionBroker broker, IServiceProvider serviceProvider)
        {
            _vsTextView = vsTextView;
            _textView = textView;
            _completionBroker = broker;
            _serviceProvider = serviceProvider;

            _vsTextView.AddCommandFilter(this, out _nextCommandHandler);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }


        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (VsShellUtilities.IsInAutomationFunction(_serviceProvider))
            {
                return _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }
            //make a copy of this so we can look at it after forwarding some commands 
            uint commandID = nCmdID;
            char typedChar = char.MinValue;
            //make sure the input is a char before getting it 
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            }

            //check for a commit character 
            if (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN
                || nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB
                || (char.IsWhiteSpace(typedChar) || char.IsPunctuation(typedChar)))
            {
                //check for a a selection 
                if (_completionSession != null && !_completionSession.IsDismissed)
                {
                    //if the selection is fully selected, commit the current session 
                    if (_completionSession.SelectedCompletionSet.SelectionStatus.IsSelected)
                    {
                        _completionSession.Commit();
                        //also, don't add the character to the buffer 
                        return VSConstants.S_OK;
                    }
                    else
                    {
                        //if there is no selection, dismiss the session
                        _completionSession.Dismiss();
                    }
                }
            }

            //pass along the command so the char is added to the buffer 
            int retVal = _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            bool handled = false;
            if (!typedChar.Equals(char.MinValue) && char.IsLetterOrDigit(typedChar))
            {
                if (_completionSession == null || _completionSession.IsDismissed) // If there is no active session, bring up completion
                {
                    this.TriggerCompletion();
                    _completionSession.Filter();
                }
                else     //the completion session is already active, so just filter
                {
                    _completionSession.Filter();
                }
                handled = true;
            }
            else if (commandID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE   //redo the filter if there is a deletion
                || commandID == (uint)VSConstants.VSStd2KCmdID.DELETE)
            {
                if (_completionSession != null && !_completionSession.IsDismissed)
                    _completionSession.Filter();
                handled = true;
            }
            if (handled) return VSConstants.S_OK;
            return retVal;
        }


        private bool TriggerCompletion()
        {
            //the caret must be in a non-projection location 
            SnapshotPoint? caretPoint =
            _textView.Caret.Position.Point.GetPoint(
            textBuffer => (!textBuffer.ContentType.IsOfType("projection")), PositionAffinity.Predecessor);
            if (!caretPoint.HasValue)
            {
                return false;
            }
            var trackingPoint = caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive);
            _completionSession = _completionBroker.CreateCompletionSession(_textView, trackingPoint, true);

            //subscribe to the Dismissed event on the session 
            _completionSession.Dismissed += this.OnSessionDismissed;
            _completionSession.Start();

            return true;
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            _completionSession.Dismissed -= this.OnSessionDismissed;
            _completionSession = null;
        }
    }
}
