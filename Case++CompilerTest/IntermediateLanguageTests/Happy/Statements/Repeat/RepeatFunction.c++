program p
{
	declare x;
	function f()
	{
		x := 9; // buffer 1
		repeat 5; // expect a jump to the start of the function (buffer 1)
		x := 9; // buffer 2
		repeat 1; // expect a jump to the start of the function (buffer 1)
	}
}