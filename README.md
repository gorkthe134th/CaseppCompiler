# Case++Compiler

A compiler for the made-up programming language Case++.<br>

## Description

An in-development compiler created as part of a university course.<br>
A minimum required language specification was provided by the associated professor, but I have also expanded the syntax and supported additional features when I deemed it appropriate.<br>
The full language documentation can be viewed bellow.<br>
The compiler's output language will be RISC-V assembly.<br>
Current functionality is limited to Lexical and Syntactical Analysis.<br>
The input file encoding is also currently restricted to UTF-8.<br>

## Case++ Documentation

Case++ is an imperative procedural programming language, with C-like syntax, designed to fit this exercise.<br>
It only supports a single integer type (no floating point or data structures) and limited control structures.<br>
The syntax is also limited in order to ensure that the grammar of the language is LL(1).<br>
Case++ files are recommended to have a "c++" file extension, without that being necessary.<br>
Whitespace characters like line changes, indentations and extra spaces are ignored.<br>

### Comments

Comments are denoted with "//" or "/\*" and "\*/".<br>
```
// Single Line Comment
/*
Multi Line Comment
*/
```
Any characters in the same line and after "//" are ignored.<br>
Any characters between "/\*" and "\*/" are ignored.<br>
Nested Comments are not taken into account.<br>
```
/* Inside of Comment /* Inside of Comment */ Outside of Comment */
```

### Program

Every Case++ file must be entirely comprised of a single Program.<br>
A Program is defined using the keyword "program", the program id, and a block of Declarations, Functions, and / or Statements, enclosed by curly brackets.<br>
```
program ExampleProgram
{
	print 72;
}
```
The program id is not required to have any relation to the file name.<br>
Declarations, Functions and Statements are required to be defined in that order, but not all three, if any, need to be present.<br>

### Declarations

Declarations are made using the keyword "declare" and any number of variable ids, ending with a semi-colon.<br>
```
program Declarations
{
	declare a;
	declare x,y,z;
	declare;
}
```
Variable ids are separated by commas.<br>
Every variable is an integer, so no type information is needed.<br>
Extra semi-colons and declarations with no variable ids are ignored.<br>

### Statements

Statements must be located after every declaration and function in their block.<br>
Statements are separated by semi-colons.<br>
```
program Statements
{
	declare x,y,z;
	x := 1;;
	y := 2;
	z := 3
}
```
The last statement of a block is allowed to end with a semi-colon, but it is not required.<br>
Any other extra semi-colons are also ignored.<br>
In place of a statement, a block can be inserted, containing local variables and functions for that block.<br>
```
program Blocks
{
	declare x;
	{
		declare y;
		// Can use both x and y here
		y := 1 + 2 + 3;
		x := y * y;
	};
	// Can only use x here
	x := x + 1;
}
```
When this statement is executed, all statements contained in the block will be executed, in order.<br>
Blocks, like any other statement, must be separated by a semi-colon from it's succeeding statement, if any.<br>

### Assignment

Assignment is performed using ":=". The left side must be a variable id and the right side must be an expression.<br>
```
x := 1
```

### Expressions

Expressions are allowed to use the four basic operations (+,-,\*,/) and parentheses, on top of constants and variables.<br>
```
x := t*a + (1-t)*(-y/2)
```
Parentheses are processed before Multiplication and Division, which are processed before Addition and Subtraction.<br>
Consecutive Multiplications and Divisions / Additions and Subtractions are processed from left to right.<br>
The unary operators "+" and "-" are allowed only at the beginning of an expression.<br>
```
x := 1 + -y // Not allowed
```
Constants must be in the range [-32767, 32767].
```
x := 32768 // Not allowed
```
The result of all operations is an integer.<br>

### Input-Output

Input can be taken from the user of the end program using the "input" statement.<br>
```
input x
```
The "input" keyword can only be followed by a variable id.<br>
<br>
Output can be given to the user of the end program using the "print" statement.<br>
```
print x + y/2
```
The "print" keyword can be followed by any expression.<br>

### Control Structures

#### Conditions

