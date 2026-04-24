program p
{
	declare x;
	break 1; // expect a jump to the end of the program (the halt instruction)
	x := 9; // buffer 1
	untilcase
	until 0 = x - 9; /* expect
	- an instruction subtracting 9 from x
	- a comparison jump skipping the next instruction if 0 is equal to the result
	- a jump to the subtraction*/
	x := 9; // buffer 2
	repeat 1; // expect a jump to the start of the program (the first break)
}