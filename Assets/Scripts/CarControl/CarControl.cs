using System;
using UnityEngine;
using System.Collections;
using AssemblyCSharp;

public struct Wheel
{
	public WheelFrictionCurve wfc;
	public WheelCollider collider;
	
	public Transform tireGraphic;
	public Transform wheelGraphic;
	
	public bool driveWheel;
	public bool steerWheel;
	
	public float radius;
}

public class CarControl : MonoBehaviour
{
	// Consts
	public string carDataFilePath = "Assets/Cardata/aeris.txt";	
	
	public GameControl e_GameControl;
	
	public const float suspensionRange = 0.02f;
	public const float suspensionDamper = 1f;
	public const float suspensionSpringFront = 18500f;
	public const float suspensionSpringRear = 9000f;
	
	public const float maxKmh = 100;
	public const float gasPower = 9.0f; // kWh / litre ("super benzin")
	
	public const float brakeForce = 18000; // Newton
	
	private const float maxFlippingAngle = 25.0f; // Prevent flipping beyond that angle
		
	private const int maximumTurn = 13; // max and min turning angles (depends on the speed)
	private const int minimumTurn = maximumTurn;
	
	private const float steeringDamper = 1f; // causes steering wheels not to 'jump' into their direction
											   // higher value equals faster steering (1.0 = instant)
	private const float gearSwitchDelayTime = 0.5f; // seconds between gear switches if you hold the button
	
	public const float wheelRadius = 0.35f; // metres
	
	private const float standardLoopFadeTime = 0.1f;
		
	// Variables
	public GUIText debugText; 
	
	private bool processInput = false;
	
	private int drivingDirection = +1; // +1: forward, -1: reverse, 0: stopped
	private float kmh;
	
	private float currentConsumption;
	private float totalConsumption;
	
	public Transform carBody;
	
	public Transform[] frontWheels = null;
	public Transform[] rearWheels = null;
	private Wheel[] wheels;
	
	private AntiRoll frontAntiRoll;
	private AntiRoll rearAntiRoll;
	
	public Transform centerOfMass = null;
	
	private float throttle = 0.0f;
	private float steer = 0.0f;		
	private float gearDelay = 0f;
	private bool engineStartPressed = false;
	private float clutchButton = 0; // Is the Button for the clutch pressed?
	
	private bool canSteer = true;
	private bool canDrive = true;
	
	private WheelFrictionCurve wfcSideways;
	private WheelFrictionCurve wfcForward;
	
	private CarEngine _engine;
	private GameControl _gameControl;
	
	/**
	  * Properties
	  */
	public bool ProcessInput
	{
		get { return processInput; }
		set
		{
			// Release the clutch when player input is turned off
			if ((!value) && (_engine != null))
				_engine.ClutchPressed = false;
				
			processInput = value;
		}
	}
	
	public float TotalConsumption
	{
		get { return totalConsumption; }
	}
		
	public float CurrentConsumption
	{
		get { return currentConsumption; }
	}
	
	public bool IsDrivingBackwards
	{
		get { return (drivingDirection < 0); } 
	}
		
	/**
	  * Methods
	  */
	void Start () 
	{		
		if (e_GameControl != null)
			_gameControl = e_GameControl;
		else
			throw new UnassignedReferenceException("The editor member 'e_GameControl' of CarControl must be assigned.");
		
		drivingDirection = 1;
		kmh = 0.0f;
		currentConsumption = 0.0f;
				
		_engine = new CarEngine(carDataFilePath);
		
		SetupCenterOfMass();		
		SetupWheels();
		
		SetupSound();
		
		// One AntiRoll for each wheel-axis
		frontAntiRoll = new AntiRoll(wheels[0], wheels[1]);
		rearAntiRoll = new AntiRoll(wheels[2], wheels[3]);
	}
	
	void Update ()
	{						
		GetInput();	
		UpdateGUI();
		_engine.Update();
	}
	
	void FixedUpdate()
	{
		if (wheels != null)
		{
			Vector3 relativeVelocity = this.transform.InverseTransformDirection(this.rigidbody.velocity);
			float wheelPerimeter = (2f * wheels[0].radius * Mathf.PI);
			float wheelRPM = 60f * (Mathf.Abs(relativeVelocity.z) / wheelPerimeter);		
			
			UpdateKmh(wheelRPM);
			UpdateDrivingDirection(relativeVelocity);
			
			_engine.FixedUpdate(wheelRPM, throttle, kmh, drivingDirection);							
			UpdateConsumption();
						
			CheckForGrounding();
			PreventFlipping();
				
			ApplyThrottle(relativeVelocity);	
			ApplySteer(relativeVelocity);
			UpdateWheelGraphics(wheelRPM);
			
			frontAntiRoll.ApplyAntiRoll(rigidbody);
			rearAntiRoll.ApplyAntiRoll(rigidbody);
		}
	}
	
