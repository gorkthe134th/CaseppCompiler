program p
{
	if true x:=9; // expect a single jump to the if body
	if [false] x:=9; // expect a single jump skipping the if body
	if x=9 x:=9; // expect two jumps: the first being a comparison and jumping to the if body and the second skipping the if body
}