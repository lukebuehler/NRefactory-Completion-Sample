using System;
using System.Linq;

public class Sample1
{
    public Sample1()
	{
		var a = "Hello";
		a.Length;
		var xs = new int[]{1,2,3,4,5};
	}

	///<summary>
	///
	///</summary>
    public int Add(int number1, int number2)
    {
        number1 + number2;
    }
}

public class OtherClass
{
    public void GetSample1()
    {
        var sample = new Sample1();
        sample.Add(10,100);
    }
}
