//Christoph Jansen

using UnityEngine;
using System.Collections;

public class ChooseHighscore : MonoBehaviour 
{
	//GUIStyle provides styling features in the editor
	public GUIStyle e_myStyle;
	public GUIStyle e_focusedStyle;
	
	private GUIStyle _myStyle;	
	private GUIStyle _focusedStyle;
	private GUIStyle _usedStyle;
	
	private bool[] buttons;
	private int currentSelection = 0;
	
	private float buttonDelay = 0.0f;
	private float buttonDelayTime = 0.25f;
	
	void Start()
	{
		//Assigning public members to private members
		if(e_myStyle != null)
			_myStyle = e_myStyle;
		else
			throw new UnassignedReferenceException("The editor member 'e_myStyle' of ChooseHighscore must be assigned");
		if(e_focusedStyle != null)
			_focusedStyle = e_focusedStyle;
		else
			throw new UnassignedReferenceException("The editor member 'e_focusedStyle' of ChooseHighscore must be assigned");
		buttons = new bool[3];
	}
	
	//Creates menu buttons
	void OnGUI()
	{
		for(int i = 0; i < 3; ++i)
		{
			buttons[i] = false;
		}
		
		if(currentSelection == 0)
			_usedStyle = _focusedStyle;
		else
			_usedStyle = _myStyle;		
		GUI.Button(new Rect(55,150,300,75), "Automatic", _usedStyle);
		
		if(currentSelection == 1)
			_usedStyle = _focusedStyle;
		else
			_usedStyle = _myStyle;		
		GUI.Button(new Rect(55,225,300,75), "Manual", _usedStyle);
		
		if(currentSelection == 2)
			_usedStyle = _focusedStyle;
		else
			_usedStyle = _myStyle;		
		GUI.Button(new Rect(55,480,300,75), "Main Menu", _usedStyle);
		
		if(buttonDelay > buttonDelayTime)
		{
		    if ((Input.GetKeyDown(KeyCode.DownArrow)) ||
				(Input.GetAxis("MenuX")) < -0.1f) {
	        	currentSelection = (currentSelection+1)%3;
				buttonDelay = 0.0f;
	    	}
			 if ((Input.GetKeyDown(KeyCode.UpArrow)) ||
				(Input.GetAxis("MenuX")) > 0.1f) {
				currentSelection = (currentSelection-1+3)%3;
				buttonDelay = 0.0f;
	    	}
		}
		else
		{
			buttonDelay += Time.deltaTime;
		}

		if ((Input.GetKeyDown(KeyCode.Return)) ||
			(Input.GetAxis("MenuSelectX") > 0.1f)) {
			buttons[currentSelection] = true;
		}
		
		if(buttons[0]){
			Highscore.isAuto = true;
			Application.LoadLevel(3);
		}
		if(buttons[1]){
			Highscore.isAuto = false;
			Application.LoadLevel(3);
		}
		if(buttons[2])
			Application.LoadLevel(0);
		
	}
}
