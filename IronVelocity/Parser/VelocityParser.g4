parser grammar VelocityParser;

options {
    tokenVocab=VelocityLexer;
}

template: block
	(
		EOF
		| (end {NotifyErrorListeners("Unexpected #end"); } template)
	) ;

block: (text | reference | comment | setDirective | ifBlock | customDirective )* ;

//"Dollar  (Exclamation | LeftCurley)*"" accounts for scenarios where the DOLLAR_SEEN
// lexical state was entered, but did not move into the REFERENCE state
// RIGHT_CURLEY required to cope with ${formal}}
// DOT required to cope with "$name."
text : (Text | Hash | Dollar (Exclamation | LeftCurley)* | RightCurley | Dot | Whitespace | Newline | EscapedDollar | EscapedHash | LoneEscape )+ ;


comment : Hash COMMENT (Newline | EOF) | Hash blockComment;
blockComment : BlockCommentStart (BlockCommentBody | blockComment)*  BlockCommentEnd ;

reference : Dollar Exclamation? referenceBody
	| Dollar Exclamation? LeftCurley referenceBody RightCurley;

referenceBody : variable (Dot ( methodInvocation | propertyInvocation))* ;

variable : Identifier;
propertyInvocation: Identifier ;
methodInvocation: Identifier LeftParenthesis argument_list RightParenthesis;

argument_list : (expression (Comma expression)*)? ;

directiveArguments: (Whitespace? LeftParenthesis directiveArgument* RightParenthesis)? ;
directiveArgument : expression | directiveWord;
directiveWord : Identifier;
primaryExpression : reference 
	| boolean
	| float
	| integer
	| string
	| interpolatedString
	| list
	| range 
	| parenthesisedExpression
	;

boolean : True | False ;
integer : Minus? Number ;
float: Minus? Number Dot Number ;
string : String ;
interpolatedString : InterpolatedString ;

list : LeftSquare argument_list RightSquare ;
range : LeftSquare expression  DotDot expression RightSquare ;
parenthesisedExpression : LeftParenthesis expression RightParenthesis;

setDirective: Hash Set Whitespace? LeftParenthesis assignment RightParenthesis (Whitespace? Newline)?;
ifBlock : Hash If Whitespace? LeftParenthesis expression RightParenthesis (Whitespace? Newline)? block ifElseifBlock* ifElseBlock?end ;
ifElseifBlock : Hash ElseIf Whitespace? LeftParenthesis expression RightParenthesis (Whitespace? Newline)? block ;
ifElseBlock : Hash Else (Whitespace? Newline)? block ;
end: Hash End (Whitespace? Newline)? ;

customDirective :
	{ !IsBlockDirective()}?  Hash DirectiveName directiveArguments (Whitespace? Newline)?
	 |  {IsBlockDirective()}? Hash DirectiveName directiveArguments (Whitespace? Newline)? block end ;

assignment: reference Assign expression ;

unaryExpression : primaryExpression
	| Exclamation unaryExpression ;
multiplicativeExpression : unaryExpression
	| multiplicativeExpression (Multiply | Divide | Modulo) unaryExpression;
additiveExpression : multiplicativeExpression
	| additiveExpression (Plus | Minus) multiplicativeExpression;
relationalExpression : additiveExpression
	| relationalExpression (LessThan | GreaterThan | LessThanOrEqual | GreaterThanOrEqual) additiveExpression;
equalityExpression : relationalExpression
	| equalityExpression (Equal | NotEqual) relationalExpression ;
andExpression : equalityExpression
	| andExpression And equalityExpression ;
expression : andExpression
	| expression Or andExpression ;
