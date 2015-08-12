parser grammar VelocityParser;

options {
    tokenVocab=VelocityLexer;
}

template : (text | reference)* ;

//Not sure about FORMAL_START on it's own.  "FORMAL_START ~IDENTIFIER" would be better
//however causes failures if there is a textual "{" followed by EOF

text : (TEXT | DOLLAR SILENT? | FORMAL_END | MEMBER_INVOCATION | FORMAL_START)+ ;


variable : IDENTIFIER;

reference : informal_reference
	| formal_reference;

informal_reference : DOLLAR SILENT? reference_body  ;
formal_reference : DOLLAR SILENT? FORMAL_START reference_body FORMAL_END;

reference_body : variable 
	| reference_body  MEMBER_INVOCATION property_invocation
	| reference_body  MEMBER_INVOCATION method_invocation ;

property_invocation: IDENTIFIER ;
method_invocation: IDENTIFIER method_arguments ;

method_arguments : METHOD_ARGUMENTS_START METHOD_ARGUMENTS_END  ;

//FORMAL_END required to cope with ${formal}}
//DOLLAR SILENT? required to cope with "$" and "$!" without a trailing identifier