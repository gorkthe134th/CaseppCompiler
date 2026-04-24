program p
{
	declare x;
	x := 9;

	if x=9 { x:=9 }; /* expect
	- a comparison jump skipping the next instruction
	- a jump skipping the if body
	- the if body*/
	if x=9 { } /* expect
	- a comparison jump skipping the next instruction
	- a jump to next instruction*/
}