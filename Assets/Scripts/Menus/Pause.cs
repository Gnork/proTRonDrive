//Christoph Jansen

using UnityEngine;
using System.Collections;

public class Pause : MonoBehaviour {

	//Sets timescale to zero to pause or back to one to resume the game
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton6))
		{
			if(Time.timeScale == 0.0f)
				Time.timeScale = 1.0f;
			else
				Time.timeScale = 0.0f;
		}
	}
}
