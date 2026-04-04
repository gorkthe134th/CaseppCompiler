program p
{
	break 1; // expect a jump to the end of the program (the halt instruction)
	x := 9; // buffer 1
	untilcase
	when x > 9:	break 1 /* expect
	- a comparison jump skipping the next instruction
	- a jump skipping the next instruction
	- a jump to after the end of the untilcase (buffer 2)*/
	when x < 9: repeat 1; /* expect
	- a comparison jump skipping the next instruction
	- a jump skipping the next instruction
	- a jump to the start of the untilcase (the first condition)*/
	until i = x - 9; /* expect
	- an instruction subtracting 9 from x
	- a comparison jump skipping the next instruction if i is equal than the result
	- a jump to the start of the untilcase (the first condition)*/
	x := 9; // buffer 2
	repeat 1; // expect a jump to the start of the program (the first break)
}