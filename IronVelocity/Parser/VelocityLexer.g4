lexer grammar VelocityLexer;

tokens {
	COMMENT,
}

fragment IDENTIFIER_TEXT : ('a'..'z' | 'A'..'Z') ('a'..'z' | 'A'..'Z' | '0'..'9' | '-' | '_'  )* ;


//===================================
// Default mode used for parsing text
// Moves to the HASH_SEEN or DOLLAR_SEEN states upon seeing '$' or '#' respectively
mode DEFAULT_MODE ;

TEXT : ~('$'| '#')+ ;
DOLLAR : '$' ->  mode(DOLLAR_SEEN) ;
HASH : '#' -> mode(HASH_SEEN) ;



//===================================
// The mode is for when a hash has been seen in a location that allows text so
// the parser can distinguish between a textual '#', comments and directives
mode HASH_SEEN ;

SINGLE_LINE_COMMENT : '#' ~('\r' | '\n')* -> type(COMMENT), mode(DEFAULT_MODE);
//Need to switch to default mode before pushing so that when the the matching end tag pops, we end back in text mode.
BLOCK_COMMENT_START : '*' -> mode(DEFAULT_MODE), pushMode(BLOCK_COMMENT) ;
DOLLAR2 : '$' ->  mode(DOLLAR_SEEN), type(DOLLAR) ;
SET : 'set(' -> mode(DEFAULT_MODE), pushMode(ARGUMENTS) ;
IF : 'if(' -> mode(DEFAULT_MODE), pushMode(ARGUMENTS) ;
ELSEIF : 'elseif(' -> mode(DEFAULT_MODE), pushMode(ARGUMENTS) ;
ELSE : 'else' -> mode(DEFAULT_MODE) ;
END : 'end' -> mode(DEFAULT_MODE) ;
TEXT2 : . -> type(TEXT), mode(DEFAULT_MODE) ;



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
// This mode should not recognise '.', otherwise you end up with funky results for things like
// "$.test"
mode DOLLAR_SEEN ;

IDENTIFIER : IDENTIFIER_TEXT -> mode(REFERENCE) ;
EXCLAMATION : '!' ;
LEFT_CURLEY : '{';
DOLLAR4 : '$' -> type(DOLLAR) ;
HASH4 : '#' -> type(HASH), mode(HASH_SEEN);
TEXT4 : . -> type(TEXT), mode(DEFAULT_MODE) ;



//===================================
// The mode is for when a dollar has been seen in a location that allows text so
// the parser can distinguish between a textual '$' and a tr
mode REFERENCE ;

DOT : '.' ;
IDENTIFIER65: IDENTIFIER_TEXT -> type(IDENTIFIER) , mode(REFERENCE_POSSIBLE_METHOD) ;
RIGHT_CURLEY : '}' ->  mode(DEFAULT_MODE);

DOLLAR5 : '$' -> type(DOLLAR), mode(DOLLAR_SEEN) ;
HASH5 : '#' -> type(HASH), mode(HASH_SEEN);
TEXT5 : (. | '..') -> type(TEXT), mode(DEFAULT_MODE) ;


//===================================
// This is identical to the DOLLAR_SEEN_VARIABLE mode, with the addition of a rule
// for '(' which transitions into the ARGUMENTS lexer state.
// This is because the left parenthesis in "$test(" is text (and shouldn't transition
// to the ARGUMENTS state), whereas in "$test.method(" it should transition.
mode REFERENCE_POSSIBLE_METHOD ;

DOT6 : '.' -> type(DOT), mode(REFERENCE);
LEFT_PARENTHESIS : '(' -> mode(REFERENCE), pushMode(ARGUMENTS) ;
RIGHT_CURLEY6 : '}' -> type(RIGHT_CURLEY),  mode(DEFAULT_MODE);

//
DOLLAR6 : '$' -> type(DOLLAR), mode(DOLLAR_SEEN) ;
HASH6 : '#' -> type(HASH), mode(HASH_SEEN);
TEXT6 : (. | '..')  -> type(TEXT), mode(DEFAULT_MODE) ;


// Three states may seem excessive for parsing references, however:
// DOLALR_SEEN and REFERENCE are seperate as otherwise the parser has problems parsing "$."
// REFERENCE and REFERENCE_MODIFIER are seperate otherwise the parser has problems parsing $test()


//===================================
// Used when parsing arguments in either a method call "$foo.bar(ARGUMENTS)",
// or a directive #directive(ARGUMENTS)
mode ARGUMENTS ;

WHITESPACE : (' ' | '\t')+ ;
COMMA : ',' ;
LEFT_PARENTHESIS7 : '(' -> type(LEFT_PARENTHESIS), pushMode(ARGUMENTS);
RIGHT_PARENTHESIS : ')' -> popMode ;
TRUE : 'true' ;
FALSE : 'false' ;
NUMBER : ('0'..'9')+ ;
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
LESSTHAN : '<' || 'lt' ;
GREATERTHAN : '>' || 'gt' ;
LESSTHANOREQUAL : '<=' || 'le' ;
GREATERTHANOREQUAL : '>=' || 'ge' ;
EQUAL : '==' | 'eq' ;
NOTEQUAL : '!=' | 'ne' ;
AND : '&&' | 'and' ;
OR : '||' | 'or' ;
IDENTIFIER7 : IDENTIFIER_TEXT -> type(IDENTIFIER) ;
UNKNOWN_CHAR : . ;