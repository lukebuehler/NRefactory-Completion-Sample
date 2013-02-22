using System;
using System.Linq;

public class Sample1
{
	public event EventHandler<EventArgs> MyEvent;
	
    public Sample1()
	{
		var a = "Hello";
	}

	///<summary>
	/// This adds two nubers together.
	///</summary>
    public virtual int Add(int number1, int number2)
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
