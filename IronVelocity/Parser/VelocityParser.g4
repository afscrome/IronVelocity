parser grammar VelocityParser;

options {
    tokenVocab=VelocityLexer;
}

template : blockpart* ;


blockpart : text | reference;

variable : IDENTIFIER;

reference : DOLLAR SILENT? reference_invocation  
	| DOLLAR SILENT? FORMAL_START reference_invocation FORMAL_END;

reference_invocation : variable
	| reference_invocation MEMBER_INVOCATION IDENTIFIER
	| reference_invocation MEMBER_INVOCATION IDENTIFIER method_arguments;

method_arguments : METHOD_ARGUMENTS_START METHOD_ARGUMENTS_END  ;

text : (TEXT | DOLLAR | DOLLAR SILENT)+ ;