program p
{
	declare x;
	function f()
	{
		break 1; // expect a jump to the end of the function (the return 0 instruction)
		x := 9; // buffer 1
		break 5; // expect a jump to the end of the function (the return 0 instruction)
		x := 9; // buffer 2
	}
}