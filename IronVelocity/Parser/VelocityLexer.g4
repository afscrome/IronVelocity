lexer grammar VelocityLexer;

tokens {
	COMMENT,
}

fragment IDENTIFIER_TEXT : ('a'..'z' | 'A'..'Z') ('a'..'z' | 'A'..'Z' | '0'..'9' | '-' | '_'  )* ;
fragment NEWLINE_CHARS : '\r' | '\n' | '\r\n' ;


//===================================
// Text mode
mode DEFAULT_MODE ;

DOLLAR : '$' ->  mode(DOLLAR_SEEN) ;
HASH : '#' -> mode(HASH_SEEN) ;
TEXT : ~('$'| '#')+ ;
NEWLINE : NEWLINE_CHARS;

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
DIRECTIVE_TEXT : . -> type(TEXT), mode(DEFAULT_MODE) ;

//===================================
// In Velocity, block comments can be nested.  i.e. the string "#* #*comment*# *#" is fully a comment
// In c style languages, only up to the first "*#" is typically considered a comment
// Because of this, we need match start & close comment tokens by pushing & poping the mode.
mode BLOCK_COMMENT ;

BLOCK_COMMENT_START3 : '#*' -> pushMode(BLOCK_COMMENT), type(BLOCK_COMMENT_START);
BLOCK_COMMENT_END : '*#' -> popMode ;
BLOCK_COMMENT_BODY :  (~('#' | '*') | '#' ~'*' | '*' ~'#')+ ;


//===================================
mode DOLLAR_SEEN ;

IDENTIFIER : IDENTIFIER_TEXT -> mode(REFERENCE) ;
EXCLAMATION : '!' ;
//LEFT_CURLEY : '{' -> mode(POSSIBLE_FORMAL_REFERENCE);
LEFT_CURLEY : '{';
// "$!!" should be considered as text
//Escape
HASH4 : '#' -> mode(HASH_SEEN), type(HASH);
DOLLAR4 : '$' -> type(DOLLAR) ;
TEXT4 : (. | '!!') -> type(TEXT), mode(DEFAULT_MODE) ;


//===================================
// We have what looks like a formal reference ("${" or "$!{")
// If we have an identifier, then switch to REFERENCE mode,
// otherwise switch back to text (default) mode.
// This is needed by the parser so that the "${" in "${$var" is considered text
//TODO: Would like to remove this mode completely - think it can be done
//mode POSSIBLE_FORMAL_REFERENCE ;

//IDENTIFIER5 : IDENTIFIER_TEXT -> type(IDENTIFIER), mode(REFERENCE) ;

//Escape
//DOLLAR5 : '$' -> type(DOLLAR), mode(DOLLAR_SEEN) ;
//HASH5 : '#' -> mode(HASH_SEEN), type(HASH);
//TEXT5 : . -> type(TEXT), mode(DEFAULT_MODE) ;


//===================================
mode REFERENCE ;

DOT : '.' ;
IDENTIFIER6 : IDENTIFIER_TEXT -> type(IDENTIFIER) , mode(REFERENCE_MEMBER) ;

//No Longer a Reference
RIGHT_CURLEY : '}' -> mode(DEFAULT_MODE);
DOLLAR6 : '$' -> type(DOLLAR), mode(DOLLAR_SEEN) ;
HASH6 : '#' -> mode(HASH_SEEN), type(HASH);
TEXT6 : (. | '..') -> type(TEXT), mode(DEFAULT_MODE) ;

//===================================
// When  we have a certain member invocation (e.g. "$foo.bar"), but are not yet sure what kind of member invocation it is
// * Method call if followed by "(" (e.g. "foo.bar(" )
// * Another reference expression if followed by "$" (E.g. "$foo.bar$")
// * If followed by "." (but not ".."), need to go back to REFERENCE (e.g. "$foo.bar.")
// * A property if followed by text, or in a formal reference, and followed by "}" (e.g. "$foo.bar%", "${foo.bar}).
mode REFERENCE_MEMBER ;

DOT7 : '.' -> type(DOT), mode(REFERENCE) ;
LEFT_PARENTHESIS : '(' -> pushMode(ARGUMENTS) ;
RIGHT_CURLEY7 : '}' -> type(RIGHT_CURLEY), mode(DEFAULT_MODE);
DOLLAR7 : '$' -> type(DOLLAR), mode(DOLLAR_SEEN) ;
HASH7 : '#' -> mode(HASH_SEEN), type(HASH);
TEXT7 : (. | '..')  -> type(TEXT), mode(DEFAULT_MODE) ;

//===================================
// Used when parsing arguments in either a method call "$foo.bar(ARGUMENTS)",
// or a directive #directive(ARGUMENTS)
mode ARGUMENTS ;

WHITESPACE : (' ' | '\t')+ ;
COMMA : ',' ;
LEFT_PARENTHESIS2 : '(' -> type(LEFT_PARENTHESIS), pushMode(ARGUMENTS);
RIGHT_PARENTHESIS : ')' -> popMode ;
TRUE : 'true' ;
FALSE : 'false' ;
MINUS : '-' ;
NUMBER : ('0'..'9')+ ;
DOT8 : '.' -> type(DOT) ;
STRING : '\'' ~('\'')* '\'' ;
INTERPOLATED_STRING : '"' ~('"')* '"' ;
DOLLAR8 : '$' -> type(DOLLAR) ;
IDENTIFIER4 : IDENTIFIER_TEXT -> type(IDENTIFIER) ;
EXCLAMATION8 : '!' -> type(EXCLAMATION) ;
LEFT_CURLEY8 : '{' -> type(LEFT_CURLEY);
RIGHT_CURLEY8 : '}' -> type(RIGHT_CURLEY);
LEFT_SQUARE : '[' ;
RIGHT_SQUARE : ']' ;
DOTDOT : '..' ;
EQUAL : '=' ;