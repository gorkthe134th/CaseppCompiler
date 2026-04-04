program p
{
	break 1; // expect a jump to the end of the program (the halt instruction)
	x := 9; // buffer 1
	{
		{
			break 1; // expect a jump to after the end of this block (buffer 5)
			x := 9; // buffer 2
			break 2; // expect a jump to after the end of the block containing this one (buffer 6)
			x := 9; // buffer 3
			break 5; // expect a jump to the end of the program (the halt instruction)
			x := 9; // buffer 4
		};
		x := 9; // buffer 5
	};
	x := 9; // buffer 6
}