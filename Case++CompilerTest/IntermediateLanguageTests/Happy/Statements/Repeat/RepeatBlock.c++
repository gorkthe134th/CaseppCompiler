program p
{
	x := 9; // buffer 1
	{
		x := 9; // buffer 2
		{
			x := 9; // buffer 3
			repeat 5; // expect a jump to the start of the program (buffer 1)
			x := 9; // buffer 4
			repeat 2; // expect a jump to the start of the block containing this one (buffer 2)
			x := 9; // buffer 5
			repeat 1; // expect a jump to the start of this block (buffer 3)
		};
	};
	x := 9; // buffer 6
	repeat 1; // expect a jump to the start of the program (buffer 1)
}