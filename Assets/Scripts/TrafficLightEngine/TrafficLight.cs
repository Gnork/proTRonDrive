using System;
using UnityEngine;
using AssemblyCSharp;

public class TrafficLight : MonoBehaviour, ICheckReceiver
{
	/**
	  * Editor variables (only used for initialization)
	  */

	// The time of the different phases (in seconds)
	public float e_GreenTime;
	public float e_YellowTime;
	public float e_RedTime;		
	
	// The light objects in the game to switch on/off
	public Light e_GreenLight;
	public Light e_YellowLight;
	public Light e_RedLight;
	
	// The 3D-Text which is used to show the remaining time in seconds
	public TextMesh e_RemainingTimeDisplay;
	public MeshRenderer e_RemainingTimeDisplayRenderer;
	
	// The engine which manages all traffic lights
	public TrafficLightManager e_TrafficLightEngine;
			
	/** 
	 * Member Variables
	 */
	private TrafficLightStates _phase;
	private Clock _lightTimer;
	
	private float _greenTime;
	private float _yellowTime;
	private float _redTime;		
	
	private Light _greenLight;
	private Light _yellowLight;
	private Light _redLight;	
	
	private Material _greenMaterial;
	private Material _redMaterial;
	private Material _yellowMaterial;
	
	private TextMesh _timeDisplay;
	private MeshRenderer _timeDisplayRenderer;
	
	private TrafficLightManager _engine;
	
	private bool _active;
	
	/**
	  * Properties
	  */
	public bool Active
	{
		get { return _active; }
		
		set
		{
			_active = value;
			
			if (_active)
				_lightTimer.Start();		
			else
				_lightTimer.Pause();	
		}
	}
	
	public String TimeString
	{
		get { return _lightTimer.TimeString; }
	}
	
	/**
	  * Methods
	  */
	void Start()
	{
		LoadMaterials();
		
		_timeDisplay = e_RemainingTimeDisplay;
		_timeDisplayRenderer = e_RemainingTimeDisplayRenderer;
		
		_greenTime = e_GreenTime;
		_yellowTime = e_YellowTime;
		_redTime = e_RedTime;
		
		_greenLight = e_GreenLight;
		_yellowLight = e_YellowLight;
		_redLight = e_RedLight;
		
		_engine = e_TrafficLightEngine;
		
		_lightTimer = new Clock();
		
		Reset ();
		
		Active = false;
		UpdateLight();
	}
	
	void Update()
	{
		
		UpdateTimeDisplay();
		
		if (_lightTimer != null)
		{
			_lightTimer.Update(Time.deltaTime);
			
			if (_lightTimer.State == TimerStates.Expired)
				SwitchPhase();
		}
	}
	
	public void CheckpointPassed(bool isDrivingForward)
	{
		if (isDrivingForward)
		{
			// Send information whether the traffic light was passed on red to the traffic light engine
			// (the yellow phase right before the green phase also counts as red! (German StVO))
			bool passedOnRed = ((_phase == TrafficLightStates.Red) || (_phase == TrafficLightStates.ToGreen));
			_engine.TrafficLightPassed(passedOnRed, _lightTimer.TimeLeft);
		}
	}
	
	private void UpdateTimeDisplay()
	{
		int secondsLeft = Mathf.CeilToInt(_lightTimer.TimeLeft);
		
		if ((_phase == TrafficLightStates.ToGreen) || (_phase == TrafficLightStates.ToRed))
			secondsLeft = 0;
		
		_timeDisplay.text = secondsLeft.ToString() + " s";
	}
	
	private void SwitchPhase()
	{
		// Switches to the next traffic light phase
		// ( Red -> Yellow -> Green -> Yellow -> Red -> ... )
		switch (_phase)
		{
		case TrafficLightStates.Red:
			_lightTimer.Stop();
			_lightTimer.MaxTime = _yellowTime;
			_lightTimer.Start();
			
			_phase = TrafficLightStates.ToGreen;
			break;
			
		case TrafficLightStates.ToGreen:
			_lightTimer.Stop();
			_lightTimer.MaxTime = _greenTime;
			_timeDisplayRenderer.material = _greenMaterial;
			_lightTimer.Start();
			
			_phase = TrafficLightStates.Green;
			break;
			
		case TrafficLightStates.Green:
			_lightTimer.Stop();
			_lightTimer.MaxTime = _yellowTime;
			_lightTimer.Start();
			
			_phase = TrafficLightStates.ToRed;
			break;
			
		case TrafficLightStates.ToRed:
			_lightTimer.Stop();
			_lightTimer.MaxTime = _redTime;
			_timeDisplayRenderer.material = _redMaterial;
			_lightTimer.Start();
			
			_phase = TrafficLightStates.Red;
			break;
			
		}
		
		UpdateLight();
	}
	
	private void UpdateLight()
	{
		switch (_phase)
		{
		case TrafficLightStates.Green:
			_greenLight.enabled = true;
			_yellowLight.enabled = false;
			_redLight.enabled = false;
			break;
			
		case TrafficLightStates.ToGreen:
		case TrafficLightStates.ToRed:
			_greenLight.enabled = false;
			_yellowLight.enabled = true;
			_redLight.enabled = false;
			break;
			
		case TrafficLightStates.Red:
			_greenLight.enabled = false;
			_yellowLight.enabled = false;
			_redLight.enabled = true;
			break;				
		}
	}
	
	public void Reset()
	{
		// Set the traffic light to red state and start over
		_lightTimer.Stop();
		_lightTimer.MaxTime = _redTime;
		_timeDisplay.font.material = _redMaterial;
		_phase = TrafficLightStates.Red;
		_lightTimer.Start();
	}
	
	private void LoadMaterials()
	{
		_greenMaterial = (Material) Resources.Load("FontGreen");
		_yellowMaterial = (Material) Resources.Load("FontYellow");
		_redMaterial = (Material) Resources.Load("FontRed");
	}
}

