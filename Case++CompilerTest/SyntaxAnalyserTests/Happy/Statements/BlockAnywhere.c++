program p
{
	{ };
	x := 9;
	{ x := 9; };
	{
		function f()
		{
			x := 9
		}
	};
	{ { { { { { { x := 9 } } } } } } }
}