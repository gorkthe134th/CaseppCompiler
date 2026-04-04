program p
{
	if not[x<>9] x:=9; // expect two jumps: the first being a comparison and skipping the if body and the second jumping to the if body
	if not[x<9 and x>9] x:=9; // expect four jumps: the first jumping to the third, the second jumping to the if body, the third skipping the if body, the forth jumping to the if body
	if not[x<=9 or x>=9] x:=9; // expect four jumps: the first skipping the if body, the second jumping to the third, the third skipping the if body, the forth jumping to the if body
	if not[not[x=9]] x:=9; // expect two jumps: the first being a comparison and jumping to the if body and the second skipping the if body
}