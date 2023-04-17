grammar CODE;
/* some keys my laptop cant type apparently: | \ - _ */

program:'BEGIN CODE' NEWLINE line+ 'END CODE';

line: statement+ | whileBlock;

statement: (vardec | assignment | functionCall) NEWLINE;

vardec: DATATYPE declaratorlist;

assignment: assignmentList '=' expression;

assignmentList: VARIABLENAME ('=' VARIABLENAME)*;

functionCall: FUNCTIONNAME ':' (expression (',' expression)*)?;

constant: INTVAL | FLOATVAL | CHARVAL | BOOLVAL | STRINGVAL;

declarator: VARIABLENAME | VARIABLENAME '=' expression;

declaratorlist: declarator | declarator ',' declaratorlist;

whileBlock: WHILE '(' expression ')' NEWLINE 'BEGIN WHILE' NEWLINE block NEWLINE 'END WHILE';

block: line*;

WHILE: 'WHILE' | 'UNTIL';

expression
	: constant				            # constantExpression
	| VARIABLENAME			            # variablenameExpression
	| functionCall			            # functionCallExpression
	| expression compareOp expression	# comparisonExpression
	| expression logicalOp expression	# logicalOpExpression
	| expression multOp expression	    # multiplicativeExpression
	| expression addOp expression		# additiveExpression
	| expression concatOp expression	# concatenateExpression
	| NEWLINEOP                         # newlineopExpression
	;
	
multOp: '*' | '/' | '%';
addOp: '+' | '-';
compareOp: '>' | '<' | '>=' | '<=' | '==' | '<>';
logicalOp: 'AND' | 'OR' | 'NOT';
concatOp: '&';
assgnOp: '=';

NEWLINEOP: '$';

DATATYPE: 'INT' | 'FLOAT' | 'CHAR' | 'BOOL';
INTVAL: ('-')? [1-9][0-9]*;
FLOATVAL: ('-')? [0-9]+ '.' ('-')? [0-9]+;
CHARVAL: '\'' ([a-zA-Z] | [0-9]) '\'';
BOOLVAL: 'TRUE' | 'FALSE';
STRINGVAL: ('"' ~'"'* '"') | ('\'' ~'\''* '\'') | ('[' ~']'* ']'+);

WS: [ \t\r]+ -> skip;
NEWLINE: [\r\n];
FUNCTIONNAME: 'DISPLAY' | 'SCAN';
VARIABLENAME: [_a-z][a-zA-Z0-9_]* | [a-z][a-zA-Z0-9_]*;
COMMENT: '#' ~[\r\n]* -> skip;