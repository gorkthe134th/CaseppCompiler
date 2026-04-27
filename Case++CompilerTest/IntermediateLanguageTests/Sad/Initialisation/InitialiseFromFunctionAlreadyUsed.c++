program p
{
	declare x, y;

	function f() x := 9;

	function g()
	{
		y := x;
		y := f();
	}

	y := g();
	print x;
}