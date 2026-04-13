program p
{
	break 1; // expect a jump to the end of the program (the halt instruction)
	x := 9; // buffer 1
	break 5; // expect a jump to the end of the program (the halt instruction)
	x := 9; // buffer 2
}