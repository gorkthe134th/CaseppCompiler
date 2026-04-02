program p
{
	break 1;
	x := 9;
	forcase i = 1
	when x > 9: break 1
	when x = 9: break 2
	when x <> 9: repeat 2
	when x < 9: repeat 1;
	x := 9;
	repeat 1;
}