using System;

public class Sample1
{
    public Sample1()
	{
		var a = "Hello";
		a.Length;
	}

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
    }
}
