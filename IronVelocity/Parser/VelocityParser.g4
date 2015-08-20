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

reference : informal_reference
	| formal_reference;

informal_reference : DOLLAR EXCLAMATION? reference_body  ;
formal_reference : DOLLAR EXCLAMATION? LEFT_CURLEY reference_body RIGHT_CURLEY;

reference_body : variable 
	| reference_body DOT property_invocation
	| reference_body DOT method_invocation ;

variable : IDENTIFIER;
property_invocation: IDENTIFIER ;
method_invocation: IDENTIFIER method_argument_list ;

method_argument_list : LEFT_PARENTHESIS arguments RIGHT_PARENTHESIS  ;

arguments : WHITESPACE?
	| WHITESPACE? argument WHITESPACE?
	| argument COMMA arguments ;

argument: reference 
	| boolean
	| integer
	| float 
	| string
	| interpolated_string
	| list 
	| range ;

boolean : TRUE | FALSE ;
integer : MINUS? NUMBER ;
float: MINUS? NUMBER DOT NUMBER ;
string : STRING ;
interpolated_string : INTERPOLATED_STRING ;

list : LEFT_SQUARE arguments RIGHT_SQUARE ;
//TODO: can we reuse WHITESPACE? argument WHITESPACE?
range : LEFT_SQUARE WHITESPACE? argument WHITESPACE? DOTDOT WHITESPACE? argument WHITESPACE? RIGHT_SQUARE ;