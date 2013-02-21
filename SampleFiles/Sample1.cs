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
	///Test
	///</summary>
    public virtual int Add(int number1, int number2)
    {
        number1 + number2;
    }
}

public class Sample2 : Sample1
{
	public event EventHandler<EventArgs> MyEvent;
	public void Hello(){
		MyEvent(this, new EventArgs());
	}
}

public class OtherClass
{
    public void GetSample1()
    {
        var sample = new Sample2();
        sample.Add(10,100);
    }
}
