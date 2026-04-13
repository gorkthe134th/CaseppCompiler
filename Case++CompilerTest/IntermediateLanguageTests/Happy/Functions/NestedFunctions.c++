program p
{
	// expect three blocks ("p_f", "p_f_g", "p_f_g_h"), each with a single instruction returning 0
	function f() { function g() { function h() {} } }
}