program p
{
	x := 9; // expect 9
	x := +9; // expect adding 9 to 0 and returning the result
	x := -9; // expect subtracting 9 from 0 and returning the result
	x := (9); // expect 9
	x := 5+4; // expect adding 4 to 5 and returning the result
	x := 11-2; // expect subtracting 2 from 11 and returning the result
	x := 5+7-3; // expect adding 7 to 5, subtracting 3 from it and returning the result
	x := 5+2*2; // expect multiplying 2 by 2, adding it to 5 and returning the result
	x := 5+8/2; // expect deviding 8 by 2, adding it to 5 and returning the result
	x := 3/2*9; // expect deviding 3 by 2, multiplying it by 9 and returning the result
	x := 3*(2+1); // expect adding 1 to 2, multiplying 3 by it and returning the result
	x := (3*i)*(-3*i); // expect multiplying 3 by i twice, subtracting the second from 0, multiplying them and returning the result
}