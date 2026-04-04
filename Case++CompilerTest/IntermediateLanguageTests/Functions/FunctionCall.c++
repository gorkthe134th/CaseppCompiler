program p
{
	function f() { }
	function gi(v) return v
	function go(out v) v := f()
	function gio(inout v) v := gi(v)
	function h(in v1, inout v2, out v3) v3 := v1 + v2

	x := f(); // expect a single return parameter before a call to "f"
	x := gi(a); // expect "a" as a value parameter and a single return parameter before a call to "gi"
	x := gi(in a); // expect the same as above
	x := go(out a);  // expect "a" as a return parameter and another temporary return parameter before a call to "go"
	x := gio(inout a); // expect "a" as a reference parameter and a single return parameter before a call to "gio"
	x := h(in a, inout b, out c); // expect "a" as a value parameter, "b" as a reference parameter, "c" as a return parameter and another temporary return parameter before a call to "h"
}