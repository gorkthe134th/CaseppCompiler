program p
{
	declare x;
	x := 9;
	if x<9 or x>9 x:=9; // expect four jumps: the first jumping to the if body, the second jumping to the third, the third jumping to the if body, the forth skipping the if body
	if x<=9 and x>=9 x:=9; // expect four jumps: the first jumping to the third, the second skipping the if body, the third jumping to the if body, the forth skipping the if body
	if false or false or x=9 x:=9; // expect two jumps to the next instruction, a comparison jump that jumps to the if body and a jump skipping the if body
	if true and true and x=9 x:=9; // expect the same jumps as above
	if true and [false or x=9] x:=9; // expect the same jumps as above
}