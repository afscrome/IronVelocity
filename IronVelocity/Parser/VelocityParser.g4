parser grammar VelocityParser;

options {
    tokenVocab=VelocityLexer;
}

template: block
	(
		EOF
		| (HASH END {NotifyErrorListeners("Unexpected #end"); } template)
	) ;

block: (text | reference | comment | set_directive | if_block | custom_directive )* ;

//"DOLLAR  (EXCLAMATION | LEFT_CURLEY)*"" accounts for scenarios where the DOLLAR_SEEN
// lexical state was entered, but did not move into the REFERENCE state
// RIGHT_CURLEY required to cope with ${formal}}
// DOT required to cope with "$name."
text : (TEXT | HASH | DOLLAR (EXCLAMATION | LEFT_CURLEY)* | RIGHT_CURLEY | DOT )+ ;

comment : HASH COMMENT | HASH block_comment;
block_comment : BLOCK_COMMENT_START (BLOCK_COMMENT_BODY | block_comment)*  BLOCK_COMMENT_END ;

reference : DOLLAR EXCLAMATION? reference_body
	| DOLLAR EXCLAMATION? LEFT_CURLEY reference_body RIGHT_CURLEY;

reference_body : variable (DOT ( method_invocation | property_invocation))* ;

variable : IDENTIFIER;
property_invocation: IDENTIFIER ;
method_invocation: IDENTIFIER LEFT_PARENTHESIS argument_list RIGHT_PARENTHESIS;

argument_list : (expression (COMMA expression)*)? ;

directive_arguments: (LEFT_PARENTHESIS directive_argument* RIGHT_PARENTHESIS)? ;
directive_argument : expression | directive_word;
directive_word : IDENTIFIER;
primary_expression : reference 
	| boolean
	| float
	| integer
	| string
	| interpolated_string
	| list
	| range 
	| parenthesised_expression
	;

boolean : TRUE | FALSE ;
integer : MINUS? NUMBER ;
float: MINUS? NUMBER DOT NUMBER ;
string : STRING ;
interpolated_string : INTERPOLATED_STRING ;

list : LEFT_SQUARE argument_list RIGHT_SQUARE ;
range : LEFT_SQUARE expression  DOTDOT expression RIGHT_SQUARE ;
parenthesised_expression : LEFT_PARENTHESIS expression RIGHT_PARENTHESIS;

set_directive: HASH SET LEFT_PARENTHESIS assignment RIGHT_PARENTHESIS;
if_block : HASH IF LEFT_PARENTHESIS expression RIGHT_PARENTHESIS block if_elseif_block* if_else_block? HASH END ;
if_elseif_block : HASH ELSEIF LEFT_PARENTHESIS expression RIGHT_PARENTHESIS block ;
if_else_block : HASH ELSE block ;


custom_directive :
	{ !IsBlockDirective()}?  HASH IDENTIFIER directive_arguments 
	 |  {IsBlockDirective()}? HASH IDENTIFIER directive_arguments block  HASH END ;

assignment: reference ASSIGN expression ;

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
expression : and_expression
	| expression OR and_expression ;
