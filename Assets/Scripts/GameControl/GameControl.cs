// Code of Milan Rüll
using System.Collections;
using UnityEngine;
using System;
using AssemblyCSharp;

public class GameControl : MonoBehaviour, ICheckReceiver, IMasterCheckReceiver
{
	// Texts
	private const string countdownEndText = "! GO !";
	private const string nextLapText = "Lap"; // + Lap Number
	private const string finalLapText = "Final Lap!";
	private const string loseText = "- Time Up -";
	private const string winText = "FINISH!";
	private const string reverseText = "Reverse!";
	
	/**
	 * Editor variables (for initialization only)
	 */
	
	// Consts
	private const int soundChannelCount = 32;
	
	public static float timePointFactor = 100f;
	public static float consumptionPointFactor = 10000f;
	
	public float maxRoundTime = 100.0f; // seconds per round
	public int countdownTime = 5; // seconds
	public int lapsToDrive = 3;
	
	private const float timeFlashDuration = 2; // duration to let the time flash when drained
	private const float timeFlashSpeed = 0.2f; // seconds/flash
	private const float infoTextDisplayDuration = 2.5f; // seconds
	
	// Member Variables
	public CarControl _carScript;
	public TrafficLightManager _trafficLightEngine;
	
	private RoundData _data;
	
	public GUIText _infoTextDisplay;
	
	private int _lapNumber;
	private float[] _lapConsumption;
	
	private GameStates _gameState;
	
	private Clock _lapTimer;
	private Clock _countdown;
	private float _goDisplayed;
	private Clock _flashTimer;
	private float _flashDelta;
	
	private SimpleSoundManager _sounds;
	
	private bool _isInTime;
	private bool _isDrivingWrongWay = false;
	
	public static bool _isAuto = true;
	public static bool _isOnlineHighscore = true;
	public static bool _isSoundOn = true;
	public static bool _useXboxController = false;	
		
	void Start ()
	{							
		GuiControl.lapsToDrive = lapsToDrive;
		_data = new RoundData();
		
		SetupSound();
		
		_carScript.ProcessInput = false;
		_infoTextDisplay.text = "";
		_infoTextDisplay.material.color = Color.red;
		
		_flashTimer = new Clock(timeFlashDuration);
		
		this._gameState = GameStates.PreStart;
		this._lapNumber = 1;
		this._lapConsumption = new float[lapsToDrive];	
		this._lapTimer = new Clock(maxRoundTime);
		this._countdown = new Clock(countdownTime);
		
		// This class will receive all checkpoint passes in the game
		Checkpoint.MasterCheckReceiver = this;
		
		this._countdown.Start();
		
		UpdateGUI();
		PlaySound(GameSounds.BackgroundBirds);
	}
	
	void Update ()
	{
		UpdateTimers();
		UpdateGUI();
		
		if ((_gameState == GameStates.Started) && (Time.timeScale > 0.0001))
			_data.Update(_carScript.CurrentConsumption, Time.deltaTime);
		
		if (_countdown.State == TimerStates.Expired)
			CountdownExpired();		
		
		if ((_lapTimer.State == TimerStates.Expired) && (_gameState == GameStates.Started))
			EndGame(false);
		
		TimeFlash();
	}
	
	// Will be invoked when the player passes the finishing line
	public void CheckpointPassed(bool isDrivingForward)
	{
		if ((_gameState == GameStates.Started) && (isDrivingForward))
		{
			bool result = IncreaseLapNumber();
			
			if (!result)
				EndGame(true);
			else
				_lapTimer.AddTime(maxRoundTime);	
		}	
	}
	
	// Will be invoked when the player passes any checkpoint in the game
	public void AnyCheckpointPassed(bool isDrivingForward)
	{
		// Car is only driving the wrong way when it passes the checkpoint in the wrong direction
		// and is driving forward when it does so
		_isDrivingWrongWay = ((!isDrivingForward) && (!_carScript.IsDrivingBackwards));
		
		if (!_isDrivingWrongWay)
		{
			if (_infoTextDisplay.text == reverseText)
				_infoTextDisplay.text = "";
		}
		else
		{
			_infoTextDisplay.text = reverseText;
			_infoTextDisplay.material.color = Color.red;
		}
	}
	
	// Will be invoked if a traffic light is passed on red
	public void TrafficLightPassedOnRed(float timeLeft)
	{
		// Time reduction on red traffic light pass
		// 3 times the time left of the red traffic light phase
		_lapTimer.AddTime(-timeLeft * 3f);
		PlaySound(GameSounds.RedLightPass);
		
		_flashDelta = 0f;
		_flashTimer.Start();
		GuiControl.showCurrentLapTime = false;
	}
	
	public void PlaySound(GameSounds sound)
	{
		_sounds.PlaySound(sound.ToString());
	}
	
	public void PlaySound(GameSounds sound, float pitch, float volume)
	{
		_sounds.PlaySound(sound.ToString(), pitch, volume);
	}
	
	private void SetupSound()
	{
		GameObject soundObject = new GameObject("Game Sound Channels");
		soundObject.transform.parent = this.transform;
		_sounds = new SimpleSoundManager(soundChannelCount, soundObject);
		
		// Add the sounds and set their volume/pitch etc..
		_sounds.AddSound(GameSounds.CarBump.ToString(), 1.0f, 1.0f, false);
		_sounds.AddSound(GameSounds.RedLightPass.ToString(), 1.0f, 0.3f, false);
		_sounds.AddSound(GameSounds.BackgroundBirds.ToString(), 1.0f, 0.25f, true);
		_sounds.AddSound(GameSounds.Countdown.ToString(), 1.5f, 1f, false);
		_sounds.AddSound(GameSounds.CountdownEnd.ToString(), 1.5f, 1f, false);
		
		_sounds.AddSound(GameSounds.Lap.ToString(), 1.0f, 0.5f, false);
		_sounds.AddSound(GameSounds.Win.ToString(), 1.0f, 1f, false);
	}
	
