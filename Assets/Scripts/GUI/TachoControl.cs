//Christoph Jansen

using UnityEngine;
using System.Collections;

public class TachoControl : MonoBehaviour {
	
	/*Rotatable GUI items
	 *public to be assigned in editor*/
	public RotatableGuiItem veloTacho;
	public RotatableGuiItem rpmTacho;
	
	//Data set by other scripts
	public static float velocity;
	public static float rpm;
	
	//Control settings
	private const int MAX_VALUES = 30;
	private float veloAbsorb = 3.0f;
	private float rpmAbsorb = 0.0f;
	
	//Store old data for slow tacho movement
	private float[] veloArray = new float[MAX_VALUES];
	private float[] rpmArray = new float[MAX_VALUES];
	private int arrayPosition = 0;
	
	//Set values to zero
	void Start () 
	{
		velocity = 0;
		rpm = 0;
		for(int i = 0; i < veloArray.Length; ++i)
		{
			veloArray[i] = rpmArray[i] = 0;
		}
	}
	
	void Update () 
	{
		AddToArray();
		//Update rotation of tacho and send velocity data to Camera script
		veloTacho.angle = ModifiedCamera.velocity = VeloAngle(VeloAverage());
		rpmTacho.angle = RpmAngle(RpmAverage());
	}
	
	//Calculate rotation angle
	float VeloAngle(float velocity)
	{
		if(velocity > 160)
		{
			velocity = 160;
		}
		return velocity * 240 / 160;
	}
	
	//Calculate rotation angle
	float RpmAngle(float rpm)
	{
		if(rpm > 8000)
		{
			rpm = 8000;
		}
		return rpm * 240 / 8000;
	}
	
	//Add new data to array
	void AddToArray()
	{
		arrayPosition = (arrayPosition + 1) % MAX_VALUES;
		veloArray[arrayPosition] = VeloAbsorber(velocity);
		rpmArray[arrayPosition] = RpmAbsorber(rpm);
	}
	
	//Use Average of old data to avoid jitter in tacho movement
	float VeloAverage()
	{
		float sum = 0;
		for(int i = 0; i < veloArray.Length; ++i)
		{
			sum += veloArray[i];
		}
		return sum/veloArray.Length;
	}
	
	//Use Average of old data to avoid jitter in tacho movement
	float RpmAverage()
	{
		float sum = 0;
		for(int i = 0; i < rpmArray.Length; ++i)
		{
			sum += rpmArray[i];
		}
		return sum/rpmArray.Length;
	}
	
	//Slow down tacho movement
	float VeloAbsorber(float v)
	{
		if(veloAbsorb <= 0.0f)
		{
			return v;
		}
		float help = veloArray[(arrayPosition - 1 + veloArray.Length) % veloArray.Length];
		if( v < help - veloAbsorb )
		{
			return help - veloAbsorb;
		}
		else
		{
			return v;
		}
	}
	
	//Slow down tacho movement
	float RpmAbsorber(float a)
	{
		if(rpmAbsorb <= 0.0f)
		{
			return a;
		}
		float help = rpmArray[(arrayPosition - 1 + MAX_VALUES) % MAX_VALUES];
		if( a < help - rpmAbsorb )
		{
			return help - rpmAbsorb;
		}
		else
		{
			return a;
		}
	}
}
