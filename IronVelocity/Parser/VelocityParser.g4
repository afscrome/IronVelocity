parser grammar VelocityParser;

options {
    tokenVocab=VelocityLexer;
}

template : (text | reference | comment)* ;

//Not sure about FORMAL_START on it's own.  "FORMAL_START ~IDENTIFIER" would be better
//however causes failures if there is a textual "{" followed by EOF
//FORMAL_END required to cope with ${formal}}
//DOLLAR SILENT? required to cope with "$" and "$!" without a trailing identifier
text : (TEXT | HASH | DOLLAR SILENT? | FORMAL_END | MEMBER_INVOCATION | FORMAL_START)+ ;

comment : HASH COMMENT | HASH block_comment;
block_comment : BLOCK_COMMENT_START (BLOCK_COMMENT_BODY | block_comment)*?  BLOCK_COMMENT_END ;

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

