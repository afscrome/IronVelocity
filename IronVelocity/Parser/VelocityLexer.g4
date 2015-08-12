lexer grammar VelocityLexer;

tokens {
	IDENTIFIER,
}

DOLLAR : '$' ->  pushMode(REFERENCE) ;
TEXT : ~[$]+ ;

fragment IDENTIFIER : [a-zA-Z][a-zA-Z0-9]* ;



//===================================
mode REFERENCE ;

VARIABLE_NAME : IDENTIFIER -> type(IDENTIFIER), mode(REFERENCE_POSSIBLE_MEMBER) ;
DOLLAR_REFERENCE : '$' -> type(DOLLAR) ;
SILENT : '!' ;
FORMAL_START : '{';
// "$!!" should be considered as text
TEXT_REFERENCE : (. | '!!') -> type(TEXT), popMode ;

//===================================
mode REFERENCE_POSSIBLE_MEMBER ;

MEMBER_INVOCATION : '.' ;
MEMBER_NAME : IDENTIFIER -> type(IDENTIFIER) , mode(REFERENCE_MEMBER) ;
FORMAL_END : '}' -> popMode;

//Handle two references one after the other - e.g. "$one$two""
REFERENCE_POSSIBLE_MEMBER_DOLLAR : '$' -> type(DOLLAR), mode(REFERENCE) ;

// ".." should be treated as text, not as two MEMBER_INVOCATION tokens
REFERENCE_POSSIBLE_MEMBER_TEXT : (. | '..')  -> type(TEXT), popMode ;

//===================================
//At this point we have a certain member invocation (e.g. "$foo.bar"), but are not yet sure what kind of member invocation it is
// * Method call if followed by "(" (e.g. "foo.bar(" )
// * Another reference expression if followed by "$" (E.g. "$foo.bar$")
// * If followed by "." (but not ".."), need to go back to REFERENCE_POSSIBLE_MEMBER (e.g. "$foo.bar.")
// * A property if followed by text, or in a formal reference, and followed by "}" (e.g. "$foo.bar%", "${foo.bar}).
mode REFERENCE_MEMBER ;

REFERENCE_MEMBER_MEMBER_INVOCATION : '.' -> type(MEMBER_INVOCATION), mode(REFERENCE_POSSIBLE_MEMBER) ;
METHOD_ARGUMENTS_START : '(' -> pushMode(ARGUMENTS) ;
REFERENCE_MEMBER_FORMAL_END : '}' -> popMode, type(FORMAL_END);
REFERENCE_MEMBER_DOLLAR : '$' -> type(DOLLAR), mode(REFERENCE) ;
REFERENCE_MEMBER_TEXT : (. | '..')  -> type(TEXT), popMode ;

mode ARGUMENTS ;
METHOD_ARGUMENTS_END : ')' -> popMode ;


