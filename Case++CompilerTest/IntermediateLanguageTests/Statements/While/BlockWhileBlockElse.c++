program p
{
	break 1;
	x := 9;
	while x=9 { break 2; repeat 2; } else { break 2; repeat 2; };
	while x=9 { break 2; repeat 2; } else { };
	while x=9 { } else { break 2; repeat 2; };
	while x=9 { } else { };
	x := 9;
	repeat 1;
}