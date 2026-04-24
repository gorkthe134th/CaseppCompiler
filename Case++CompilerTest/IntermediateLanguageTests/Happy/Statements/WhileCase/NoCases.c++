program p
{
	declare x;
	break 1; // expect a jump to the end of the program (the halt instruction)
	x := 9; // buffer 1
	whilecase
	default: x := 9; // expect only the assignment
	x := 9; // buffer 2
	repeat 1; // expect a jump to the start of the program (the first break)
}