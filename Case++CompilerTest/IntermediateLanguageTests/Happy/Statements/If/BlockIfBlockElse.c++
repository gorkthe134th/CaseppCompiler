program p
{
	declare x;
	x := 9;

	if x=9 { x:=9 } else { x:=9 }; /* expect
	- a comparison jump skipping the next instruction
	- a jump to the else body
	- the if body
	- a jump skipping the else body
	- the else body*/
	if x=9 { } else { } /* expect
	- a comparison jump skipping the next instruction
	- a jump skipping the next instruction
	- a jump to the next instruction*/
}