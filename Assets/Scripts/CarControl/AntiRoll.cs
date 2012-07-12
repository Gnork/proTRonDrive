using UnityEngine;
using System.Collections;

public class AntiRoll
{
	private Wheel wheelL;
	private Wheel wheelR;

	public const float antiRollValue = 10000.0f;

	public AntiRoll(Wheel left, Wheel right)
	{
		this.wheelL = left;
		this.wheelR = right;
	}
	
	public void ApplyAntiRoll(Rigidbody rigidbody)
	{	
	    WheelHit hit = new WheelHit();
		float travelL = 1.0f;
		float travelR = 1.0f;
	
	    bool groundedL = wheelL.collider.GetGroundHit(out hit);	
	    if (groundedL)
		{
	        travelL = (-wheelL.collider.transform.InverseTransformPoint(hit.point).y - wheelL.radius) / wheelL.collider.suspensionDistance;
		}
		 
	    bool groundedR = wheelR.collider.GetGroundHit(out hit);	
	    if (groundedR)
		{
	        travelR = (-wheelR.collider.transform.InverseTransformPoint(hit.point).y - wheelR.radius) / wheelR.collider.suspensionDistance;
		}
		 
	    float antiRollForce = (travelL - travelR) * antiRollValue;	
	
	    if (groundedL)	
		{
	    	rigidbody.AddForceAtPosition(wheelL.collider.transform.up * -antiRollForce,
											wheelL.collider.transform.position);  
		}
	
	    if (groundedR)
		{
	        rigidbody.AddForceAtPosition(wheelR.collider.transform.up * antiRollForce,	
	              							 wheelR.collider.transform.position);  
		}
	}
}
