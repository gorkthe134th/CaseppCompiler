program p
{
	declare x;
	break 1; // expect a jump to the end of the program (the halt instruction)
	x := 9; // buffer 1
	incase // expect an instruction initializing a temporary variable to 0
	when x > 9: break 1 /* expect
	- a comparison jump skipping the next instruction
	- a jump skipping the next two instructions
	- a jump to after the end of the incase (buffer 2)
	- an instruction setting the temporary variable to something non 0*/
	when x < 9: repeat 1; /* expect
	- a comparison jump skipping the next instruction
	- a jump skipping the next two instructions
	- a jump to the start of the incase (the instruction initializing the temporary variable to 0)
	- an instruction setting the temporary variable to something non 0*/
	// expect a comparison jump to the instruction initializing the temporary variable if it is not 0
	x := 9; // buffer 2
	repeat 1; // expect a jump to the start of the program (the first break)
}