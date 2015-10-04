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

fragment IDENTIFIER_TEXT :  ALPHA_CHAR IDENTIFIER_CHAR* ;
fragment DIRECTIVE_TEXT : ALPHA_CHAR DIRECTIVE_CHAR* ;
fragment WHITESPACE_TEXT : WHITESPACE_CHAR+ ;

//===================================
// Default mode used for parsing text
// Moves to the HASH_SEEN or DOLLAR_SEEN states upon seeing '$' or '#' respectively
TEXT : ~('$'| '#' | ' ' | '\t' | '\r' | '\n' | '\\' )+ ;
DOLLAR : '$' ->  mode(DOLLAR_SEEN) ;
HASH : '#' -> mode(HASH_SEEN) ;
WHITESPACE:  WHITESPACE_TEXT ;
NEWLINE : '\r' | '\n' | '\r\n' ;
ESCAPED_DOLLAR: '\\'+ '$' ;
ESCAPED_HASH: '\\'+ '#' ;
LONE_ESCAPE : '\\' ;

//===================================
// The mode is for when a hash has been seen in a location that allows text so
// the parser can distinguish between a textual '#', comments and directives
mode HASH_SEEN ;

SINGLE_LINE_COMMENT : '#' ~('\r' | '\n')* -> type(COMMENT), mode(DEFAULT_MODE);
//Need to switch to default mode before pushing so that when the the matching end tag pops, we end back in text mode.
BLOCK_COMMENT_START : '*' -> mode(DEFAULT_MODE), pushMode(BLOCK_COMMENT) ;
SET : 'set' -> mode(DIRECTIVE_ARGUMENTS) ;
IF : 'if' -> mode(DIRECTIVE_ARGUMENTS) ;
ELSEIF : 'elseif' -> mode(DIRECTIVE_ARGUMENTS);
ELSE : 'else' -> mode(DEFAULT_MODE) ;
END : 'end' -> mode(DEFAULT_MODE) ;
DIRECTIVE_NAME : DIRECTIVE_TEXT -> mode(DIRECTIVE_ARGUMENTS);
TEXT_FALLBACK2 : -> type(TRANSITION), channel(HIDDEN), mode(DEFAULT_MODE) ;

mode DIRECTIVE_ARGUMENTS ;

//TODO: Including spaces in LEFT_PARENTHESIS is a bit of a hack
WHITESPACE2A:  WHITESPACE_TEXT -> type(WHITESPACE);
LEFT_PARENTHESIS : '(' -> mode(DEFAULT_MODE), pushMode(ARGUMENTS) ;
TEXT_FALLBACK2A : -> type(TRANSITION), channel(HIDDEN), mode(DEFAULT_MODE) ;


//===================================
// In Velocity, block comments can be nested.  i.e. the string "#* #*comment*# *#" is fully a comment
// In c style languages, only up to the first "*#" is typically considered a comment
// Because of this, we need match start & close comment tokens by pushing & poping the mode.
mode BLOCK_COMMENT ;

BLOCK_COMMENT_START3 : '#*' -> pushMode(BLOCK_COMMENT), type(BLOCK_COMMENT_START);
BLOCK_COMMENT_END : '*#' -> popMode ;
BLOCK_COMMENT_BODY :  (~('#' | '*') | '#' ~'*' | '*' ~'#')+ ;



//===================================
// The mode is for when a dollar has been seen to parse a possible reference
//
mode DOLLAR_SEEN ;

IDENTIFIER : IDENTIFIER_TEXT -> mode(REFERENCE) ;
EXCLAMATION : '!' ;
LEFT_CURLEY : '{';

TEXT_FALLBACK4 : -> type(TRANSITION), channel(HIDDEN), mode(DEFAULT_MODE) ;



//===================================
// The mode is entered once we haev a complete reference. May be followed by either
// text, or further member access.
mode REFERENCE ;

DOT : '.' ;
IDENTIFIER5: IDENTIFIER_TEXT -> type(IDENTIFIER) , mode(REFERENCE_MEMBER_ACCESS) ;
RIGHT_CURLEY : '}' ->  mode(DEFAULT_MODE);

DOTDOT_TEXT5 : '..' -> type(TEXT), mode(DEFAULT_MODE) ;
TEXT_FALLBACK5 : -> type(TRANSITION), channel(HIDDEN), mode(DEFAULT_MODE) ;


//===================================
// The mode is entered once we have a member access.  If followed by '(' the member access
// becomes a method invocation, and moves to the ARGUMENTS state to tokenise arguments.
// Otherwise it is a property invocation and returns to the REFERENCE state.
mode REFERENCE_MEMBER_ACCESS ;

LEFT_PARENTHESIS6 : '(' -> type(LEFT_PARENTHESIS), mode(REFERENCE), pushMode(ARGUMENTS) ;
TEXT_FALLBACK6 : -> type(TRANSITION), channel(HIDDEN), mode(REFERENCE) ;



//===================================
// Used when parsing arguments in either a method call "$foo.bar(ARGUMENTS)",
// or a directive #directive(ARGUMENTS)
mode ARGUMENTS ;

WHITESPACE7 : WHITESPACE_TEXT -> type(WHITESPACE), channel(HIDDEN);
COMMA : ',' ;
LEFT_PARENTHESIS7 : '(' -> type(LEFT_PARENTHESIS), pushMode(ARGUMENTS);
RIGHT_PARENTHESIS : ')' -> popMode ;
TRUE : 'true' ;
FALSE : 'false' ;
NUMBER : NUMERIC_CHAR+ ;
DOT7 : '.' -> type(DOT) ;
STRING : '\'' ~('\'' | '\r' | '\n' )* '\'' ;
INTERPOLATED_STRING : '"' ~('"' | '\r' | '\n' )* '"' ;
DOLLAR7 : '$' -> type(DOLLAR) ;
EXCLAMATION7 : '!' -> type(EXCLAMATION) ;
LEFT_CURLEY7 : '{' -> type(LEFT_CURLEY);
RIGHT_CURLEY7 : '}' -> type(RIGHT_CURLEY);
LEFT_SQUARE : '[' ;
RIGHT_SQUARE : ']' ;
DOTDOT : '..' ;
ASSIGN : '=' ;
MULTIPLY : '*' ;
DIVIDE : '/' ;
MODULO : '%' ;
PLUS : '+' ;
MINUS : '-' ;
LESSTHAN : '<' | 'lt' ;
GREATERTHAN : '>' | 'gt' ;
LESSTHANOREQUAL : '<=' | 'le' ;
GREATERTHANOREQUAL : '>=' | 'ge' ;
EQUAL : '==' | 'eq' ;
NOTEQUAL : '!=' | 'ne' ;
AND : '&&' | 'and' ;
OR : '||' | 'or' ;
IDENTIFIER7 : IDENTIFIER_TEXT -> type(IDENTIFIER) ;
UNKNOWN_CHAR : . ;