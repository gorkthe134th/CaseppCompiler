program p
{
	declare x;
	# {
		jump, _, _, l1
		l2:
	};
	x := 9;
	# jump, _, _, l2;
	x := 9;
	# l1:;
}