using System;
using UnityEngine;

public class TrafficLightManager : MonoBehaviour
{
	/**
	 * Consts
	 */
	private const string overRedSignName = "OverRed";
	
	/**
	 * Editor members (for initialization only)
	 */
	public GameControl e_GameControl;
	public DriveMessages e_DriveMessages;
	
	/**
	 * Member variables
	 */
	private GameControl _control;
	private DriveMessages _messages;
	
	private bool _initialized = false;
	
	/**
	 * Properties
	 */
	private TrafficLight[] AllTrafficLights
	{
		get { return this.GetComponentsInChildren<TrafficLight>(); }
	}
	
	void Start()
	{
		if (e_GameControl != null)
			_control = e_GameControl;
		else
			throw new UnassignedReferenceException("The editor member 'e_GameControl' of TrafficLightEngine must be assigned.");
		
		if (e_DriveMessages != null)
			_messages = e_DriveMessages;
		else
			throw new UnassignedReferenceException("The editor member 'e_DriveMessages' of TrafficLightEngine must be assigned.");		
	}
	
	void Update()
	{
		// Has to wait till the message engine is initialized
		if ((!_initialized) && (_messages.IsInitialized))
		{
			// Add the "passed a red traffic light" sign
			_messages.AddSign(overRedSignName, true);
			_initialized = true;
		}
	}
	
	public void StartEngine()
	{
		foreach (TrafficLight trafficLight in AllTrafficLights)
		{
			trafficLight.Active = true;
		}
	}
	
	public void TrafficLightPassed(bool passedOnRed, float timeLeft)
	{
		if (passedOnRed)
		{
			_control.TrafficLightPassedOnRed(timeLeft);
			_messages.ShowSign(overRedSignName);
		}
	}
}
