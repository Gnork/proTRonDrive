//Christoph Jansen

using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour 
{
	//GUIStyle provides styling features in the editor
	public GUIStyle e_myStyle;
	public GUIStyle e_focusedStyle;
	
	private GUIStyle _myStyle;	
	private GUIStyle _focusedStyle;
	private GUIStyle _usedStyle;
	
	
	private string[] buttonNames = {"Start", "Highscore", "Options", "Credits", "Exit"};
	private bool[] buttons;
	private int currentSelection = 0;
	
	private float buttonDelay = 0.0f;
	private float buttonDelayTime = 0.25f;
	
	void Start()
	{
		Screen.showCursor = false;
		//Assigning public members to private members
		if(e_myStyle != null)
			_myStyle = e_myStyle;
		else
			throw new UnassignedReferenceException("The editor member 'e_myStyle' of Menu must be assigned");
		if(e_focusedStyle != null)
			_focusedStyle = e_focusedStyle;
		else
			throw new UnassignedReferenceException("The editor member 'e_focusedStyle' of Menu must be assigned");
		
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
			GUI.SetNextControlName(buttonNames[i]);
			buttons[i] = false;
			GUI.Button(new Rect(55,150+i*75,300,75), buttonNames[i], _usedStyle);
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
		}
		else
		{
			buttonDelay += Time.deltaTime;
		}
		
		if ((Input.GetKeyDown(KeyCode.Return)) ||
			(Input.GetAxis("MenuSelectX") > 0.1f)) {
			buttons[currentSelection] = true;
		}
		
		if(buttons[0])
			Application.LoadLevel(1);
		if(buttons[1])
			Application.LoadLevel(5);
		if(buttons[2])
			Application.LoadLevel(6);
		if(buttons[3])
			Application.LoadLevel(7);
		if(buttons[4])
			Application.Quit();
	}
}