	// Play the bump sound when we collide
	void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.material.name == "Metal (Instance)")
		{
			float crashIntensity = kmh / maxKmh;
			_gameControl.PlaySound(GameSounds.CarBump, 1.0f, crashIntensity);
		}
	}
		
	private void GetInput()
	{
		if ((processInput) && (Time.timeScale > 0.0001))
		{
			float steerAxis;
			float carStart;
			float gearAxis;
			
			// Get all Axis based on the input
			
			// Keyboard
			if (!GameControl._useXboxController)
			{
				throttle = Input.GetAxis("Vertical");
				
				steerAxis = Input.GetAxis("Horizontal");
				clutchButton = Input.GetAxis("Clutch");
				carStart = Input.GetAxis("EngineStart");
				gearAxis = Input.GetAxis("Gear");	
			}
			
			// XBox-Controller
			else
			{
				throttle = Input.GetAxis("VerticalX");
				steerAxis = Input.GetAxis("HorizontalX");
				gearAxis = Input.GetAxis("GearX");	;
				
				if (Input.GetKeyDown(KeyCode.JoystickButton7))
					carStart = 1;
				else
					carStart = 0;
				
				if (Input.GetKeyDown(KeyCode.JoystickButton4))
					clutchButton = 1;
				if (Input.GetKeyUp(KeyCode.JoystickButton4))
					clutchButton = 0;
			}
			
			steer += (steerAxis - steer) * steeringDamper;	
			if (GameControl._isAuto)
				clutchButton = 0;
						
			if (gearDelay > gearSwitchDelayTime)
			{
				if (gearAxis > 0.5)
				{
					_engine.IncreaseGear();	
					gearDelay = 0f;
				}
				if (gearAxis < -0.5)
				{
					_engine.DecreaseGear();
					gearDelay = 0f;
				}
			}
			gearDelay += Time.deltaTime;
			if (Mathf.Abs(gearAxis) < 0.01)
				gearDelay = 1f;
			
			// Clutch
			if (clutchButton > 0.01)
				_engine.ClutchPressed = true;
			else
				_engine.ClutchPressed = false;
			
			// Engine Start
			if ((carStart > 0.01) && (!engineStartPressed))
			{
				_engine.PowerOn = !_engine.PowerOn;
				engineStartPressed = true;
			}
			
			if (carStart < 0.001)
				engineStartPressed = false;
		}
	}
					
	private void ApplyThrottle(Vector3 relativeVelocity)
	{
		if (canDrive)
		{		
	        float force = 0;
			int intendedDrivingDirection = drivingDirection;
			// If the car stands still, use the intended driving direction
			if ((intendedDrivingDirection == 0) && (Mathf.Abs(throttle) > 0.001))
			{
				if (_engine.InReverseGear)
					intendedDrivingDirection = -1;
				else
					intendedDrivingDirection = 1;
			}
			
			if (processInput)
			{	
				// Only brake, if the player wants to drive into the opposite direction and is not standing still yet
				if ((Utility.HaveTheSameSign(throttle, intendedDrivingDirection)) ||
						(Mathf.Abs(throttle) < 0.0001f))
				{
					force += _engine.WheelTurningMoment / wheels[0].radius; // in Newton
				}
				else if (Mathf.Abs(relativeVelocity.z) > 0.1)
				{
					force -= brakeForce;
				}
			}
			
			// Brake if player input is turned off or the engine is turned off and clutch is not pressed
			if ((!processInput) || ((!_engine.PowerOn) && (!_engine.ClutchPressed)))
			{
				if (Mathf.Abs(relativeVelocity.z) > 0.1f)
					force = -(brakeForce / 2f);
				else
					rigidbody.velocity = new Vector3(0, 0, 0);
			}

			this.rigidbody.AddForce(this.transform.forward * force * intendedDrivingDirection, ForceMode.Force);
			
//			if (debugText != null)
//				debugText.text = _engine.WheelTurningMoment.ToString("0.0");
		}
	}
	
	private void ApplySteer(Vector3 relativeVelocity)
	{
		if (canSteer)
		{
			if (Mathf.Abs(steer) > 0.001f)
			{
				float minMaxTurn = EvaluateTurn();
				float turnRadius = 3.0f / Mathf.Sin((90 - (steer * 30)) * Mathf.Deg2Rad);
				float turnSpeed = Mathf.Clamp(relativeVelocity.z / turnRadius, -minMaxTurn / 10, minMaxTurn / 10);				
				
				transform.RotateAround(transform.position + transform.right * turnRadius * Mathf.Sign(steer),
									   transform.up,
									   turnSpeed * Time.deltaTime * steer * Mathf.Rad2Deg);
			}
		}
	}
	
	/**
	 * Update functions
	 */
	private void UpdateWheelGraphics(float wheelRPM)
	{
		foreach (Wheel w in wheels)
		{
			// Driving rotation
			float spinningAngle = (wheelRPM * 360) * (Time.deltaTime / 60);
			w.tireGraphic.RotateAround(w.wheelGraphic.right, drivingDirection * spinningAngle * Mathf.Deg2Rad);
			
			// Steering rotation
			if (w.steerWheel)
			{
				const float maxRotationAngle = 28; // absolute degrees
				float rotationAngle = Mathf.LerpAngle(0,
												  Mathf.Sign(steer) * maxRotationAngle * Mathf.Deg2Rad,
												  Mathf.Abs(steer)) * Mathf.Rad2Deg;
				
				Vector3 rotationEulerAngles = w.wheelGraphic.parent.localEulerAngles;
				rotationEulerAngles.y = rotationAngle;
				
				w.wheelGraphic.parent.localEulerAngles = rotationEulerAngles;
			}
		}
	}
	
	private void UpdateKmh(float wheelRPM)
	{
		if (processInput)
		{
			kmh = 3.6f * ((wheelRPM * (2f * wheels[0].radius * Mathf.PI)) / 60f);
			
			if (kmh < 0.01)
			{
				kmh = 0f;
				rigidbody.velocity = new Vector3(0, 0, 0);
			}
		}
	}
	
	private void UpdateConsumption()
	{
		float currentKW = _engine.KW;
		
		if ((currentKW > 0.001f) || (_engine.ClutchPressed))
		{			
			// Don't calculate with anything under 1 kmh, else the value will get too damn high
			float currentKmh = kmh;
			if (currentKmh < 1f)
				currentKmh = 1f;
			
			this.currentConsumption = currentKW / gasPower; // liter per hour	
			this.currentConsumption /= currentKmh; // liter per km
			this.currentConsumption *= 100; // liter per 100km
		}
		else
			this.currentConsumption = 0.0f;
		
		this.totalConsumption += (this.currentConsumption / 100f) * (kmh * (Time.deltaTime / 3600.0f));				
	}	
	
	private void UpdateDrivingDirection(Vector3 relativeVelocity)
	{
		drivingDirection = 0;
		
		if (relativeVelocity.z < -0.1f)
			drivingDirection = -1;
		if (relativeVelocity.z > 0.1f)
			drivingDirection = +1;
		
		if ((drivingDirection > 0) && (_engine.Gear < 1))
			_engine.IncreaseGear();
		if ((drivingDirection < 0) && (_engine.Gear > 0))
			_engine.InReverseGear = true;
	}
	
	private void UpdateGUI()
	{
		TachoControl.velocity = kmh;
		TachoControl.rpm = _engine.RPM;

		GuiControl.gear = _engine.Gear;
	}
	
	public void ResetTotalConsumption()
	{
		this.totalConsumption = 0.0f;
	}
	
	private void CheckForGrounding()
	{
		canSteer = false;
		canDrive = false;
		
		foreach (Wheel w in wheels)
		{
			if (w.collider.isGrounded)
			{				
				if (w.driveWheel)
				{
					canDrive = true;
				}
				if (w.steerWheel)
				{
					canSteer = true;
				}
			}
		}
	}
	
	private void PreventFlipping()
	{
		float currentFlipAngle = transform.localRotation.eulerAngles.z;
		if (currentFlipAngle > 180.0f)
			currentFlipAngle -= 360.0f;
		
		// Prevent flipping beyond maxFlippingAngle
		if (Mathf.Abs(currentFlipAngle) > maxFlippingAngle)
		{
			transform.RotateAround(transform.forward, -(currentFlipAngle - (maxFlippingAngle * Mathf.Sign(currentFlipAngle))) * Mathf.Deg2Rad);
//			rigidbody.AddForce(transform.right * Mathf.Sign(currentFlipAngle) * brakeForce);
		}		
	}
	
	public void Unfreeze()
	{
		this.rigidbody.isKinematic = false;
	}
	
	/**
	 * Utility Functions	
	 */
	float EvaluateTurn()
	{
		if(kmh > maxKmh)
			return minimumTurn;
		
		float speedIndex = 1 - (kmh / maxKmh);
		return minimumTurn + speedIndex * (maximumTurn - minimumTurn);
	}

	
	/**
	  * Setup Functions	
	  */
	private void SetupCenterOfMass()
	{
		if (centerOfMass != null)
		{
			rigidbody.centerOfMass = centerOfMass.localPosition;
		}
	}
	
	private void SetupSound()
	{
		// Car-Engine Sound:
		// Create a new game object and add 4 audio sources
		// (2 for each audio player which can loop a single sound file
		//  and 2 audio players to fade between two different sound files)
		// Then load all engine sounds and assign the resulting DriveSoundManager to the engine
		GameObject engineSoundObject = new GameObject("Car Engine Sound");
		engineSoundObject.transform.parent = this.transform;
		
		
		SmoothSoundPlayer player1 = new SmoothSoundPlayer(engineSoundObject.AddComponent<AudioSource>(),
														engineSoundObject.AddComponent<AudioSource>());
				
		SmoothSoundPlayer player2 = new SmoothSoundPlayer(engineSoundObject.AddComponent<AudioSource>(),
														engineSoundObject.AddComponent<AudioSource>());
		
		SmoothSoundManager engineSoundManager = new SmoothSoundManager(player1, player2);
						
		_engine.LoadSounds(engineSoundManager);
	}
	
	private void SetupWheels()
	{
		SetupWheelFrictionCurves();
		
		wheels = new Wheel[frontWheels.Length + rearWheels.Length];
		
		int currentWheel = 0;		
		foreach (Transform t in frontWheels)
		{
			wheels[currentWheel] = CreateWheel(t, true);
			currentWheel++;
		}
		
		foreach (Transform t in rearWheels)
		{
			wheels[currentWheel] = CreateWheel(t, false);
			currentWheel++;
		}
	}
	
	private Wheel CreateWheel(Transform t, bool isFrontWheel)
	{		
		GameObject obj = new GameObject(t.name + " Collider");
		obj.transform.position = t.position;
		obj.transform.parent   = this.transform;
		obj.transform.rotation = t.rotation;
		
		WheelCollider wc = obj.AddComponent<WheelCollider>();
		wc.suspensionDistance = suspensionRange;
		JointSpring js = wc.suspensionSpring;
		
		if (isFrontWheel)
			js.spring = suspensionSpringFront;
		else
			js.spring = suspensionSpringRear;
			
		js.damper = suspensionDamper;
		wc.suspensionSpring = js;
		
		// Setup wheel
		Wheel wheel = new Wheel();
		wheel.wfc = wfcSideways;
		
		wheel.collider = wc;
		wheel.collider.sidewaysFriction = wfcSideways;
		wheel.collider.forwardFriction = wfcForward;
		wheel.steerWheel = isFrontWheel;
		wheel.driveWheel = !isFrontWheel;
		
		wheel.wheelGraphic = t;
		wheel.tireGraphic = t.GetComponentsInChildren<Transform>()[1];
		
		float wheelRendererRadius = wheel.tireGraphic.renderer.bounds.size.y / 2;	
		wheel.collider.radius = wheelRendererRadius;
		
		// Fix the radius
		wheel.radius = wheel.collider.radius;
				
		return wheel;
	}
	
	private void SetupWheelFrictionCurves()
	{
		wfcSideways = new WheelFrictionCurve();
		wfcSideways.extremumSlip = 0.1f;
		wfcSideways.extremumValue = 20000; //4000
		wfcSideways.asymptoteSlip = 4f;
		wfcSideways.asymptoteValue = 4000; //1900
		wfcSideways.stiffness = 0.2f;
		
		wfcForward = new WheelFrictionCurve();
		wfcForward.extremumSlip = 1;
		wfcForward.extremumValue = 0;
		wfcForward.asymptoteSlip = 2;
		wfcForward.asymptoteValue = 0;
		wfcForward.stiffness = 0.08f;
	}
}



























