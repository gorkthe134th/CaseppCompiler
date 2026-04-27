program p
{
	declare x, y, z;

	function f() return x;
	function g()
	{
		x := 9;
		y := f();
	}

	z := g();
	print x;
	print y;
}