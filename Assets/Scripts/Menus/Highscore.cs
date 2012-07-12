//Christoph Jansen

using UnityEngine;
using System.Collections;
using System.Text;
using System;

public class Highscore : MonoBehaviour {
	
	//GUIStyle provides styling features in the editor	
	public GUIStyle e_myStyle;
	public GUIStyle e_labelStyle;
	public GUIStyle e_positionStyle;
	
	private GUIStyle _myStyle;
	private GUIStyle _labelStyle;
	private GUIStyle _positionStyle;
	
	private GUIStyle usedStyle;
	
	private int _position;
	private string _isAutoString;
	
	private string _keysAndValues;
	
	//Data stored in Highscore is set by different scripts after a race
	public static string player = "Player";
	public static int score = 0;
	public static float consumption = 0.0f;
	public static bool isAfterRace = false;
	public static bool isAuto = false;
	
	void Start()
	{
		_isAutoString = IsAutoToString();
		_position = 10;
		//Assigning public members to private members
		if(e_myStyle != null)
			_myStyle = e_myStyle;
		else
			throw new UnassignedReferenceException("The editor member 'e_myStyle' of Highscore must be assigned");
		
		if(e_labelStyle != null)
			_labelStyle = e_labelStyle;
		else
			throw new UnassignedReferenceException("The editor member 'e_labelStyle' of Highscore must be assigned");
		
		if(e_positionStyle != null)
			_positionStyle = e_positionStyle;
		else
			throw new UnassignedReferenceException("The editor member 'e_positionStyle' of Highscore must be assigned");
		
		//Initialize Highscore if no Highscore exists
		if(!PlayerPrefs.HasKey("0name"+_isAutoString))
			InitializeHighscore();
		
		//Add player to Highscore if this script is called after a race
		if(isAfterRace)
		{
			CheckPlayerName();
			if(GameControl._isOnlineHighscore)
				AddPlayerToOnlineHighscore();
			else
				AddPlayerToOfflineHighscore();
		}			
		isAfterRace = false;
		if(GameControl._isOnlineHighscore)
		{
			LoadOnlineHighscore();
		}
	}
	
	void OnGUI()
	{
		_isAutoString = IsAutoToString();
		//Create labels to display Highscore data
		if(!GameControl._isOnlineHighscore)
		{		
			for(int i = 0; i < 10; ++i)
			{
				if(i == _position)
					usedStyle = _positionStyle;
				else
					usedStyle = _labelStyle;
				GUI.Label (new Rect(55,150+i*30,40,20), (i+1).ToString("00"), usedStyle);
				GUI.Label (new Rect(100,150+i*30,120,20), PlayerPrefs.GetString(i+"name"+_isAutoString), usedStyle);
				GUI.Label (new Rect(225,150+i*30,120,20), PlayerPrefs.GetInt(i+"score"+_isAutoString).ToString("000000"), usedStyle);
				GUI.Label (new Rect(350,150+i*30,120,20), PlayerPrefs.GetFloat(i+"consumption"+_isAutoString).ToString("00.00")+" l/100km", usedStyle);
			}
		}else
		{
			int j = 0;
			if(!isAuto)
				j = 10;
			for(int i = 0; i < 10; ++i)
			{
				GUI.Label (new Rect(55,150+i*30,40,20), (i+1).ToString("00"), _labelStyle);
				GUI.Label (new Rect(100,150+i*30,120,20), PlayerPrefs.GetString("name"+j), _labelStyle);
				GUI.Label (new Rect(225,150+i*30,120,20), PlayerPrefs.GetInt("score"+j).ToString("000000"), _labelStyle);
				GUI.Label (new Rect(350,150+i*30,120,20), PlayerPrefs.GetFloat("consumption"+j).ToString("00.00")+" l/100km", _labelStyle);
				++j;
			}	
		}
		//Create menu button
		GUI.Button(new Rect(55,450,300,75), "Main Menu", _myStyle);
		
		if ((Input.GetKeyDown(KeyCode.Return)) ||
			(Input.GetAxis("MenuSelectX") > 0.1f)) {
			Application.LoadLevel(0);
		}
	}
	
	private void LoadOnlineHighscore()
	{
		string data = PhpConnect.Read();
		char[] dataArray = data.ToCharArray();
		StringBuilder helper = new StringBuilder();
		string helperString;
		int counter = 0;
		for(int i = 0; i < dataArray.Length; ++i)
		{
			if(!dataArray[i].Equals(' '))
			{
				helper.Append(dataArray[i]);
			}else
			{
				helperString = helper.ToString();
				if(counter%3 == 0)
					PlayerPrefs.SetString("name"+(counter/3), helperString);
				else if(counter%3 == 1)
					PlayerPrefs.SetInt("score"+(counter/3), int.Parse(helperString));
				else
					PlayerPrefs.SetFloat("consumption"+(counter/3), (float)int.Parse(helperString)/100);
				++counter;
				helper = new StringBuilder();
			}
		}
	}
	
	//Add new player to Highscore
	private void AddPlayerToOfflineHighscore()
	{
		string newName = player, oldName = player;
		int newScore = score, oldScore = score;
		float newConsumption = score, oldConsumption = consumption;
		_position = 0;
		
		for(int i = 0; i < 10; ++i)
		{
			if(PlayerPrefs.GetInt(i+"score"+_isAutoString) < newScore){
				oldScore = PlayerPrefs.GetInt(i+"score"+_isAutoString);
				oldName = PlayerPrefs.GetString(i+"name"+_isAutoString);
				oldConsumption = PlayerPrefs.GetFloat(i+"consumption"+_isAutoString);
				PlayerPrefs.SetInt(i+"score"+_isAutoString, newScore);
				PlayerPrefs.SetString(i+"name"+_isAutoString, newName);
				PlayerPrefs.SetFloat(i+"consumption"+_isAutoString, newConsumption);
				newScore = oldScore;
				newName = oldName;
				newConsumption = oldConsumption;						
			}else
				++_position;
		}
	}
	
	private void AddPlayerToOnlineHighscore()
	{
		_keysAndValues = "name="+player+"&points="+score+"&consumption="+Math.Round(consumption*100, 0);
		PhpConnect.Send(_keysAndValues, isAuto);
	}
	
	private string IsAutoToString()
	{
		if(isAuto)
			return "auto";
		else
			return "manu";
		
	}
	
	/*Initialize Highscore with PlayerPrefs for the first time
	 *In MS Windows Player Prefs are stored in registry*/
	private void InitializeHighscore()
	{
		for(int i = 0; i < 10; ++i)
		{
			PlayerPrefs.SetString(i+"name"+_isAutoString, "Player");
			PlayerPrefs.SetFloat(i+"consumption"+_isAutoString, 0.0f);
			PlayerPrefs.SetInt(i+"score"+_isAutoString, 0);
		}
	}
	
	private void CheckPlayerName(){
		char[] charArray = player.ToCharArray();
		StringBuilder nameBuilder = new StringBuilder();
		int counter = 0;
		for(int i = 0; i < charArray.Length; ++ i)
		{
			if((charArray[i] >= '0' && charArray[i] <= '9')||(charArray[i] >= 'a' && charArray[i] <= 'z')||(charArray[i] >= 'A' && charArray[i] <= 'Z')){
				nameBuilder.Append(charArray[i]);
				++counter;
			}
			if(counter == 10)
				break;
		}
		if(nameBuilder.Length == 0)
			player = "Player";
		else
			player = nameBuilder.ToString();
	}
}
