//Christoph Jansen

using UnityEngine;
using System.Collections;

public class GuiControl : MonoBehaviour {
	
	//GUI data set by different scripts
	public static int laps = 1;
	public static float consumption = 0.0f;
	public static string playerName = "Player";
	public static int gear = 0;
	public static float totalConsumption = 0;
	public static float[] lapConsumption = new float[lapsToDrive];
	public static int currentLapTime = 0;
	public static bool showBlackground = false;
	public static int timePoints = 0;
	public static int consumptionPoints = 0;
	public static bool showCurrentLapTime = true;
	public static int lapsToDrive;
	
	//Style
	private const float optimalConsumption = 5.0f;
	private const string consumptionUnit = "l/100km";
	private const int bgWidth = 9000;
	private const int bgHeight = 9000;
	
	public static Color GUIColor = new Color(189.0f/255.0f, 225.0f/255.0f, 255.0f/255.0f, 1);
	public static Color ProtronBlue = new Color(37.0f/255.0f, 151.0f/255.0f, 248.0f/255.0f, 1);
	public static Color ProtronGreen = new Color(136.0f/255.0f, 188.0f/255.0f, 43.0f/255.0f, 1);
	
	//GUIStyle provides styling features in the editor
	public GUIStyle myStyle;
	public GUIStyle focusedStyle;
	private GUIStyle usedStyle;
	
	/*Text and Textures to display on GUI
	 *public to be assigned in editor*/
	public GUIText lapsText;
	public GUIText consumptionText;
	public GUIText playerNameText;
	public GUIText gearText;
	public GUIText totalConsumptionText;
	public GUIText lapConsumptionText;
	public GUIText currentLapTimeText;
	public GUIText pointsText;
	public GUIText countDownText;
	
	public GUITexture fullscreenBlackground;
	public GUITexture logo;
	public GUITexture whiteBackground;
	
	public GUITexture blackgroundTopLeft;
	public GUITexture blackgroundTopCenter;
	public GUITexture blackgroundBottomCenter;
	public GUITexture blackgroundBottomRight;
	
	public RotatableGuiItem rpmBackground;
	public RotatableGuiItem rpmNeedle;
	public RotatableGuiItem veloBackground;
	public RotatableGuiItem veloNeedle;
	
	private string[] buttonNames = {"Resume", "Main Menu"};
	private bool[] buttons;
	private int currentSelection = 0;
	
	private bool buttonPressed = false;
	private bool buttonXPressed = false;
	
	void Start ()
	{
		buttons = new bool[buttonNames.Length];
		
		//Set Style
		lapsText.material.color = GUIColor;
		consumptionText.material.color = GUIColor;
		playerNameText.material.color = GUIColor;
		gearText.material.color = GUIColor;
		totalConsumptionText.material.color = GUIColor;
		lapConsumptionText.material.color = GUIColor;
		currentLapTimeText.material.color = GUIColor;
		
		Rect bgInset = new Rect();
		
		bgInset.width = bgWidth;
		bgInset.height = bgHeight;
		bgInset.x = bgWidth / -2;
		bgInset.y = bgHeight / -2;
		
		fullscreenBlackground.pixelInset = bgInset;
		showBlackground = false;
		pointsText.text = "";
		
		UpdateSettings();
	}
	
	void Update () 
	{
		UpdateSettings();
	}
	
	//Update GUI with new data
	void UpdateSettings(){
		if(consumption > optimalConsumption)
		{
			consumptionText.material.color = Color.red;
		}
		else
		{
			consumptionText.material.color = GUIColor;
		}
				
		lapsText.text = LapsToString();
		consumptionText.text = ConsumptionToString();
		playerNameText.text = PlayerNameToString();
		gearText.text = GearToString();
		totalConsumptionText.text = TotalConsumptionToString();
		lapConsumptionText.text = LapTimeToString();
		currentLapTimeText.text = currentLapTimeToString();
		fullscreenBlackground.enabled = showBlackground;
		if(showBlackground)
			pointsText.text = TotalPointsToString();
		
		
	}
	