Conditions are allowed to use the two basic operations (or, and) and square brackets, on top of constants and comparisons.<br>
```
t < 1 and false or not[t < 1] and [not[y = 2] and true]
```
Square brackets are processed before Logical And, which is processed before Logical Or.<br>
The unary operator "not" is allowed only before a square bracket.<br>
```
not false or y <> 2 // Not allowed
```
Comparisons are allowed to use the six basic operations (=,>,<,<>,<=,>=), between any two expressions.<br>
```
47*x-x*x >= y/x+y*x
```

#### If

When this statement is executed, the condition after the "if" keyword will be checked and the subsequent statement will only be executed if the condition is true.<br>
```
if x > 10 x := x - 10
```
If the subsequent statement is a block, all statements in that block will be executed.<br>
```
if x > 10
{
	declare y;
	y := x - 10;
	x := y * y;
}
```
The "else" keyword may follow that statement, indicating that *its* subsequent statement will be executed only if the condition is false.<br>
```
if x/2*2=x x:=x/2 else x:=3*x+1
```
The statement between the "if" and "else" keywords must not end with a semi-colon.<br>
The whole if statement, though, like any other statement, must be separated by a semi-colon from it's succeeding statement, if any.<br>
```
if x/2*2=x
	x:=x/2; // Not allowed
else
	x:=3*x+1; // Required
print x
```

#### While

When this statement is executed, the condition after the "while" keyword will be checked and the subsequent statement will only be executed if the condition is true.<br>
If the condition was true, it will be checked again, after the statement is executed, and, if it is still true, the statement will be executed again.<br>
This process repeats while the condition is true.<br>
```
while x > 10 x := x - 10
```
If the subsequent statement is a block, all statements in that block will be executed with each iteration.<br>
```
while x < 100
{
	declare y;
	y := x - 10;
	x := y * y;
}
```
The whole while statement, like any other statement, must be separated by a semi-colon from it's succeeding statement, if any.<br>

#### Switchcase

When this statement is executed, the conditions after each "when" keyword between the "switchcase" and "default" keywords will be checked, in order, and only the statement after the first condition that is true will be executed.<br>
If none of the conditions are true, the statement after the "default" keyword is executed.<br>
```
switchcase
	when x > 10: x := x - 10
	when x < 0 : x := x + 10;
default: print 89
```
Every statement must be preceded by a colon and may be succeeded by a semi-colon.<br>
Conditions after the first one that was true are not evaluated.<br>
The whole switchcase statement, like any other statement, must be separated by a semi-colon from it's succeeding statement, if any.<br>

#### Whilecase

When this statement is executed, the conditions after each "when" keyword between the "whilecase" and "default" keywords will be checked, in order, and only the statement after the first condition that is true will be executed.<br>
After a statement is executed, the conditions are checked again, in the same order, repeating while at least one condition is true. When no conditions are true, the execution of the program is continued normally from the statement after the "default" keyword.<br>
```
whilecase
	when x > 10:
	{
		x := x - 10;
		c := c + 1
	};
	when x < 0:
	{
		x := x + 10;
		c := c + 1;
	}
default: print x
```
Every statement must be preceded by a colon and may be succeeded by a semi-colon.<br>
Conditions after the first one that was true are not evaluated for that iteration.<br>
The whole whilecase statement, like any other statement, must be separated by a semi-colon from it's succeeding statement, if any.<br>

#### Untilcase

When this statement is executed, the conditions after each "when" keyword will be checked, in order, and the statement after EVERY condition that is true will be executed.<br>
After all conditions are checked, the condition after the "until" keyword is checked, with the execution of the program continuing after the untilcase statement if it is true.<br>
If it is false, all conditions are checked again, in the same order, repeating until the "until" condition is true.<br>
```
x := 0;
y := 0;
untilcase
	when x = y:
	{
		input x;
		input y;
	};
	when x < 0: x := 0;
	when y < 0: y := 0;
	when x < y:
	{
		y := y - x;
		print y;
	}
	when y < x:
	{
		x := x - y;
		print x;
	}
until x = 0 and y = 0
```
Every statement must be preceded by a colon and may be succeeded by a semi-colon.<br>
The whole untilcase statement, like any other statement, must be separated by a semi-colon from it's succeeding statement, if any.<br>

#### Incase

