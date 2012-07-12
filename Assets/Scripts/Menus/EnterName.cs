//Christoph Jansen

using UnityEngine;
using System.Collections;

public class EnterName : MonoBehaviour {
	
	//GUIStyle provides styling features in the editor	
	public GUIStyle e_myStyle;
	public GUIStyle e_labelStyle;
	
	private GUIStyle _myStyle;
	private GUIStyle _labelStyle;
	
	//String to edit in text field
	private string playerName = "";
	
	void Start()
	{
		//Assigning public members to private members
		if(e_myStyle != null)
			_myStyle = e_myStyle;
		else
			throw new UnassignedReferenceException("The editor member 'e_myStyle' of EnterName must be assigned");
		if(e_labelStyle != null)
			_labelStyle = e_labelStyle;
		else
			throw new UnassignedReferenceException("The editor member 'e_labelStyle' of EnterName must be assigned");
	}
	
	//Creates text field for player name and menu button
	void OnGUI()
	{
		GUI.Label(new Rect(55,150,300,30), "Enter name and press RETURN", _labelStyle);
		if( ((Event.current.type == EventType.KeyDown || Event.current.type == EventType.KeyUp) && Event.current.keyCode == KeyCode.Return ) ||
				(Input.GetAxis("MenuSelectX") > 0.1f) )
    	{
        	Highscore.player = playerName;
			Highscore.isAfterRace = true;
			Highscore.isAuto = GameControl._isAuto;
			Application.LoadLevel(3);
    	}
    	else
    	{
        	GUI.SetNextControlName("nameField");
			playerName = GUI.TextField(new Rect(55,180,370,75), playerName, 10, _myStyle);
			GUI.FocusControl("nameField");
    	}
		/*GUI.SetNextControlName("nameField");
		playerName = GUI.TextField(new Rect(55,150,370,75), playerName, 10, _myStyle);
		GUI.FocusControl("nameField");
		
		if (Input.GetKeyDown(KeyCode.Return))
		{
			Highscore.player = playerName;
			Highscore.isAfterRace = true;
			Highscore.isAuto = GameControl._isAuto;
			Application.LoadLevel(3);
		}*/
	}
}
