program p
{
	declare x;
	x := 9;

	if x=9 ; /* expect
	- a comparison jump skipping the next instruction
	- a jump to the next instruction*/
	x:=9; // buffer
}