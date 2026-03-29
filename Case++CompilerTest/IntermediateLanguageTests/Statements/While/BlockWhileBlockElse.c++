program p
{
	break 1;
	x := 9;
	while x=9 { x:=9; break 2; x:=9; } else { x:=9 };
	while x=9 { break 2 } else { };
	while x=9 { } else { };
	x := 9;
}