	private bool IncreaseLapNumber()
	{
		bool result = true;
		
		_lapNumber++;
		
		if (_lapNumber < lapsToDrive)
			ShowInfoText(nextLapText+" "+_lapNumber.ToString()+"/"+lapsToDrive.ToString(), GuiControl.GUIColor);
		if (_lapNumber == lapsToDrive)
			ShowInfoText(finalLapText, GuiControl.GUIColor);
		
		_data.NextRound();
		UpdateGUI();		
		
		if (_lapNumber > lapsToDrive)
			result = false;
		else
			PlaySound(GameSounds.Lap);

		
		return result;
	}
	
	private void CountdownExpired()
	{
		if (_gameState == GameStates.PreStart)
		{
			StartGame();
		}
		else
		{
			_countdown.Stop();
			_infoTextDisplay.text = "";
		}
	}
	
	private void ShowInfoText(string text, Color textColor)
	{
		_countdown.Stop();
		
		_infoTextDisplay.material.color = textColor;
		_infoTextDisplay.text = text;
		_infoTextDisplay.enabled = true;
		
		_countdown.MaxTime = infoTextDisplayDuration;
		_countdown.Start();
	}
	
	private void TimeFlash()
	{
		if (_flashTimer.State == TimerStates.Started)
		{
			if (_flashDelta >= timeFlashSpeed)
			{
				GuiControl.showCurrentLapTime = !GuiControl.showCurrentLapTime;
				_flashDelta = 0;
			}
			
			_flashDelta += Time.deltaTime;
		}
		if (_flashTimer.State == TimerStates.Expired)
		{
			_flashTimer.Stop();
			GuiControl.showCurrentLapTime = true;
		}
	}
	
	private void StartGame()
	{
		_gameState = GameStates.Started;
		
		_infoTextDisplay.material.color = GuiControl.ProtronGreen;
		ShowInfoText(countdownEndText, GuiControl.ProtronGreen);
		PlaySound(GameSounds.CountdownEnd);
		
		_carScript.Unfreeze();
		_carScript.ProcessInput = true;
		_data.Reset();
		
		_trafficLightEngine.StartEngine();
		_lapTimer.Start();
	}
	
	private void EndGame(bool inTime)
	{
		_isInTime = inTime;
		
		_gameState = GameStates.Over;
		_lapTimer.Pause();
		_infoTextDisplay.enabled = true;
		
		_carScript.ProcessInput = false;
		
		if (inTime)
		{
			PlaySound(GameSounds.Win);
			
			_infoTextDisplay.material.color = GuiControl.ProtronBlue;
			_infoTextDisplay.text = winText;
			
			int consumptionPoints = (int)(consumptionPointFactor / GuiControl.totalConsumption);
			int timePoints = (int)((_lapTimer.TimeLeft / 60.0f) * timePointFactor);
			
			GuiControl.timePoints = timePoints;
			GuiControl.consumptionPoints = consumptionPoints;
			
			Highscore.score = timePoints + consumptionPoints;
			Highscore.consumption = GuiControl.totalConsumption;
		}
		else
		{
			_infoTextDisplay.material.color = Color.red;
			_infoTextDisplay.text = loseText;
		}
		
		GuiControl.showBlackground = true;
		StartCoroutine("EndgameCoroutine");
	}
	
	// Code of Christoph Jansen //
	private IEnumerator EndgameCoroutine()
	{
		yield return new WaitForSeconds(5.0f);
		
		if(_isInTime)
		{
			Application.LoadLevel(HighscoreCheck());
		}
		else
		{
			Application.LoadLevel(0);
		}
	}
	
	private int HighscoreCheck()
	{
		string isAutoString;
		if(_isAuto)
			isAutoString = "auto";
		else
			isAutoString = "manu";
		Highscore.isAuto = _isAuto;
		if(!PlayerPrefs.HasKey("0name"+isAutoString) || Highscore.score > PlayerPrefs.GetInt("9score"+isAutoString) || _isOnlineHighscore)
			return 2;
		else
			return 0;
	}
	// End
	
	private void UpdateGUI()
	{			
		if (Time.timeScale > 0.0001)
		{
			GuiControl.consumption = _data.GetAverageConsumption(_lapNumber);
					
			int i=0;
			float totalAverage = 0.0f;
			
			for (; i < _lapNumber-1; i++)
			{
				_lapConsumption[i] = _data.GetAverageConsumption(i+1);
				totalAverage += _lapConsumption[i];
			}
			for (; i < lapsToDrive; i++)
				_lapConsumption[i] = 0;
			
			GuiControl.lapConsumption = _lapConsumption;
			if (_lapNumber > 1)
				GuiControl.totalConsumption = totalAverage / (_lapNumber-1);
			else
				GuiControl.totalConsumption = totalAverage;		
			
			GuiControl.laps = _lapNumber;
			GuiControl.currentLapTime = (int)(_lapTimer.TimeLeft * 10);
			
			if (_gameState == GameStates.PreStart)
			{
				string lastText = _infoTextDisplay.text;
				_infoTextDisplay.text = Mathf.Ceil(_countdown.TimeLeft).ToString();
				if (lastText != _infoTextDisplay.text)
					PlaySound(GameSounds.Countdown);
			}
		}
	}
	
	private void UpdateTimers()
	{
		if (_lapTimer != null)
			_lapTimer.Update(Time.deltaTime);
		if (_countdown != null)
			_countdown.Update(Time.deltaTime);
		if (_flashTimer != null)
			_flashTimer.Update(Time.deltaTime);
	}
}






















