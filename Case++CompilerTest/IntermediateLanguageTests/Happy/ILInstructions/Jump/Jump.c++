program p
{
	declare x;
	# {
		jump, _, _, l1
		l1: :=, 9, _, x
		l2: jump, _, _, l2
		l3: :=, 9, _, x
		jump, _, _, l3
	};
}