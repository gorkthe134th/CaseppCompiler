program p
{
	declare x;
	break 1; // expect a jump to the end of the program (the halt instruction)
	x := 9; // buffer 1
	while x=9 else ; /* expect
	- a comparison jump skipping the next instruction
	- a jump skipping the next instruction
	- a jump to the start of the while (the comparison jump)*/
	x := 9; // buffer 2
	repeat 1; // expect a jump to the start of the program (the first break)
}