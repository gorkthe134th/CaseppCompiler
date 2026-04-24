program p
{
	declare x, y;
	declare a, b, c;

	function f() return 1
	function g(a) return 2
	function h(a, b) return 3
	function F(a, b, t) return 4

	a := g(f()); /* expect
	- a single temporary return parameter before a call to "f",
	- the same temporary variable as a value parameter and a single return parameter before a call to "g"*/
	b := g(g(a)); /* expect
	- "a" as a value parameter and a single temporary return parameter before a call to "g",
	- the same temporary variable as a value parameter and a single return parameter before a call to "g"*/
	c := h(f(),a); /* expect
	- a single temporary return parameter before a call to "f",
	- the same temporary variable as a value parameter, "a" as a value parameter and a single return parameter before a call to "h"*/
	x := h(g(a),g(b)); /* expect
	- "a" as a value parameter and a single temporary return parameter before a call to "g",
	- "b" as a value parameter and a single temporary return parameter before a call to "g",
	- the same temporary variables as value parameters and a single return parameter before a call to "h"*/
	y := F(h(a,f()),g(b),c); /* expect
	- a single temporary return parameter before a call to "f",
	- "a" as a value parameter, the above temporary variable as a value parameter and a single return parameter before a call to "h",
	- "b" as a value parameter and a single temporary return parameter before a call to "g",
	- the last two temporary variables as value parameters, "c" as a value parameter and a single return parameter before a call to "h"*/
}