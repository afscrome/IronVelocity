parser grammar VelocityParser;

options {
    tokenVocab=VelocityLexer;
}

template: block
	(
		EOF
		| (end {NotifyErrorListeners("Unexpected #end"); } template)
	) ;

block: (text | reference | setDirective | ifBlock | customDirective | comment)* ;

text: (rawText | literal | whitespace)+ ;

//"Dollar  (Exclamation | LeftCurley)*"" accounts for scenarios where the DOLLAR_SEEN
// lexical state was entered, but did not move into the REFERENCE state
// RIGHT_CURLEY required to cope with ${formal}}
// DOT required to cope with "$name."
rawText : (Text | Hash | Dollar (Exclamation | LeftCurley)* | RightCurley | Dot | EscapedDollar | EscapedHash | LoneEscape )+ ;

whitespace: (VerticalWhitespace | Newline)+ ;

comment : COMMENT (Newline | EOF) | blockComment;
blockComment : BlockCommentStart (BlockCommentBody | blockComment)*  BlockCommentEnd ;

directiveArguments: (VerticalWhitespace? LeftParenthesis directiveArgument* RightParenthesis)? ;
directiveArgument : expression | directiveWord;
directiveWord : Identifier;

endOfLineWhitespace : VerticalWhitespace? Newline ;

literal : LiteralContent;
setDirective: Set VerticalWhitespace? LeftParenthesis assignment RightParenthesis endOfLineWhitespace?;
ifBlock : If VerticalWhitespace? LeftParenthesis expression RightParenthesis endOfLineWhitespace? block ifElseifBlock* ifElseBlock? end ;
ifElseifBlock : ElseIf VerticalWhitespace? LeftParenthesis expression RightParenthesis endOfLineWhitespace? block ;
ifElseBlock : Else endOfLineWhitespace? block ;
end: End endOfLineWhitespace? ;


customDirective :
	Hash (DirectiveName | LeftCurley DirectiveName RightCurley) directiveBody ;

directiveBody
	: { IsSingleLineDirective() }? directiveArguments endOfLineWhitespace?
	| { IsBlockDirective() }? directiveArguments endOfLineWhitespace?  block end
	| { IsUnknownDirective() }? 
	;


reference : Dollar Exclamation? referenceBody
	| Dollar Exclamation? LeftCurley referenceBody RightCurley;

referenceBody : variable (Dot ( methodInvocation | propertyInvocation)| indexInvocation)* ;

variable : Identifier;
propertyInvocation: Identifier ;
methodInvocation: Identifier LeftParenthesis argument_list RightParenthesis;
indexInvocation : LeftSquare argument_list RightSquare ;

argument_list : (expression (Comma expression)*)? ;

assignment: reference Assign expression ;

expression
	: reference #ReferenceExpression
	| Boolean=(True | False) #BooleanLiteral
	| Minus? Number Dot Number #FloatingPointLiteral
	| Minus? Number #IntegerLiteral
	| string #StringExpression
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

string 
	: String #StringLiteral
	| InterpolatedString #InterpolatedStringLiteral ;

dictionaryEntry : dictionaryKey Colon expression;
dictionaryKey : string | Identifier ;