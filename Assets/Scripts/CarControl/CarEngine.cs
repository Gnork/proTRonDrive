using System.Collections;
using UnityEngine;

namespace AssemblyCSharp
{
	public class CarEngine
	{	
		private const float maxCarSoundPitch = 1.5f;
		private const float maxGasSoundPitch = 1.4f;
		
		private const float maxRPM = 6700;
		// The RPM amount at which the engine shuts off
		private const float minRPM = 800;
		
		// The RPM amount in idle state
		private const float idleRPM = 1000;
		// The amount of kw the car uses when it's in idle state
		private const float idleKWUsage = 5.314716f;
		// Gain or lose RPM whether the gas pedal is pressed or not by a value of idleRPMGain per second
		private const float idleRPMGain = 10000;
		 
		// The RPM will not go instantly to the target RPM
		// The higher this damper value is, the faster it will adjust itself
		// Value means Percentage of RPM per second/100
		private const float RPMDamper = 0.1f;
		// Clutch doesnt go instantly from 1 to zero,
		// it will go down smoothly by this value per fixed update
		private const float clutchDamperValue = 0.11f;
		
		/**
		  * Member Variables
		  */
		private float _currentRPM;
		private float _targetRPM;
		
		private float _throttle;
		
		private CarData _carData;
	
		private double _actualTurningMoment = 0.0f;
		private double _maxTurningMoment = 0.0f;
		
		private float _finalDriveRatio;
		private int _currentGear;
		private float[] _gears;
		
		// If the clutch is being pressed at the moment
		private bool _clutchPressed = false;
		// When you release the clutch it doesnt go instantly to zero but smoothly from 1 to 0
		// with a step size per fixed update of the const clutchDamperValue
		private float _clutchValue = 0f;
		private bool _started = false;
		
		private SmoothSoundManager _soundManager = null;
		// Save the engine sound in a seperate variable to manipulate its pitch
		private DriveSound _engineSound;
		private DriveSound _gasSound;
		private bool _isAccelerating = false;
		
		private int _drivingDirection;
				
		/**
		 * Properties
		 */ 	
		public bool ClutchPressed
		{
			get { return this._clutchPressed; }
			set
			{
				if (value)
					_clutchValue = 1.0f;
				
				this._clutchPressed = value;
			}
		}
		
		public int Gear
		{
			get { return this._currentGear; }
		}
		
		public float RPM
		{
			get { return this._currentRPM; }
		}
				
		public float WheelTurningMoment
		{
			get
			{
				// Don't return any turning moment if engine is idle or not started
				if ((_clutchPressed) || (!_started))
					return 0;
				
				return (float)_actualTurningMoment * _gears[_currentGear] * _finalDriveRatio;
			}
		}
		
		public float KW
		{
			get
			{
				if (_started)
				{
					// return usual consumption if the clutch is not pressed
					// or if the clutch is pressed but the gas pedal is also pressed
					if ((!_clutchPressed) || (_throttle > 0.01))
					{
						float W = (_currentRPM / 60) * Mathf.Abs((float)_maxTurningMoment * Mathf.Abs(_throttle)) * 2f * Mathf.PI;
						return W / 1000.0f;
					}
					
					// return the consumption it needs to hold the engine on idleRPM
					// this value is constant
					else
					{
						return idleKWUsage;
					}
				}
				else
					return 0;
			}
		}
		
		public bool PowerOn
		{
			set
			{			
				// only start if it's off and only turn off if it's on
				if ((!_started) && (value))
					StartEngine();				
				
				if ((_started) && (!value))
					StopEngine();	
				
				_started = value;
			}
			get { return _started; }
		}
		
		public bool InReverseGear
		{
			get { return (_currentGear == 0); }
			set
			{
				if (value)
					_currentGear = 0;
				if ((!value) && (_currentGear == 0))
					_currentGear = 1;
			}
		}
		
		/**
		 * Constructors
		 */ 
		public CarEngine(string carDataFilePath)
		{
			_carData = new CarData(carDataFilePath);
			
			_currentRPM = minRPM;
			_actualTurningMoment = 0.0f;
			_throttle = 0.0f;
			
			SetupGears();
		}
		
		/**
		 * Methods
		 */ 
		public void FixedUpdate(float wheelRPM, float throttle, float kmh, int drivingDirection)
		{	
			_drivingDirection = drivingDirection;
			_throttle = throttle;
			
			// Automatically switch into reverse or 1st gear if standing still and trying to drive into one direction
			if ((_throttle < 0) && (drivingDirection == 0) && (_currentGear == 1))
				InReverseGear = true;
			if ((throttle > 0) && (drivingDirection == 0) && (_currentGear == 0))
				InReverseGear = false;
			
			// Invert throttle in reverse gear
			if (InReverseGear)
				_throttle *= -1;
			
			UpdateTurningMoment();
			
			UpdateRPM(wheelRPM);
			// Release clutch softly if not pressed anymore
			if ((_clutchValue > 0.0001f) && (!_clutchPressed))
			{
				_clutchValue -= clutchDamperValue;
				if (_clutchValue <= 0.001)
					_clutchValue = 0f;
			}			
		}
		
		public void Update()
		{
			_soundManager.Update(Time.deltaTime);
		}
		