When this statement is executed, the conditions after each "when" keyword will be checked, in order, and the statement after EVERY condition that is true will be executed.<br>
In case AT LEAST ONE condition was true, the conditions are checked again, in the same order, repeating until no condition is true.<br>
```
input x;
input y;
incase
	when x > y: x := x - y
	when x < y: y := y - x;
print x;
```
Every statement must be preceded by a colon and must NOT be succeeded by a semi-colon, except for the last one.<br>
The whole incase statement, like any other statement, must be separated by a semi-colon from it's succeeding statement, if any.<br>

#### Forcase

When this statement is executed, the expression after the equals sign is evaluated and the following steps will be repeated that many times:<br>
* The specified variable will be set to current iteration number, i.e. "1" for the first iteration, "2" for the second, etc.
* The conditions after each "when" keyword will be checked, in order, and the statement after EVERY condition that is true will be executed.
```
input x;
input y;
forcase i = 100
	when true: print i
	when i/x*x=i: print x
	when i/y*y=i: print y
```
The expression that determines the number of iterations is only evaluated once, before any iterations are performed.<br>
The conditions are not required to contain the iteration variable.<br>
Every statement must be preceded by a colon and must NOT be succeeded by a semi-colon, except for the last one.<br>
The whole forcase statement, like any other statement, must be separated by a semi-colon from it's succeeding statement, if any.<br>

#### Break

When this statement is executed, the program exits the specified number of loops and the execution is continued normally from the statement after the exited loop.<br>
```
sum := 0;
untilcase
	when true: input x;
	when x = 0: break 1;
	when x < 0: sum := sum - x*x;
	when x > 0: sum := sum + x*x;
until x = a;
print sum;
```
The break statement can also be used to break any block.<br>
This provides a safe way to jump to the end of any block of code.<br>
Beware that this also means a higher break count is required when a loop consists of a block of code.<br>
```
{
	input power;
	if power <= 0 break 1;
	
	sum := 0;
	while true
	{
		input x;
		if x = 0 break 2;
		term := 1;
		forcase i = power
			when true: term := term * x;
		sum := sum + term;
	};
	print sum;
}
```
The break statement cannot jump to the end of an if or switchcase statement. When determining the target of a break, if and switchcase statements are ignored.<br>
The break statement ignores any loops or blocks on the same level as itself or the if or switchcase statement it is contained within.<br>
Any break statements with a count of "0" are ignored.<br>

#### Repeat

When this statement is executed, the program repeats the statements within a target loop. The loop to repeat is determined by the provided index.<br>
When specifying an index of "x", the target loop is the one containing the repeat statement and starting x levels outside of the level of the repeat statement.<br>
In other words, if "1" is specified, the target loop is the current loop, if "2" is specified, it is the loop that contains the current loop, etc.<br>
```
a := 42;
untilcase
	when true: input x;
	when x <= 0: repeat 1;
	when x < a: print -1;
	when x > a: print 1;
until x = a;
print 89;
```
The repeat statement can also be used to repeat any block.<br>
This provides a safe way to jump to the beginning of any block of code.<br>
Beware that this also means a higher repeat index is required when a loop consists of a block of code.<br>
```
{
	sum := 0;
	budget := 20;
	
	while budget > 0
	{
		print sum;
		print budget;
		
		// Assume a Rumdom Number Generator is defined in a different part of the program
		x := random(1, 6); 
		print x;
		
		input c;
		if c = 0
		{
			sum := sum + x;
			budget := budget - 1;
			repeat 3;
		};
		
		y := random(0, 10);
		if y = 0
		{
			print 70;
			repeat 4;
		};
		
		sum := sum + x*x;
		budget := budget - x/2;
		// Let's call the fact that the budget can become negative "Intentional Game Design"
	};
	
	print sum;
}
```

#### Break & Repeat Points

