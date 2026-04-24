program p
{
	declare x;
	break 1; // expect a jump to the end of the program (the halt instruction)
	x := 9; // buffer 1
	forcase i=x-9; /* expect
	- an instruction initializing i to 0
	- an instruction subtracting 9 from x
	- a comparison jump skipping the next instruction if i is less than the result
	- a jump to after the end of the forcase (buffer 2)
	- an instruction incrementing i
	- a jump to the comparison*/
	x := 9; // buffer 2
	repeat 1; // expect a jump to the start of the program (the first break)
}