		public void LoadSounds(SmoothSoundManager soundManager)
		{
			_soundManager = soundManager;
			// Load all Sounds into the sound manager
			_engineSound = _soundManager.AddSound(CarEngineSounds.CarDrive.ToString(), 1.0f, 1.0f, true, 1f);
			_soundManager.AddSound(CarEngineSounds.CarStart.ToString(), 1.0f, 1.0f, false, 0);
			_soundManager.AddSound(CarEngineSounds.CarStop.ToString(), 1.0f, 1.0f, false, 0.5f);
			_gasSound = _soundManager.AddSound(CarEngineSounds.CarGas.ToString(), 1.0f, 1.0f, true, 0.6f);
		}
		
		
		private void AdjustSoundPitch()
		{
			// Adjust the motor sound pitch to the current RPM
			// minPitch at minRPM, max pitch at maxRPM
			_engineSound.Pitch = 1 + ((_currentRPM - minRPM) / (maxRPM - minRPM)) * (maxCarSoundPitch - 1);
			_gasSound.Pitch = 1 + ((_currentRPM - minRPM) / (maxRPM - minRPM)) * (maxGasSoundPitch - 1);
		}
		
		private void StartEngine()
		{
			_currentRPM = minRPM + 200;
			
			// Play Car Start sound and then fade over to the usual driving sound
			PlaySound(CarEngineSounds.CarStart, 0f);
			PlaySound(CarEngineSounds.CarDrive, 3f);
			AdjustSoundPitch();
		}
		
		private void StopEngine()
		{
			_targetRPM = 0;
			_currentGear = 1;			
			
			_soundManager.PlaySound(CarEngineSounds.CarStop.ToString(), 0);
		}	
		
		/**
		 * Update Methods
		 */
		private void UpdateRPM(float wheelRPM)
		{
			if (_started)
			{
				if ((!_clutchPressed) || (GameControl._isAuto))
				{
					_targetRPM = wheelRPM * _gears[_currentGear] * _finalDriveRatio;	
					if ((_clutchValue > 0.0001f) && (!GameControl._isAuto))
					{
						// Interpolate between idleRPM and the real target RPM, depending on clutch value
						_targetRPM = (_clutchValue * idleRPM) + ((1-_clutchValue) * _targetRPM);
					}
				}
				else
				{
					// Gain or lose RPM whether the gas pedal is pressed or not by a value of idleRPMGain per second
					_currentRPM += (-idleRPMGain + (idleRPMGain * 2 * _throttle)) * Time.fixedDeltaTime;
					_targetRPM = _currentRPM;
				}
				
				// Stop the motor if it falls under minRPM
				// hold it on idleRPM if the clutch is pressed
				if (_currentRPM < minRPM)
				{
					// Hold the motor on idle rpm if in automatic mode - but only on gear 1 and reverse gear
					if ((_clutchPressed) ||
						((GameControl._isAuto) && (_currentGear <= 1)))
					{
						_currentRPM = idleRPM;
						_targetRPM = _currentRPM;
						
/*						if ((_automatic) && (_currentGear > 1))
						{
							// _currentGear = 1;
							PowerOn = false;
						}*/
					}
					else
					{
						PowerOn = false;
						_currentGear = 1;
					}
				}
				
				// Cut at maxRPM
				if (_currentRPM > maxRPM)
				{
					_currentRPM = maxRPM;
					_targetRPM = maxRPM;
				}
				
				AdjustSoundPitch();
			}
			
			// Adjust the current RPM to the target RPM
			_currentRPM += (_targetRPM - _currentRPM) * RPMDamper * (Time.deltaTime * 100f);			
		}
		
		private void UpdateTurningMoment()
		{				
			if (_started)
			{
				_maxTurningMoment = _carData.GetMaxTurningMoment(_currentRPM);
				
				_actualTurningMoment = (_maxTurningMoment * Mathf.Abs(_throttle)) - _carData.GetNeededTurningMoment(_currentRPM, _currentGear);	
				
				// Braking through engine:
				if (Mathf.Abs(_throttle) < 0.001f)
					_actualTurningMoment -= (_maxTurningMoment / 3);
			}
			else
				_actualTurningMoment = - _carData.GetNeededTurningMoment(_currentRPM, _currentGear);
		}
		
		/*
		 * Setup Methods
		 */
		private void SetupGears()
		{
			double[] gearTranslations = _carData.GearTranslations;
			
			// gears[] contains the gear translations for each gear
			// gear 0 = reverse gear
			_gears = new float[gearTranslations.Length];
			
			for (int i=0; i < gearTranslations.Length; i++)
				_gears[i] = (float)gearTranslations[i];
			
			// The ratio between driveshaft turns and axleshaft turns
			_finalDriveRatio = (float)_carData.FinalDrive;
			
			_currentGear = 1;
		}
		
		/**
		 * Public Control Interface
		 */
		public void PlaySound(CarEngineSounds sound, float fadeTime)
		{
			if (_soundManager != null)
			{
				_soundManager.PlaySound(sound.ToString(), fadeTime);
			}
		}
				
		public void IncreaseGear()
		{
			if ((ClutchPressed) || (GameControl._isAuto))
			{
				_currentGear++;
				
				if (_currentGear > _gears.Length-1)
				{
					// Stay in gear 5 in automatic mode
					if (GameControl._isAuto)
						_currentGear = _gears.Length-1;
					
					// Switch from gear 5 to 1 in manual mode
					else
						_currentGear = 1;
				}
			}
		}
		
		public void DecreaseGear()
		{
			if ((ClutchPressed) || (GameControl._isAuto))
			{
				if (_currentGear-1 >= 0)
					_currentGear--;
			}
		}
	}
}
























































