lexer grammar VelocityLexer;

tokens {
	IDENTIFIER,
}

DOLLAR : '$' ->  pushMode(REFERENCE) ;
TEXT : ~[$]+ ;

fragment IDENTIFIER : [a-zA-Z][a-zA-Z0-9]* ;



mode REFERENCE ;

VARIABLE_NAME : IDENTIFIER -> type(IDENTIFIER), mode(REFERENCE_MEMBER) ;
DOLLAR_REFERENCE : '$' -> type(DOLLAR) ;
SILENT : '!' ;
FORMAL_START : '{';
TEXT_REFERENCE : (. | '!!') -> type(TEXT), popMode ;

mode REFERENCE_MEMBER ;

MEMBER_INVOCATION : '.' ;
MEMBER_NAME : IDENTIFIER -> type(IDENTIFIER) ;
FORMAL_END : '}' -> popMode;
METHOD_ARGUMENTS_START : '(' -> pushMode(ARGUMENTS) ;

//Handle two references one after the other - e.g. "$one$two""
REFERENCE_END_DOLLAR : '$' -> type(DOLLAR), mode(REFERENCE) ;

// ".." should be treated as text, not as two MEMBER_INVOCATION tokens
REFERENCE_END_TEXT : (. | '..')  -> type(TEXT), popMode ;

mode ARGUMENTS ;
METHOD_ARGUMENTS_END : ')' -> popMode ;


