lexer grammar VelocityLexer;

tokens {
	IDENTIFIER,
}

DOLLAR : '$' ->  pushMode(REFERENCE) ;
TEXT : ~[$]+ ;

fragment IDENTIFIER : [a-zA-Z][a-zA-Z0-9]* ;



mode REFERENCE ;

VARIABLE_NAME : IDENTIFIER -> type(IDENTIFIER) ;
DOLLAR_REFERENCE : '$' -> type(DOLLAR) ;
SILENT : '!' ;
FORMAL_START : '{';
FORMAL_END : '}' -> popMode;
TEXT_REFERENCE : (. | '!!') -> type(TEXT), popMode ;


//Not sure about this??


mode REFERENCE_MEMBER ;

MEMBER_INVOCATION : '.' ;
METHOD_ARGUMENTS_START : '(' ;
METHOD_ARGUMENTS_END : ')' ;
TEXT_REFERENCE_MEMBER : . -> type(TEXT), popMode ;
MEMBER_NAME : IDENTIFIER -> type(IDENTIFIER) ;
