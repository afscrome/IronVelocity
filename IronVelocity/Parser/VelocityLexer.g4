lexer grammar VelocityLexer;

tokens {
	COMMENT,
}

fragment IDENTIFIER_TEXT : [a-zA-Z][a-zA-Z0-9]* ;
fragment NEWLINE : '\r' | '\n' | '\r\n' ;

DOLLAR : '$' ->  mode(REFERENCE) ;
HASH : '#' -> mode(HASH_SEEN) ;
TEXT : ~('$'| '#')+ ;

//===================================
//The mode is for when a hash has been seen in a location that allows text so
// the parser can distinguish between a textual '#', comments and directives
mode HASH_SEEN ;

SINGLE_LINE_COMMENT : '#' ~('\r' | '\n')* NEWLINE? -> type(COMMENT), mode(DEFAULT_MODE);
//Need to switch to default mode before pushing so that when the the matching end tag pops, we end back in text mode.
BLOCK_COMMENT_START : '*' -> mode(DEFAULT_MODE), pushMode(BLOCK_COMMENT) ;
DOLLAR2 : '$' ->  mode(REFERENCE), type(DOLLAR) ;
DIRECTIVE_TEXT : . -> type(TEXT), mode(DEFAULT_MODE) ;


//===================================
// In Velocity, block comments can be nested.  i.e. the string "#* #*comment*# *#" is fully a comment
// In c style languages, only up to the first "*#" is typically considered a comment
// Because of this, we need match start & close comment tokens by pushing & poping the mode.
mode BLOCK_COMMENT ;

BLOCK_COMMENT_START2 : '#*' -> pushMode(BLOCK_COMMENT), type(BLOCK_COMMENT_START);
BLOCK_COMMENT_END : '*#' -> popMode ;
BLOCK_COMMENT_BODY :  (~('#' | '*') | '#' ~'*' | '*' ~'#')* ;


//===================================
mode REFERENCE ;

IDENTIFIER : IDENTIFIER_TEXT -> mode(REFERENCE_POSSIBLE_MEMBER) ;
HASH2 : '#' -> mode(HASH_SEEN), type(HASH);
DOLLAR3 : '$' -> type(DOLLAR) ;
EXCLAMATION : '!' ;
LEFT_CURLEY : '{' -> mode(REFERENCE_FORMAL);
// "$!!" should be considered as text
TEXT2 : (. | '!!') -> type(TEXT), mode(DEFAULT_MODE) ;


//===================================
// We have what looks like a formal reference ("${" or "$!{")
// If we have an identifier, then continue with the formal reference
// otherwise treat what we have so far as text.
mode REFERENCE_FORMAL ;

IDENTIFIER2 : IDENTIFIER_TEXT -> type(IDENTIFIER), mode(REFERENCE_POSSIBLE_MEMBER) ;
DOLLAR4 : '$' -> type(DOLLAR), mode(REFERENCE) ;
HASH3 : '#' -> mode(HASH_SEEN), type(HASH);
TEXT3 : . -> type(TEXT), mode(DEFAULT_MODE) ;


//===================================
mode REFERENCE_POSSIBLE_MEMBER ;

DOT : '.' ;
IDENTIFIER3 : IDENTIFIER_TEXT -> type(IDENTIFIER) , mode(REFERENCE_MEMBER) ;
RIGHT_CURLEY : '}' -> mode(DEFAULT_MODE);

//Handle two references one after the other - e.g. "$one$two""
DOLLAR5 : '$' -> type(DOLLAR), mode(REFERENCE) ;
HASH4 : '#' -> mode(HASH_SEEN), type(HASH);

// ".." should be treated as text, not as two DOT tokens
TEXT4 : (. | '..')  -> type(TEXT), mode(DEFAULT_MODE) ;

//===================================
//At this point we have a certain member invocation (e.g. "$foo.bar"), but are not yet sure what kind of member invocation it is
// * Method call if followed by "(" (e.g. "foo.bar(" )
// * Another reference expression if followed by "$" (E.g. "$foo.bar$")
// * If followed by "." (but not ".."), need to go back to REFERENCE_POSSIBLE_MEMBER (e.g. "$foo.bar.")
// * A property if followed by text, or in a formal reference, and followed by "}" (e.g. "$foo.bar%", "${foo.bar}).
mode REFERENCE_MEMBER ;

DOT2 : '.' -> type(DOT), mode(REFERENCE_POSSIBLE_MEMBER) ;
LEFT_PARENTHESIS : '(' -> pushMode(ARGUMENTS) ;
RIGHT_CURLEY2 : '}' -> type(RIGHT_CURLEY), mode(DEFAULT_MODE);
DOLLAR6 : '$' -> type(DOLLAR), mode(REFERENCE) ;
HASH5 : '#' -> mode(HASH_SEEN), type(HASH);
TEXT5 : (. | '..')  -> type(TEXT), mode(DEFAULT_MODE) ;

mode ARGUMENTS ;

RIGHT_PARENTHESIS : ')' -> popMode ;
