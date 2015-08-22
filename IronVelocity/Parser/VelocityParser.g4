parser grammar VelocityParser;

options {
    tokenVocab=VelocityLexer;
}

template : (text | reference | comment | set_directive)* EOF;

//Not sure about LEFT_CURLEY on it's own.  "LEFT_CURLEY ~IDENTIFIER" would be better
//however causes failures if there is a textual "{" followed by EOF
//RIGHT_CURLEY required to cope with ${formal}}
//DOLLAR EXCLAMATION? required to cope with "$" and "$!" without a trailing identifier
text : (TEXT | HASH | DOLLAR EXCLAMATION? | RIGHT_CURLEY | DOT | LEFT_CURLEY)+ ;

comment : HASH COMMENT | HASH block_comment;
block_comment : BLOCK_COMMENT_START (BLOCK_COMMENT_BODY | block_comment)*?  BLOCK_COMMENT_END ;

reference : DOLLAR EXCLAMATION? reference_body
	| DOLLAR EXCLAMATION? LEFT_CURLEY reference_body RIGHT_CURLEY;

reference_body : variable (DOT (property_invocation | method_invocation))* ;

variable : IDENTIFIER;
property_invocation: IDENTIFIER ;
method_invocation: IDENTIFIER LEFT_PARENTHESIS argument_list RIGHT_PARENTHESIS;

argument_list : WHITESPACE?
	| argument (COMMA argument)* ;

argument:  WHITESPACE? 
	(	 reference 
		| boolean
		| integer
		| float 
		| string
		| interpolated_string
		| list 
		| range
	) WHITESPACE?;

boolean : TRUE | FALSE ;
integer : MINUS? NUMBER ;
float: MINUS? NUMBER DOT NUMBER ;
string : STRING ;
interpolated_string : INTERPOLATED_STRING ;

list : LEFT_SQUARE argument_list RIGHT_SQUARE ;
range : LEFT_SQUARE argument  DOTDOT argument RIGHT_SQUARE ;

set_directive: WHITESPACE? HASH SET assignment RIGHT_PARENTHESIS WHITESPACE? NEWLINE? ;
assignment: WHITESPACE? reference WHITESPACE? EQUAL argument ;