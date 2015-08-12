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
//At this point we have what looks like a property call (e.g. "$foo.bar").  But it may not be a property call
// * Could be the end of a formal reference (e.g. "${foo.bar}")
// * Could be followed by text (e.g. "$foo.bar ", "$foo.bar!")
// * Could become a method call (e.g. "$foo.bar(")
// * Could have a further member invocation (e.g. "$foo.bar.")
// * Could be followed by another reference (E.g. "$foo.bar$")
mode REFERENCE_MEMBER ;

REFERENCE_MEMBER_MEMBER_INVOCATION : '.' -> mode(REFERENCE_POSSIBLE_MEMBER) ;
METHOD_ARGUMENTS_START : '(' -> pushMode(ARGUMENTS) ;
REFERENCE_MEMBERFORMAL_END2 : '}' -> popMode, type(FORMAL_END);


mode ARGUMENTS ;
METHOD_ARGUMENTS_END : ')' -> popMode ;