For most control structures, the point a break or repeat jumps to is the beginning or end of the whole statement, respectively.<br>
Many of them also offer a way to insert code that can only be executed if the loop is not broken.<br>
```
while x > 0 {
	break 2; // break skips the else statement and jumps to "print x"
	repeat 2; // repeat jumps to the condition check
} else y := 1; // This statement will be executed when the loop ends normally, but not when broken
print x;

whilecase
	when x > 0: break 1; // break skips the default statement and jumps to "print x"
	when x < 0: repeat 1; // repeat jumps to the first condition check, "x > 0"
default: y := 1; // This statement will be executed when the loop ends normally, but not when broken
print x;

untilcase
	when x > 0: break 1; // break skips the until condition and jumps to "print x"
	when x < 0: repeat 1; // repeat jumps to the first condition check, "x > 0"
until x = 0;
print x;

incase
	when x > 0: break 1 // break jumps to "print x"
	when x < 0: repeat 1; // repeat jumps to the first condition check, "x > 0"
print x;
```
The Forcase statement offers two break points and two repeat points, to allow finer control of the program execution.<br>
One set of break and repeat points behaves similarly to the break and repeat points of other control structures, ending or restarting the whole statement.<br>
This set allows for the iteration variable to be reset or for the loop to end prematurely.<br>
The other set of break and repeat points controls the current iteration, ending or restarting only the checking of the conditions within the forcase statement.<br>
This set allows rechecking all conditions without resetting the iteration variable or starting the next iteration without checking subsequent conditions.<br>
```
forcase i = 100
	when x > 0: break 2 // break 2 jumps to "print x"
	when x = 0: repeat 2 // repeat 2 jumps to the resetting of i at the start of the statement
	when x <> 0: break 1 // break 1 jumps to the incrementation and check of i
	when x < 0: repeat 1; // repeat 1 jumps to the first condition check, "x > 0"
print x;
```

### Functions

Functions are declared using the "function" keyword, the function id, any number of parameters in parentheses, and a block of Declarations, Sub-Functions, and / or Statements, enclosed by curly brackets.<br>
```
function f()
{
	declare a;
	input a;
	print a*a;
}
```
Similarly to blocks, any variables declared in a function can only be used inside that function.<br>
Function declarations are NOT separated by semi-colons.<br>
<br>
Functions can have parameters, which are declared by listing their ids separated by commas.<br>
Parameters don't need to be declared using the "declare" keyword.<br>
```
function g(a, b, c)
{
	print 2*a + b - c
}
```
Parameters can be used as input, output or both.<br>
Each parameter can declare it's purpose using one of "in", "out" and "inout" keywords before its id.<br>
If no keyword is added before a parameter id, the parameter is assumed to be an input.<br>
```
function h(in a, out b, inout c)
{
	b := 2*a + c;
	c := c - a;
}
```
The meaning of each parameter type is as follows:
* "in" parameters are a copy of the result of the expression that was used to call the function.
* "out" parameters can only be assigned to a value and cannot be used in an expression. The value assigned to an out parameter is assigned to the variable whose id was used to call the function.
* "inout" parameters give full access to the variable that was used to call the the function.<br>

Every function has an integer return value. By default the return value is "0", but it can be changed using a "return" statement, consisting of the "return" keyword, followed by any expression.<br>
```
function factorial(n)
{
	declare result;
	result := 1;
	forcase i = n
		when true: result := result * i;
	return result;
}
```
When a return statement is executed, the execution of the function is stopped and the return value can be used immediately by the caller.<br>
Before executing a return statement, all out parameters must have been assigned a value at least once.<br>
```
function factorial(n)
{
	if n <= 0 return 1;
	return n * factorial(n - 1);
}
```
A function can only be called as part of an expression, but it can be used freely in said expression.<br>
```
x := f();
y := g(g(1,2,3),g(1,2,3),g(1,2,3));
z := h(in 5*5, out x, inout y);
if factorial(x)/factorial(y)/factorial(x-y) < 10
	print 70;
```
When a function appears many times, even if the parameters are the same or there are no parameters, the whole function will be executed each time the expression is evaluated.<br>
If a function takes out or inout parameters, the appropriate keyword must be used before the variable id.<br>
The "in" keyword can also be used when specifying an "in" parameter, but it is not required.<br>
<br>
If a function can be implemented using only a single statement, the function can be defined without curly brackets.<br>
```
function g(a, b, c) print 2*a + b - c
function square(x) return x*x
```
Such functions must not end with a semi-colon.<br>