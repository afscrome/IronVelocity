parser grammar VelocityParser;

options {
    tokenVocab=VelocityLexer;
}

template : block? EOF;

block: (text | reference | comment | set_directive | if_block | custom_directive_single_line | custom_directive_multi_line)* ;

//Not sure about LEFT_CURLEY on it's own.  "LEFT_CURLEY ~IDENTIFIER" would be better
//however causes failures if there is a textual "{" followed by EOF
//RIGHT_CURLEY required to cope with ${formal}}
//"DOLLAR  EXCLAMATION? LEFT_CURLEY?"" accounts for scenarios where the DOLLAR_SEEN
// lexical state was entered, but did not move into the REFERENCE state

text : (TEXT | HASH | DOLLAR (EXCLAMATION | LEFT_CURLEY)* | RIGHT_CURLEY | DOT )+ ;

comment : HASH COMMENT | HASH block_comment;
block_comment : BLOCK_COMMENT_START (BLOCK_COMMENT_BODY | block_comment)*  BLOCK_COMMENT_END ;

reference : DOLLAR EXCLAMATION? reference_body
	| DOLLAR EXCLAMATION? LEFT_CURLEY reference_body RIGHT_CURLEY;

reference_body : variable (DOT (property_invocation | method_invocation))* ;

variable : IDENTIFIER;
property_invocation: IDENTIFIER ;
method_invocation: IDENTIFIER LEFT_PARENTHESIS argument_list RIGHT_PARENTHESIS;

argument_list : WHITESPACE?
	| argument (COMMA argument)* ;

argument:  primary_expression | or_expression ;

primary_expression : WHITESPACE? 
	(reference 
		| boolean
		| integer
		| float 
		| string
		| interpolated_string
		| list
		| range 
		| parenthesised_expression
		)
	WHITESPACE? ;

boolean : TRUE | FALSE ;
integer : MINUS? NUMBER ;
float: MINUS? NUMBER DOT NUMBER ;
string : STRING ;
interpolated_string : INTERPOLATED_STRING ;

list : LEFT_SQUARE argument_list RIGHT_SQUARE ;
range : LEFT_SQUARE argument  DOTDOT argument RIGHT_SQUARE ;
parenthesised_expression : LEFT_PARENTHESIS argument RIGHT_PARENTHESIS;

set_directive: HASH SET LEFT_PARENTHESIS assignment RIGHT_PARENTHESIS;
if_block : HASH IF LEFT_PARENTHESIS argument RIGHT_PARENTHESIS block if_elseif_block* if_else_block? HASH END ;
if_elseif_block : HASH ELSEIF LEFT_PARENTHESIS argument RIGHT_PARENTHESIS block ;
if_else_block : HASH ELSE block ;

custom_directive_single_line : HASH IDENTIFIER (LEFT_PARENTHESIS argument_list RIGHT_PARENTHESIS)? 
	{$IDENTIFIER.text != "multiLine"}? ;
custom_directive_multi_line : HASH IDENTIFIER (LEFT_PARENTHESIS argument_list RIGHT_PARENTHESIS)? block  HASH END
	{$IDENTIFIER.text == "multiLine"}? ;

assignment: WHITESPACE? reference WHITESPACE? ASSIGN argument ;

unary_expression : primary_expression
	| EXCLAMATION unary_expression ;
multiplicative_expression : unary_expression
	| multiplicative_expression (MULTIPLY | DIVIDE | MODULO) unary_expression;
additive_expression : multiplicative_expression
	| additive_expression (PLUS | MINUS) multiplicative_expression;
relational_expression : additive_expression
	| relational_expression (LESSTHAN | GREATERTHAN | LESSTHANOREQUAL | GREATERTHANOREQUAL) additive_expression;
equality_expression : relational_expression
	| equality_expression (EQUAL | NOTEQUAL) relational_expression ;
and_expression : equality_expression
	| and_expression AND equality_expression ;
or_expression : and_expression
	| or_expression OR and_expression ;
