parser grammar VelocityParser;

options {
    tokenVocab=VelocityLexer;
}

template : (text | reference | comment)* ;

//Not sure about LEFT_CURLEY on it's own.  "LEFT_CURLEY ~IDENTIFIER" would be better
//however causes failures if there is a textual "{" followed by EOF
//RIGHT_CURLEY required to cope with ${formal}}
//DOLLAR EXCLAMATION? required to cope with "$" and "$!" without a trailing identifier
text : (TEXT | HASH | DOLLAR EXCLAMATION? | RIGHT_CURLEY | DOT | LEFT_CURLEY)+ ;

comment : HASH COMMENT | HASH block_comment;
block_comment : BLOCK_COMMENT_START (BLOCK_COMMENT_BODY | block_comment)*?  BLOCK_COMMENT_END ;

variable : IDENTIFIER;

reference : informal_reference
	| formal_reference;

informal_reference : DOLLAR EXCLAMATION? reference_body  ;
formal_reference : DOLLAR EXCLAMATION? LEFT_CURLEY reference_body RIGHT_CURLEY;

reference_body : variable 
	| reference_body DOT property_invocation
	| reference_body DOT method_invocation ;

property_invocation: IDENTIFIER ;
method_invocation: IDENTIFIER method_arguments ;

method_arguments : LEFT_PARENTHESIS RIGHT_PARENTHESIS  ;

