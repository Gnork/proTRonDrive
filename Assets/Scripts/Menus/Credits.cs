using UnityEngine;
using System.Collections;

public class Credits : MonoBehaviour {
	
	public GUIStyle e_myStyle;
	public GUIStyle e_labelStyle;
	
	private GUIStyle _myStyle;
	private GUIStyle _labelStyle;

	// Use this for initialization
	void Start () {
		if(e_myStyle != null)
			_myStyle = e_myStyle;
		else
			throw new UnassignedReferenceException("The editor member 'e_myStyle' of Credits must be assigned");
		if(e_labelStyle != null)
			_labelStyle = e_labelStyle;
		else
			throw new UnassignedReferenceException("The editor member 'e_labelStyle' of Credits must be assigned");
	}

	void OnGUI () {
		GUI.Label(new Rect(55,150,700,400),"proTRon Drive created by Christoph Jansen and Milan Ruell\n\nFH Trier 2012, supervised by Prof. Dr. Ing. Christoph Luerig\n\n\nAll sounds from freesound.org and edited by Milan Ruell\n\nIn coorperation with Team proTRon",_labelStyle);
		
		GUI.Button(new Rect(55,480,300,75), "Main Menu", _myStyle);
		
		if ((Input.GetKeyDown(KeyCode.Return)) ||
			(Input.GetAxis("MenuSelectX") > 0.1f)) {
			Application.LoadLevel(0);
		}
	}
}
