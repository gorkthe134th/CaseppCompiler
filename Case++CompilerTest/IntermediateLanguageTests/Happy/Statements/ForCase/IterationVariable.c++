program p
{
	declare x;
	forcase x=9; // Use declared variable
	forcase i=5; // Use new iteration variable
	forcase i=x; // Use existing iteration variable
	print i; // Get leftover value
}