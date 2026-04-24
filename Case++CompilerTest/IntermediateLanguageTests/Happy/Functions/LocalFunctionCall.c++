program p
{
	declare x;
	/*
	expect
	- a "p_f_g" block with an instruction returning 9 and an instruction returning 0 (return 0 is just a fallback in case there was no return)
	- a "p_f" block with a single temporary return parameter before a call to "f", an assignment to "x" of the same temporary variable and an instruction returning 0
	*/
	function f() {
		function g() return 9
		x := g();
	}
}