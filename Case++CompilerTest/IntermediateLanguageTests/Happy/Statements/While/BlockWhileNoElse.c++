program p
{
	break 1; // expect a jump to the end of the program (the halt instruction)
	x := 9; // buffer 1
	while x=9 break 1; /* expect
	- a comparison jump skipping the next instruction
	- a jump to after the end of the while (skip two instruction)
	- a jump to after the end of the while (skip one instruction)
	- a jump to the start of the while (the comparison jump)*/
	while x=9 repeat 1; /* expect
	- a comparison jump skipping the next instruction
	- a jump to after the end of the while (skip two instruction)
	- a jump to the start of the while (the comparison jump)
	- a jump to the start of the while (the comparison jump)*/
	while x=9 { }; /* expect
	- a comparison jump skipping the next instruction
	- a jump to after the end of the while (skip one instruction)
	- a jump to the start of the while (the comparison jump)*/
	x := 9; // buffer 2
	repeat 1; // expect a jump to the start of the program (the first break)
}