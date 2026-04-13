program p
{
	switchcase
	when x > 9: /* expect
	- a comparison jump skipping the next instruction
	- a jump skipping the next instruction
	- a jump to after the end of the switchcase (the halt instruction)*/
	when x = 9: setup := work /* expect
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