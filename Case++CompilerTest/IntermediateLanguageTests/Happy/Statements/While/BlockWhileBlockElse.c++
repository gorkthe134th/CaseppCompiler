program p
{
	declare x;
	break 1; // expect a jump to the end of the program (the halt instruction)
	x := 9; // buffer 1
	 /* for all whiles, expect
	- a comparison jump skipping the next instruction
	- a jump to the else body
	- the while body
	- a jump to the start of the while (the comparison jump)
	- the else body */
	 // for all breaks, expect a jump to after the respective else body
	 // for all repeats, expect a jump to after the start of the while (the comparison jump)
	while x=9 { break 2; repeat 2; } else { break 2; repeat 2; };
	while x=9 { break 2; repeat 2; } else { };
	while x=9 { } else { break 2; repeat 2; };
	while x=9 { } else { };
	x := 9; // buffer 2
	repeat 1; // expect a jump to the start of the program (the first break)
}