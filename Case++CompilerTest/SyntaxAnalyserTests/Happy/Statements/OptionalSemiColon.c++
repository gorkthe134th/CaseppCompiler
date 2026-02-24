program p
{
	function a1() { ; }
	function f1()
	{
		x := 9
	}
	function g1()
	{
		x := 8;
		x := 9
	}
	function h1()
	{
		x := 7;;
		x := 8;
		x := 9
	}

	function a2() { ;;; }
	function f2()
	{
		x := 9;
	}
	function g2()
	{
		x := 8;;
		x := 9;
	}
	function h2()
	{
		x := 7;;;
		x := 8;;
		x := 9;
	}

	function b() { ;;;x := 9;;; }
}