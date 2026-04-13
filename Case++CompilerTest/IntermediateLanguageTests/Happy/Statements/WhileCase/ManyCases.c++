program p
{
	break 1; // expect a jump to the end of the program (the halt instruction)
	x := 9; // buffer 1
	whilecase
	when x > 9: break 1 /* expect
	- a comparison jump skipping the next instruction
	- a jump skipping the next two instruction
	- a jump to after the end of the whilecase (buffer 2)
	- a jump to the start of the whilecase (the first condition)*/
	when x < 9: repeat 1; /* expect
	- a comparison jump skipping the next instruction
	- a jump skipping the next two instruction
	- a jump to the start of the whilecase (the first condition)
	- a jump to the start of the whilecase (the first condition)*/
	default: { x := 9; };
	x := 9; // buffer 2
	repeat 1; // expect a jump to the start of the program (the first break)
}