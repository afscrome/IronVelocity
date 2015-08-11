lexer grammar VelocityLexer;

tokens {
	IDENTIFIER,
}
DOT : '.' ;
DOLLAR : '$' ->  pushMode(REFERENCE) ;
TEXT : ~[$]+ ;

fragment IDENTIFIER : [a-zA-Z][a-zA-Z0-9]* ;



mode REFERENCE ;

VARIABLE_NAME : IDENTIFIER -> mode(REFERENCE_MEMBER), type(IDENTIFIER) ;
DOLLAR_REFERENCE : '$' -> type(DOLLAR) ;
SILENT : '!' ;
TEXT_REFERENCE : (. | '!!') -> type(TEXT), popMode ;

//Not sure about this??
FORMAL_START : '{' -> pushMode(REFERENCE) ;
FORMAL_END : '}' -> popMode;


mode REFERENCE_MEMBER ;

MEMBER_NAME : IDENTIFIER -> type(IDENTIFIER) ;
MEMBER_INVOCATION : '.' ;
METHOD_ARGUMENTS_START : '(' ;
METHOD_ARGUMENTS_END : ')' ;
TEXT_REFERENCE_MEMBER : . -> type(TEXT), popMode ;
