using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronVelocity.VisualStudio.IntelliSense
{
    public class CompletionSource : ICompletionSource
    {
        private readonly IList<Completion> _completions = new List<Completion>();
        private readonly ITextBuffer _buffer;
        private readonly ITextStructureNavigatorSelectorService _navigation;
        private readonly IGlyphService _glyphService;

        public CompletionSource(ITextBuffer buffer, ITextStructureNavigatorSelectorService navigation, IGlyphService glyphService)
        {
            _buffer = buffer;
            _navigation = navigation;
            _glyphService = glyphService;

            var keywordImage = _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupNamespace, StandardGlyphItem.GlyphItemPublic);
            foreach (var str in _poorMansKeywords)
            {
                _completions.Add(new Completion(str, str, str, keywordImage, null));
            }

            var extensionImage = _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupClass, StandardGlyphItem.GlyphItemPublic);
            foreach (var str in _poorMansExtensionIntellisense)
            {
                _completions.Add(new Completion(str, str, str, extensionImage, null));
            }

            var localVariableImage = _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic);
            foreach (var localVar in new[] { "$localVar1", "$localVar2", "$anotherLocalVar" })
            {
                _completions.Add(new Completion(localVar, localVar, localVar, localVariableImage, null));
            }

            _completions = _completions.OrderBy(x => x.DisplayText).ToList();
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {

            completionSets.Add(new CompletionSet(
                "Velocity",    //the non-localized title of the tab 
                "Velocity",    //the display title of the tab
                FindTokenSpanAtPosition(session.GetTriggerPoint(_buffer), session),
                _completions,
                null
                ));
        }

        private ITrackingSpan FindTokenSpanAtPosition(ITrackingPoint point, ICompletionSession session)
        {
            SnapshotPoint currentPoint = (session.TextView.Caret.Position.BufferPosition) - 1;
            ITextStructureNavigator navigator = _navigation.GetTextStructureNavigator(_buffer);
            TextExtent extent = navigator.GetExtentOfWord(currentPoint);
            return currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
        }

        private bool m_isDisposed;
        public void Dispose()
        {
            if (!m_isDisposed)
            {
                GC.SuppressFinalize(this);
                m_isDisposed = true;
            }
        }

        private string[] _poorMansKeywords = new[]
            {
                "#if",
                "#elseif",
                "#end",
                "#set(",
                "#foreach(",
                "#stop"
            };

        private string[] _poorMansExtensionIntellisense = new[] {
                "$core_v2_encoding",
                "$core_v2_language",
                "$core_v2_page",
                "$core_v2_widget",
                "$core_v2_blogPost",
                "$core_v2_user",
                "$core_v2_blogComment",
                "$core_v2_forumThread",
                "$core_v2_following",
                "$core_v2_userValidation",
                "$core_v2_infoResult",
                "$core_v2_forumReply",
                "$core_v2_searchResult",
                "$core_v2_conversation",
                "$core_v2_cfs",
                "$core_v2_wikiPage",
                "$core_v2_wikiFiles",
                "$core_v2_wikiPageRevision",
                "$core_v2_nodePermission",
                "$core_v2_activityMessage",
                "$core_v2_activityStory",
                "$core_v2_group",
                "$core_v2_follower",
                "$core_v2_userPresence",
                "$core_v2_groupUserMember",
                "$core_v2_groupRoleMember",
                "$core_v2_groupContactRequest",
                "$core_v2_conversationMessage",
                "$core_v2_wikiToc",
                "$core_v2_statusMessage",
                "$core_v2_wikiComment",
                "$core_v2_mediaComment",
                "$core_v2_forum",
                "$core_v2_blog",
                "$core_v2_role",
                "$core_v2_roleUsers",
                "$core_v2_permission",
                "$core_v2_friendship",
                "$core_v2_gallery",
                "$core_v2_replyMessage",
                "$core_v2_wiki",
                "$core_v2_media",
                "$core_v2_ui",
                "$core_v2_urls",
                "$core_v2_groupUrls",
                "$core_v2_utility",
                "$core_v2_mediaUrls",
                "$core_v2_forumUrls",
                "$core_v2_customPage",
                "$core_v2_message",
                "$core_v2_userFile",
                "$core_v2_userFolder",
                "$core_v2_blogUrls",
                "$core_v2_wikiUrls",
                "$core_v2_editor",
                "$core_v2_dynamicForm",
                "$core_v2_configuration",
                "$core_v2_editableGroup",
                "$core_v2_userProfileField",
                "$core_v2_userProfileFieldGroup",
                "$core_v2_blogPostSummary",
                "$core_v2_uploadedFile",
                "$core_v2_search",
                "$core_v2_userInvitation",
                "$core_v2_customNavigation",
                "$core_v2_rssFeedItem",
                "$core_v2_forumConfiguration",
                "$core_v2_favorite",
                "$core_v2_feature",
                "$core_v2_blogConfiguration",
                "$core_v2_mediaConfiguration",
                "$core_v2_wikiConfiguration",
                "$core_v2_emailDigest",
                "$core_v2_emoticon",
                "$core_v2_tags",
                "$core_v2_hashTag",
                "$core_v2_taggedContent",
                "$core_v2_apiKey",
                "$core_v2_rank",
                "$core_v2_ldapGroups",
                "$core_v2_ldapUsers",
                "$core_v2_rating",
                "$core_v2_ratedItem",
                "$core_v2_like",
                "$core_v2_comments",
                "$core_v2_contentViews",
                "$core_v2_oauthClient",
                "$core_v2_moderationTemplates",
                "$core_v2_mentionable",
                "$core_v2_mention",
                "$core_v2_abuseAppeal",
                "$core_v2_abuseReport",
                "$core_v2_abusiveContent",
                "$core_v2_wikiPageCommentSubscriptions",
                "$core_v2_contentType",
                "$core_v2_applicationType",
                "$core_v2_containerType",
                "$core_v2_container",
                "$core_v2_application",
                "$core_v2_content",
                "$core_v2_authorQualityScore",
                "$core_v2_contentQualityScore",
                "$core_v2_groupAuthorQualityScore",
                "$core_v2_searchCategory",
                "$core_v2_notification",
                "$core_v2_notificationType",
                "$core_v2_notificationDistributionType",
                "$core_v2_notificationUserPreference",
                "$core_v2_bookmark",
                "$core_v2_activityStorySitePreference",
                "$core_v2_activityStoryUserPreference",
                "$core_v2_contentRecommendation",
                "$core_v2_userRecommendation"
            };

    }
}
