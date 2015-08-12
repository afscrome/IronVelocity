parser grammar VelocityParser;

options {
    tokenVocab=VelocityLexer;
}

template : (text | reference)* ;

text : (TEXT | DOLLAR SILENT? | FORMAL_END | MEMBER_INVOCATION)+ ;


variable : IDENTIFIER;

reference : informal_reference
	| formal_reference;

informal_reference : DOLLAR SILENT? reference_body  ;
formal_reference : DOLLAR SILENT? FORMAL_START reference_body FORMAL_END;

reference_body : variable 
	| reference_body  MEMBER_INVOCATION property_invocation ;
//	| reference_body  MEMBER_INVOCATION method_invocation ;

property_invocation: IDENTIFIER ;
//method_invocation: IDENTIFIER method_arguments ;

//method_arguments : METHOD_ARGUMENTS_START METHOD_ARGUMENTS_END  ;

//FORMAL_END required to cope with ${formal}}
//DOLLAR SILENT? required to cope with "$" and "$!" without a trailing identifier