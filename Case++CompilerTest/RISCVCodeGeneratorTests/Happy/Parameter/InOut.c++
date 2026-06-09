program p
{
	declare x;
	
	function f(inout x) x := x + 1

	x := 9;
	return f(inout x);
}