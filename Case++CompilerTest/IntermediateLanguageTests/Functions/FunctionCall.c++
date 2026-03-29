program p
{
	function f() { }
	function gi(v) return v
	function go(out v) v := f()
	function gio(inout v) v := gi(v)
	function h(in v1, inout v2, out v3) v3 := v1 + v2

	x := f();
	x := gi(a);
	x := gi(in a);
	x := go(out a);
	x := gio(inout a);
	x := h(in a, inout b, out c);
}