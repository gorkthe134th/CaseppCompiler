program p
{
	break 1; // expect a jump to the end of the program (the halt instruction)
	x := 9; // buffer 1
	forcase i = x + 9 /* expect
	- an instruction initializing i to 0
	- an instruction adding 9 to x
	- a comparison jump skipping the next instruction if i is less than the result
	- a jump to after the end of the forcase (buffer 2)
	- an instruction incrementing i*/
	when x > 9: break 1 // expect a comparison jump and an unconditional jump, both skipping their next instruction, and a jump to the end of the forcase (the jump to the instruction adding 9 to x)
	when x = 9: break 2 // expect similar instructions to above, but jumping to after the end of the forcase (buffer 2)
	when x <> 9: repeat 2 // expect similar instructions to above, but jumping to the start of the forcase (the instruction initializing i to 0)
	when x < 9: repeat 1; // expect similar instructions to above, but jumping to the start of the current iteration (the first comparison)
	// expect a jump to the instruction adding 9 to x
	x := 9; // buffer 2
	repeat 1; // expect a jump to the start of the program (the first break)
}