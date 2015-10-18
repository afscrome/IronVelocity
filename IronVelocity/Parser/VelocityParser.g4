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

//"DOLLAR  (EXCLAMATION | LEFT_CURLEY)*"" accounts for scenarios where the DOLLAR_SEEN
// lexical state was entered, but did not move into the REFERENCE state
// RIGHT_CURLEY required to cope with ${formal}}
// DOT required to cope with "$name."
text : (TEXT | HASH | DOLLAR (EXCLAMATION | LEFT_CURLEY)* | RIGHT_CURLEY | DOT | WHITESPACE | NEWLINE | ESCAPED_DOLLAR | ESCAPED_HASH | LONE_ESCAPE )+ ;


comment : HASH COMMENT (NEWLINE | EOF) | HASH blockComment;
blockComment : BLOCK_COMMENT_START (BLOCK_COMMENT_BODY | blockComment)*  BLOCK_COMMENT_END ;

reference : DOLLAR EXCLAMATION? referenceBody
	| DOLLAR EXCLAMATION? LEFT_CURLEY referenceBody RIGHT_CURLEY;

referenceBody : variable (DOT ( methodInvocation | propertyInvocation))* ;

variable : IDENTIFIER;
propertyInvocation: IDENTIFIER ;
methodInvocation: IDENTIFIER LEFT_PARENTHESIS argument_list RIGHT_PARENTHESIS;

argument_list : (expression (COMMA expression)*)? ;

directiveArguments: (WHITESPACE? LEFT_PARENTHESIS directiveArgument* RIGHT_PARENTHESIS)? ;
directiveArgument : expression | directiveWord;
directiveWord : IDENTIFIER;
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

boolean : TRUE | FALSE ;
integer : MINUS? NUMBER ;
float: MINUS? NUMBER DOT NUMBER ;
string : STRING ;
interpolatedString : INTERPOLATED_STRING ;

list : LEFT_SQUARE argument_list RIGHT_SQUARE ;
range : LEFT_SQUARE expression  DOTDOT expression RIGHT_SQUARE ;
parenthesisedExpression : LEFT_PARENTHESIS expression RIGHT_PARENTHESIS;

setDirective: HASH SET WHITESPACE? LEFT_PARENTHESIS assignment RIGHT_PARENTHESIS (WHITESPACE? NEWLINE)?;
ifBlock : HASH IF WHITESPACE? LEFT_PARENTHESIS expression RIGHT_PARENTHESIS (WHITESPACE? NEWLINE)? block ifElseifBlock* ifElseBlock?end ;
ifElseifBlock : HASH ELSEIF WHITESPACE? LEFT_PARENTHESIS expression RIGHT_PARENTHESIS (WHITESPACE? NEWLINE)? block ;
ifElseBlock : HASH ELSE (WHITESPACE? NEWLINE)? block ;
end: HASH END (WHITESPACE? NEWLINE)? ;

customDirective :
	{ !IsBlockDirective()}?  HASH DIRECTIVE_NAME directiveArguments (WHITESPACE? NEWLINE)?
	 |  {IsBlockDirective()}? HASH DIRECTIVE_NAME directiveArguments (WHITESPACE? NEWLINE)? block end ;

assignment: reference ASSIGN expression ;

unaryExpression : primaryExpression
	| EXCLAMATION unaryExpression ;
multiplicativeExpression : unaryExpression
	| multiplicativeExpression (MULTIPLY | DIVIDE | MODULO) unaryExpression;
additiveExpression : multiplicativeExpression
	| additiveExpression (PLUS | MINUS) multiplicativeExpression;
relationalExpression : additiveExpression
	| relationalExpression (LESSTHAN | GREATERTHAN | LESSTHANOREQUAL | GREATERTHANOREQUAL) additiveExpression;
equalityExpression : relationalExpression
	| equalityExpression (EQUAL | NOTEQUAL) relationalExpression ;
andExpression : equalityExpression
	| andExpression AND equalityExpression ;
expression : andExpression
	| expression OR andExpression ;
