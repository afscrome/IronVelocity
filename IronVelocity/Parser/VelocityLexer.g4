lexer grammar VelocityLexer;

tokens {
	COMMENT,
	TRANSITION
}

fragment ALPHA_CHAR : 'a'..'z' | 'A'..'Z';
fragment NUMERIC_CHAR : '0'..'9' ;
fragment DIRECTIVE_CHAR : ALPHA_CHAR | NUMERIC_CHAR | '_' ;
fragment IDENTIFIER_CHAR : DIRECTIVE_CHAR | '-' ;
fragment WHITESPACE_CHAR : ' ' | '\t' ;

fragment IDENTIFIER_TEXT :  (ALPHA_CHAR | '_') IDENTIFIER_CHAR* ;
fragment DIRECTIVE_TEXT : ALPHA_CHAR DIRECTIVE_CHAR* ;
fragment WHITESPACE_TEXT : WHITESPACE_CHAR+ ;

//===================================
// Default mode used for parsing text
Text : ~('$'| '#' | ' ' | '\t' | '\r' | '\n' | '\\' )+ ;

SingleLineComment : '##' ~('\r' | '\n')* -> type(COMMENT), mode(DEFAULT_MODE);
BlockCommentStart : '#*' ->pushMode(BLOCK_COMMENT) ;
Set : ( '#set' | '#{set}' ) -> mode(DIRECTIVE_ARGUMENTS) ;
If : ( '#if' | '#{if}' ) -> mode(DIRECTIVE_ARGUMENTS) ;
ElseIf : ( '#elseif' | '#elseif' ) -> mode(DIRECTIVE_ARGUMENTS);
Else : ( '#else' | '#{else}' ) -> mode(DIRECTIVE_ARGUMENTS) ;
End : ( '#end' | '#{end}' ) -> mode(DIRECTIVE_ARGUMENTS) ;
LiteralContent : '#[[' (~(']') | ']' ~(']'))* ']]' ;

Dollar : '$' ->  mode(POSSIBLE_REFERENCE) ;
Hash : '#' -> mode(POSSIBLE_CUSTOM_DIRECTIVE) ;

Whitespace:  WHITESPACE_TEXT ;
Newline : '\r' | '\n' | '\r\n' ;
EscapedDollar: '\\'+ '$' ;
EscapedHash: '\\'+ '#' ;
LoneEscape : '\\' ;

//===================================
// The mode is for when a hash has been seen in a location that allows text so
// the parser can distinguish between a textual '#', comments and directives
mode POSSIBLE_CUSTOM_DIRECTIVE ;

LeftCurley : '{';
DirectiveName : DIRECTIVE_TEXT -> mode(DIRECTIVE_ARGUMENTS);

TextFallback1 : -> type(TRANSITION), channel(HIDDEN), mode(DEFAULT_MODE) ;

mode DIRECTIVE_ARGUMENTS ;

RightCurley : '}' ;
WhitespaceA:  WHITESPACE_TEXT -> type(Whitespace);
LeftParenthesis : '(' -> mode(DEFAULT_MODE), pushMode(ARGUMENTS) ;
TextFallback2 : -> type(TRANSITION), channel(HIDDEN), mode(DEFAULT_MODE) ;


//===================================
// In Velocity, block comments can be nested.  i.e. the string "#* #*comment*# *#" is fully a comment
// In c style languages, only up to the first "*#" is typically considered a comment
// Because of this, we need match start & close comment tokens by pushing & poping the mode.
mode BLOCK_COMMENT ;

NestedBlockCommentStart : '#*' -> pushMode(BLOCK_COMMENT), type(BlockCommentStart);
BlockCommentEnd : '*#' -> popMode ;
BlockCommentBody :  (~('#' | '*') | '#' ~'*' | '*' ~'#')+ ;



//===================================
// The mode is for when a dollar has been seen to parse a possible reference
//
mode POSSIBLE_REFERENCE ;

Exclamation : '!' ;
LeftCurley4 : '{' -> type(LeftCurley);
Identifier : IDENTIFIER_TEXT -> mode(REFERENCE) ;

TextFallback4 : -> type(TRANSITION), channel(HIDDEN), mode(DEFAULT_MODE) ;


//===================================
// The mode is entered once we haev a complete reference. May be followed by either
// text, or further member access.
mode REFERENCE ;

Dot : '.' ;
Identifier5: IDENTIFIER_TEXT -> type(Identifier) , mode(REFERENCE_MEMBER_ACCESS) ;
LeftSquare5 : '[' -> type(LeftSquare), pushMode(ARGUMENTS);
RightCurley5 : '}' ->  type(RightCurley), mode(DEFAULT_MODE);

DotDotText5 : '..' -> type(Text), mode(DEFAULT_MODE) ;
TextFallback5 : -> type(TRANSITION), channel(HIDDEN), mode(DEFAULT_MODE) ;


//===================================
// The mode is entered once we have a member access.  If followed by '(' the member access
// becomes a method invocation, and moves to the PARENTHESISED_ARGUMENT_LIST state to tokenise arguments.
// Otherwise it is a property invocation and returns to the REFERENCE state.
mode REFERENCE_MEMBER_ACCESS ;

LeftParenthesis6 : '(' -> type(LeftParenthesis), mode(REFERENCE), pushMode(ARGUMENTS) ;
LeftSquare6 : '[' -> type(LeftSquare), pushMode(ARGUMENTS);
TextFallback6 : -> type(TRANSITION), channel(HIDDEN), mode(REFERENCE) ;


//===================================
// Used when parsing arguments in either a method call "$foo.bar(ARGUMENTS)",
// or a directive #directive(ARGUMENTS)
mode ARGUMENTS ;

Whitespace7 : WHITESPACE_TEXT -> type(Whitespace), channel(HIDDEN);
Comma : ',' ;
True : 'true' ;
False : 'false' ;
Number : NUMERIC_CHAR+ ;
Dot7 : '.' -> type(Dot) ;
String : '\'' (~('\'' | '\r' | '\n' ) | '\'\'' )* '\'' ;
InterpolatedString : '"' (~('"' | '\r' | '\n' ) | '""')* '"' ;
Dollar7 : '$' -> type(Dollar) ;
Exclamation7 : '!' -> type(Exclamation) ;
LeftCurley7 : '{' -> type(LeftCurley);
RightCurley7 : '}' -> type(RightCurley);
DotDot : '..' ;
Colon : ':' ;
Assign : '=' ;
Multiply : '*' ;
Divide : '/' ;
Modulo : '%' ;
Plus : '+' ;
Minus : '-' ;
LessThan : '<' | 'lt' ;
GreaterThan : '>' | 'gt' ;
LessThanOrEqual : '<=' | 'le' ;
GreaterThanOrEqual : '>=' | 'ge' ;
Equal : '==' | 'eq' ;
NotEqual : '!=' | 'ne' ;
And : '&&' | 'and' ;
Or : '||' | 'or' ;
Identifier7 : IDENTIFIER_TEXT -> type(Identifier) ;
LeftParenthesis8 : '(' -> type(LeftParenthesis), pushMode(ARGUMENTS);
RightParenthesis : ')' -> popMode ;
LeftSquare : '['  -> pushMode(ARGUMENTS);
RightSquare : ']' -> popMode ;
UnknownChar : .;
