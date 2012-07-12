//Christoph Jansen

using UnityEngine;
using System.Collections;

public class Options: MonoBehaviour 
{
	//GUIStyle provides styling features in the editor
	public GUIStyle e_myStyle;
	public GUIStyle e_focusedStyle;
	
	private GUIStyle _myStyle;
	private GUIStyle _focusedStyle;
	
	private GUIStyle _usedStyle;
	
	private bool _isAuto;
	private bool _isSoundOn;
	private bool _isOnlineHighscore;
	private bool _isXboxController;
	
	private string[] buttonNames = {"Automatic Mode", "Sound On", "Online Highscore", "XBOX Controller", "OK"};
	private bool[] buttons;
	private int currentSelection = 0;
	
	private float buttonDelay = 0.0f;
	private float buttonDelayTime = 0.25f;
	
	private string buttonText = "hallo";
	
	void Start()
	{
		//Assigning public members to private members
		if(e_myStyle != null)
			_myStyle = e_myStyle;
		else
			throw new UnassignedReferenceException("The editor member 'e_myStyle' of Options must be assigned");
		
		if(e_focusedStyle != null)
			_focusedStyle = e_focusedStyle;
		else
			throw new UnassignedReferenceException("The editor member 'e_notActive' of Options must be assigned");
		
		this._isAuto = GameControl._isAuto;
		this._isSoundOn = GameControl._isSoundOn;
		this._isOnlineHighscore = GameControl._isOnlineHighscore;
		this._isXboxController = GameControl._useXboxController;
		
		buttons = new bool[buttonNames.Length];
	}
	
	//Creates menu buttons
	void OnGUI()
	{
		for(int i = 0; i < buttonNames.Length; ++i)
		{
			if(i == currentSelection)
				_usedStyle = _focusedStyle;
			else
				_usedStyle = _myStyle;
			//GUI.SetNextControlName(buttonNames[i]);
			buttons[i] = false;
			if(i == 0){
				if(_isAuto)
					buttonText = "Automatic Mode";
				else
					buttonText = "Manual Mode";
			}
			else if(i == 1){
				if(_isSoundOn)
					buttonText = "Sound On";
				else
					buttonText = "Sound Off";
			}
			else if(i == 2){
				if(_isOnlineHighscore)
					buttonText = "Online Highscore";
				else
					buttonText = "Offline Higscore";
			}
			else if(i == 3){
				if(_isXboxController)
					buttonText = "XBOX Controller";
				else
					buttonText = "Keyboard";
			}
			else if(i == 4){
				buttonText = "Back";
			}
			GUI.Button(new Rect(55,150+i*75,600,75), buttonText, _usedStyle);
		}
		
		if(buttonDelay > buttonDelayTime)
		{
		    if ((Input.GetKeyDown(KeyCode.DownArrow)) ||
				(Input.GetAxis("MenuX")) < -0.1f) {
	        	currentSelection = (currentSelection+1)%buttonNames.Length;
				buttonDelay = 0.0f;
	    	}
			if ((Input.GetKeyDown(KeyCode.UpArrow)) ||
				(Input.GetAxis("MenuX")) > 0.1f) {
				currentSelection = (currentSelection-1+buttonNames.Length)%buttonNames.Length;
				buttonDelay = 0.0f;
	    	}
			
			if ((Input.GetKeyDown(KeyCode.Return)) ||
			(Input.GetAxis("MenuSelectX") > 0.1f)) {
				buttons[currentSelection] = true;
				buttonDelay = 0;
			}
		}
		else
		{
			buttonDelay += Time.deltaTime;
		}
		
		
		if(buttons[0]){
			_isAuto = !_isAuto;
		}
		if(buttons[1]){
			_isSoundOn = !_isSoundOn;
		}
		if(buttons[2]){
			_isOnlineHighscore = !_isOnlineHighscore;
		}
		if(buttons[3]){
			_isXboxController = !_isXboxController;
		}
		if(buttons[4]){
			GameControl._isAuto = this._isAuto;
			GameControl._isSoundOn = this._isSoundOn;
			GameControl._isOnlineHighscore = this._isOnlineHighscore;
			GameControl._useXboxController = this._isXboxController;
			Application.LoadLevel(0);
		}
	}
}