	//Display pause menu if timescale is set to zero in Pause script
	void OnGUI(){
		if(Time.timeScale == 0.0f)
		{
			//Hide all GUI items
			lapsText.enabled = false;
			consumptionText.enabled = false;
			playerNameText.enabled = false;
			gearText.enabled = false;
			totalConsumptionText.enabled = false;
			lapConsumptionText.enabled = false;
			currentLapTimeText.enabled = false;
			pointsText.enabled = false;
			countDownText.enabled = false;
			blackgroundTopLeft.enabled = false;
			blackgroundTopCenter.enabled = false;
			blackgroundBottomCenter.enabled = false;
			blackgroundBottomRight.enabled = false;
			rpmBackground.enabled = false;
			rpmNeedle.enabled = false;
			veloBackground.enabled = false;
			veloNeedle.enabled = false;
			
			//Show pause menu
			logo.enabled = true;	
			whiteBackground.enabled = true;
			
			for(int i = 0; i < buttonNames.Length; ++i)
			{
				if(i == currentSelection)
					usedStyle = focusedStyle;
				else
					usedStyle = myStyle;
				GUI.SetNextControlName(buttonNames[i]);
				buttons[i] = false;
				GUI.Button(new Rect(55,150+i*75,300,75), buttonNames[i], usedStyle);
			}
			
			if(!buttonPressed)
			{
			    if (Input.GetKeyDown(KeyCode.DownArrow))
				{
		        	currentSelection = (currentSelection+1)%buttonNames.Length;
					buttonPressed = true;
				}
				if (Input.GetKeyDown(KeyCode.UpArrow))
				{
					currentSelection = (currentSelection-1+buttonNames.Length)%buttonNames.Length;
					buttonPressed = true;
		    	}
			}
			else if ((Input.GetKeyUp(KeyCode.UpArrow)) || (Input.GetKeyUp(KeyCode.DownArrow)))
					buttonPressed = false;
			
			if (!buttonXPressed)
			{
				if (Input.GetAxisRaw("MenuX") < -0.1f)
				{
				 	currentSelection = (currentSelection+1)%buttonNames.Length;
					buttonXPressed = true;
		    	}
				if (Input.GetAxisRaw("MenuX") > 0.1f) 
				{
					currentSelection = (currentSelection-1+buttonNames.Length)%buttonNames.Length;
					buttonXPressed = true;
				}
			}
			else if (Mathf.Abs(Input.GetAxisRaw("MenuX")) < 0.01)
				buttonXPressed = false;
			
			if ((Input.GetKeyDown(KeyCode.Return)) ||
			(Input.GetAxisRaw("MenuSelectX") > 0.1f)) {
				buttons[currentSelection] = true;
			}
			
			if(buttons[0])
				Time.timeScale = 1.0f;
			if(buttons[1]){
				Time.timeScale = 1.0f;
				Application.LoadLevel(0);
			}
		}
		else
		{
			//Show all GUI Items
			lapsText.enabled = true;
			consumptionText.enabled = true;
			playerNameText.enabled = true;
			gearText.enabled = true;
			totalConsumptionText.enabled = true;
			lapConsumptionText.enabled = true;
			currentLapTimeText.enabled = showCurrentLapTime;
			countDownText.enabled = true;
			blackgroundTopLeft.enabled = true;
			blackgroundTopCenter.enabled = true;
			blackgroundBottomCenter.enabled = true;
			blackgroundBottomRight.enabled = true;
			rpmNeedle.enabled = true;
			rpmBackground.enabled = true;
			veloNeedle.enabled = true;
			veloBackground.enabled = true;
			pointsText.enabled = true;
			
			//Hide pause menu
			logo.enabled = false;
			whiteBackground.enabled = false;
		}
		
		
	}
	
	//The following functions convert data to strings
	string LapsToString()
	{
		return "Lap " + laps + "/" + lapConsumption.Length;
	}
	
	string TotalPointsToString()
	{
		int totalPoints = timePoints + consumptionPoints;
		
		if (totalPoints > 0)
		{
			string finalScoreText = "  " + totalConsumption.ToString("0.00") + " " + consumptionUnit + " = "+consumptionPoints+" Points\n"+
									"+ " + (currentLapTime / 10.0f).ToString("0.0") + " s left = "+timePoints+" Points\n"+
									"--------------------------------------------\n"+
									"Final Score: " + totalPoints;
			
			return 	finalScoreText;
		}
		else
			return "";
	}
	
	string ConsumptionToString()
	{
		consumption = ((float)Mathf.Round(consumption * 10))/10;		
		return consumption + " "+consumptionUnit;
	}
	
	string PlayerNameToString()
	{
		return playerName;
	}
	
	string GearToString()
	{
		if(gear == 0)
			return "R";
		return "" + gear;
	}
	
	string TotalConsumptionToString()
	{
		if(totalConsumption == 0)
		{
			return "Total: -.- "+consumptionUnit+"\n";
		}
		else
		{
			totalConsumption = ((float)Mathf.Round(totalConsumption * 10))/10;		
			return "Total: " + totalConsumption + " "+consumptionUnit;
		}		
	}
	
	string LapTimeToString()
	{
		string lapString = "";
		float help = 0;
		
		for(int i = 1; i <= lapConsumption.Length; ++i)
		{
			lapString = lapString + "Lap " + i + ": ";
			
			if(lapConsumption[i-1] == 0)
			{
				lapString += "-.-  "+consumptionUnit+"\n";
			}
			else
			{
				help = ((float)Mathf.Round(lapConsumption[i-1] * 10))/10;
				lapString = lapString + help + " "+consumptionUnit+"\n";
			}
		}
		
		return lapString;
	}
	
	string currentLapTimeToString()
	{
		return CentTimeFormat(currentLapTime, false);
	}
	
	string CentTimeFormat(int time, bool cent)
	{
		int x, help;
		string xString;
		
		if(cent)
		{
			x = time%100;
			help = time/100;
			xString = x.ToString();
			if(x < 10)
				xString = "0" + xString;
		}
		else
		{
			x = time%10;
			help = time/10;
			xString = x.ToString();
		}
		
		int seconds = help%60;
		string secString = seconds.ToString();
		
		if(seconds < 10)
			secString = "0" + secString;	
		
		int minutes = help/60;
		string minString = minutes.ToString();
		
		if(minutes < 10)
			minString = "0" + minString;
		
		return minString + ":" + secString + "." + x;
	}
}
