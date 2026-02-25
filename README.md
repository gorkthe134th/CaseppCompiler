# Case++Compiler

A compiler for the made-up programming language case++.<br>

## Description

An in-development compiler created as part of a university course.<br>
A minimum required language specification was provided by the associated professor, but I have also expanded the syntax and supported additional features when I deemed it appropriate.<br>
The full language documentation can be viewed bellow.<br>
The compiler's output language will be RISC-V assembly.<br>
Current functionality is limited to Lexical and Syntactical Analysis.<br>
The input file encoding is also currently restricted to UTF-8.<br>

## Case++ Documentation

Case++ is an imperative procedural programming language, with C-like syntax, created for the purpose of this exercise.<br>
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
Declarations, Functions and Statements are required to be in defined in that order, but not all three, if any, need to be present.<br>

### Declarations

Declarations are made using the keyword "declare", any number of variable ids, ending with a semi-colon.<br>
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
	x := 1;
	y := 2;
	z := 3
}
```
The last statement of a block is allowed to end with a semi-colon, but it is not required.<br>
In place of a statement, a block can be started using curly brackets, containing local variables and functions for that block.<br>
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
Blocks, like any other statement, must be separated by a semi-colon from it's succeeding statement.<br>

### Assignment

Assignment is performed using ":=". The left side must be a variable id and right side must be an expression.<br>
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

When the program is executed, the condition after the "if" keyword will be checked and the subsequent statement will only be executed if the condition is true.<br>
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
The "else" keyword may follow that statement, indicating that *its* subsequent statement will be executed only if the condition was false.<br>
```
if x/2*2=x x:=x/2 else x:=3*x+1
```
The statement between the "if" and "else" keywords must not end with a semi-colon.<br>
```
if x/2*2=x
	x:=x/2; // Not allowed
else
	x:=3*x+1; // Required
print x
```
The whole if statement, like any other statement, must be separated by a semi-colon from it's succeeding statement.<br>

#### While

When the program is executed, the condition after the "while" keyword will be checked and the subsequent statement will only be executed if the condition is true.<br>
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
The whole while statement, like any other statement, must be separated by a semi-colon from it's succeeding statement.<br>

#### Switchcase

When the program is executed, the condition after each "when" keyword between the "switchcase" and "default" keywords will be checked and the statement after the first condition that was true will only be executed.<br>
If none of the conditions are true, the statement after the "default" keyword is executed.<br>
```
switchcase
	when x > 10: x := x - 10
	when x < 0 : x := x + 10;
default: print 89
```
Every statement must be preceded by a colon and may be succeeded by a semi-colon.<br>