program p
{
	declare x;
	x := 9;

	switchcase
	when x > 9: /* expect
	- a comparison jump skipping the next instruction
	- a jump skipping the next instruction
	- a jump to after the end of the switchcase (the halt instruction)*/
	when x = 9: x := 42 /* expect
	- a comparison jump skipping the next instruction
	- a jump skipping the next two instructions
	- the assignment instruction
	- a jump to after the end of the switchcase (the halt instruction)*/
	when x < 9: ; /* expect
	- a comparison jump skipping the next instruction
	- a jump skipping the next instruction
	- a jump to after the end of the switchcase (the halt instruction)*/
	default: { x := 9; } // expect only the assignment instruction
}