parser grammar VelocityParser;

options {
    tokenVocab=VelocityLexer;
}

template: block
	(
		EOF
		| (end {NotifyErrorListeners("Unexpected #end"); } template)
	) ;

block: (text | reference | comment | setDirective | ifBlock | customDirective | literal)* ;

//"Dollar  (Exclamation | LeftCurley)*"" accounts for scenarios where the DOLLAR_SEEN
// lexical state was entered, but did not move into the REFERENCE state
// RIGHT_CURLEY required to cope with ${formal}}
// DOT required to cope with "$name."
text : (Text | Hash | Dollar (Exclamation | LeftCurley)* | RightCurley | Dot | Whitespace | Newline | EscapedDollar | EscapedHash | LoneEscape )+ ;


comment : Hash COMMENT (Newline | EOF) | Hash blockComment;
blockComment : BlockCommentStart (BlockCommentBody | blockComment)*  BlockCommentEnd ;

directiveArguments: (Whitespace? LeftParenthesis directiveArgument* RightParenthesis)? ;
directiveArgument : expression | directiveWord;
directiveWord : Identifier;

literal : Hash LiteralContent;
setDirective: Hash (Set | LeftCurley Set RightCurley) Whitespace? LeftParenthesis assignment RightParenthesis (Whitespace? Newline)?;
ifBlock : Hash (If | LeftCurley If RightCurley) Whitespace? LeftParenthesis expression RightParenthesis (Whitespace? Newline)? block ifElseifBlock* ifElseBlock? end ;
ifElseifBlock : Hash (ElseIf | LeftCurley ElseIf RightCurley) Whitespace? LeftParenthesis expression RightParenthesis (Whitespace? Newline)? block ;
ifElseBlock : Hash (Else | LeftCurley Else RightCurley) (Whitespace? Newline)? block ;
end: Hash (End | LeftCurley End RightCurley) (Whitespace? Newline)? ;

customDirective :
	{ !IsBlockDirective()}?  Hash (DirectiveName | LeftCurley DirectiveName RightCurley) directiveArguments (Whitespace? Newline)?
	 |  {IsBlockDirective()}? Hash (DirectiveName | LeftCurley DirectiveName RightCurley) directiveArguments (Whitespace? Newline)? block end ;


reference : Dollar Exclamation? referenceBody
	| Dollar Exclamation? LeftCurley referenceBody RightCurley;

referenceBody : variable (Dot ( methodInvocation | propertyInvocation))* ;

variable : Identifier;
propertyInvocation: Identifier ;
methodInvocation: Identifier LeftParenthesis argument_list RightParenthesis;

argument_list : (expression (Comma expression)*)? ;

assignment: reference Assign expression ;

expression
	: reference #ReferenceExpression
	| Boolean=(True | False) #BooleanLiteral
	| Minus? Number Dot Number #FloatingPointLiteral
	| Minus? Number #IntegerLiteral
	| String #StringLiteral
	| InterpolatedString #InterpolatedStringLiteral
	| LeftSquare argument_list RightSquare #List
	| LeftSquare expression  DotDot expression RightSquare #Range
	| LeftCurley (dictionaryEntry (Comma dictionaryEntry)*)?  RightCurley #DictionaryExpression
	| LeftParenthesis expression RightParenthesis #ParenthesisedExpression
	| Exclamation expression #UnaryExpression
	| expression Operator=(Multiply | Divide | Modulo) expression #MultiplicativeExpression
	| expression Operator=(Plus | Minus) expression #AdditiveExpression
	| expression Operator=(LessThan | GreaterThan | LessThanOrEqual | GreaterThanOrEqual) expression #RelationalExpression
	| expression Operator=(Equal | NotEqual) expression #EqualityExpression
	| expression And expression #AndExpression
	| expression Or expression #OrExpression ;

dictionaryEntry : Key=(String | InterpolatedString | Identifier) Colon expression;