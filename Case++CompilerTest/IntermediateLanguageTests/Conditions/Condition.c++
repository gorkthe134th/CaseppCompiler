program p
{
	if true x:=9;
	if [false] x:=9;
	if true or false x:=9;
	if true or x=9 or false x:=9;
	if true or not[false] and true x:=9;
	if true or true and x=9 x:=9;
	if x=9 and true and false x:=9;
	if true and [false or x=9] x:=9;
	if [true and x=9] and [false and x=9] x:=9;
	if x<9 and x>9 x:=9;
	if x<>9 or not[not[x<>9] and false] x:=9;
}