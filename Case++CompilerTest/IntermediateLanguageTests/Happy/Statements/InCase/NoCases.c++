program p
{
	break 1; // expect a jump to the end of the program (the halt instruction)
	x := 9; // buffer 1
	incase; /* expect
	- an instruction initializing a temporary variable to 0
	- a comparison jump to the above instruction if the temporary variable is not 0*/
	x := 9; // buffer 2
	repeat 1; // expect a jump to the start of the program (the first break)
}