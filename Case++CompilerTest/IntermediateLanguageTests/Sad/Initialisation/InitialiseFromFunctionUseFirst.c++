program p
{
	declare x, y;

	function f() x := x;

	y := f();
	print x;
}