program p
{
	if x=9 else ; /* expect
	- a comparison jump skipping the next instruction
	- a jump skipping the next instruction
	- a jump to the next instruction*/
	x:=9; // buffer
}