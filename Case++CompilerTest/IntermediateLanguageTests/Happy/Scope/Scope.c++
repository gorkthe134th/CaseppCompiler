program p
{
	declare x;
	function f() x := 9
	function g1() { function g2() x := 9 }
	{
		declare y;
		function h1() x := 9
		function h2() y := 42
		x := 9;
		y := 42;
	};
	x := 9;
}