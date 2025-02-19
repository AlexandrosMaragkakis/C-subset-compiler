grammar MINIC;

/*
 * Parser Rules
 */


compileUnit : (declaration | functionDefinition)* EOF;

// handle both global variables and prototypes
declaration
    : typeSpecifier IDENTIFIER initializer? SEMICOLON
    | functionDeclaration
    ;

initializer
    : ASSIGN expression
    ;

functionDeclaration 
    : typeSpecifier IDENTIFIER LP fargs? RP SEMICOLON
    ;

functionDefinition 
    : typeSpecifier IDENTIFIER LP fargs? RP block
    ;

typeSpecifier 
    : INTEGER
    | DOUBLE
    ;

statement 
    : expression SEMICOLON
    | ifStatement
    | whileStatement
    | block
    | RETURN expression SEMICOLON
    | BREAK SEMICOLON
    | localDeclaration
    ;
    
    
localDeclaration
    : typeSpecifier IDENTIFIER (ASSIGN expression)? SEMICOLON
    ;
    
ifStatement 
    : IF LP expression RP block (ELSE block)?
    ;

whileStatement 
    : WHILE LP expression RP statement
    ;

// block must have at leasr 1 statement
block 
    : LCB statementList RCB
    ;

statementList 
    : (statement)+
    ;

expression 
    : NUMBER                              #expr_NUMBER
    | IDENTIFIER                          #expr_IDENTIFIER
    | IDENTIFIER LP args RP               #expr_FCALL
    | NOT expression                      #expr_NOT
    | expression op=(DIV|MULT) expression #expr_DIVMULT
    | expression op=(PLUS|MINUS) expression #expr_PLUSMINUS
    | PLUS expression                     #expr_PLUS
    | MINUS expression                    #expr_MINUS
    | LP expression RP                    #expr_PARENTHESIS
    | IDENTIFIER ASSIGN expression        #expr_ASSIGN
    | expression AND expression           #expr_AND // AND kai OR prepei na einai pio katw apo GT GTE kai loipa
    | expression OR expression            #expr_OR
    | expression GT expression            #expr_GT
    | expression GTE expression           #expr_GTE
    | expression LT expression            #expr_LT
    | expression LTE expression           #expr_LTE
    | expression EQUAL expression         #expr_EQUAL
    | expression NEQUAL expression        #expr_NEQUAL
    ;
    
/* chatgpt suggested this:
expression 
   : logicalOrExpression
   ;

logicalOrExpression 
   : logicalAndExpression (OR logicalAndExpression)* 
   ;

logicalAndExpression 
   : equalityExpression (AND equalityExpression)* 
   ;

equalityExpression 
   : relationalExpression ((EQUAL|NEQUAL) relationalExpression)* 
   ;

relationalExpression
   : additiveExpression ((GT|GTE|LT|LTE) additiveExpression)* 
   ;

additiveExpression
   : multiplicativeExpression ((PLUS|MINUS) multiplicativeExpression)* 
   ;

multiplicativeExpression
   : unaryExpression ((MULT|DIV) unaryExpression)* 
   ;

unaryExpression
   : (PLUS|MINUS|NOT) unaryExpression
   | LP expression RP
   | IDENTIFIER LP args RP
   | IDENTIFIER ASSIGN expression
   | IDENTIFIER
   | NUMBER
   ;

*/

args 
    : (expression (COMMA expression)*)?
    ;

fargs 
    : (typeSpecifier IDENTIFIER (COMMA typeSpecifier IDENTIFIER)* )?
    ;

/*
 * Lexer Rules
 */

// Reserved words
INTEGER : 'int' ;
DOUBLE  : 'double' ;
IF      : 'if' ;
ELSE    : 'else' ;
WHILE   : 'while' ;
RETURN  : 'return' ;
BREAK   : 'break' ;

// Operators
PLUS     : '+'; 
MINUS    : '-';
DIV      : '/'; 
MULT     : '*';
OR       : '||';
AND      : '&&';
NOT      : '!';
EQUAL    : '==' ;
NEQUAL   : '!=' ;
GT       : '>';
LT       : '<';
GTE      : '>=' ;
LTE      : '<=' ;
SEMICOLON: ';';
LP       : '(';
RP       : ')';
LCB      : '{';
RCB      : '}'; 
COMMA    : ',';
ASSIGN   : '=';

// Identifiers - numbers
IDENTIFIER : [a-zA-Z_][a-zA-Z0-9_]* ;
NUMBER     : '0' | [1-9][0-9]*;

// Ignore whitespaces
WS : [ \r\t\n]+ -> skip;    

// Ignore comments
COMMENT : '//' ~[\r\n]* -> channel(HIDDEN);
