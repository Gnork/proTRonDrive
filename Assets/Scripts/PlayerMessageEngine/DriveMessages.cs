using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using AssemblyCSharp;

public class DriveMessages : MonoBehaviour
{
	/**
	 * Consts
	 */
	private const float signFlashDelay = 0.3f; // seconds between the sign flashes
	private const float signShowTime = 1.5f; // seconds until the signs disappear
	
	/**
	 * Editor variables (for initialization only)
	 */
	public GUITexture e_GUITexture;
	
	/**
	 * Member variables
	 */
	private GUITexture _guiTexture;	
	
	// Store the texture of the sign and the information, if it should flash
	private List<Texture2D> _signs;
	private List<bool> _signShallFlash;
	
	private Clock _signTimer;
	private Clock _flashTimer;
	
	private bool _initialized = false;
	
	/**
	 * Properties
	 */
	public bool IsInitialized
	{
		get { return _initialized; }
	}
	
	/**
	 * Methods
	 */
	void Start()
	{
		_guiTexture = e_GUITexture;
		_signs = new List<Texture2D>();
		_signShallFlash = new List<bool>();
		
		_signTimer = new Clock(signShowTime);
		_flashTimer = new Clock(signFlashDelay);
		
		_guiTexture.enabled = false;
		_initialized = true;
	}
	
	void Update()
	{
		if (Time.timeScale < 0.0001)
			_guiTexture.enabled = false;
			
		_signTimer.Update(Time.deltaTime);
		_flashTimer.Update(Time.deltaTime);
		
		UpdateDisplay();
	}
	
	public void AddSign(string name, bool signShallFlash)
	{			
		Texture2D tex = (Texture2D) Resources.Load(name, typeof(Texture2D));
		
		if (tex == null)
			throw new FileNotFoundException("The file under the path " + name + " could not be loaded.");

		tex.name = name;		
		_signs.Add(tex);
		_signShallFlash.Add(signShallFlash);
	}
	
	public void ShowSign(string name)
	{
		int indexToShow = GetSignIndexByName(name);
		if (indexToShow < 0)
			return;
		
		_guiTexture.texture = _signs[indexToShow];
		_guiTexture.enabled = true;
		
		_signTimer.Stop();
		_signTimer.Start();
		
		if (_signShallFlash[indexToShow])
		{
			_flashTimer.Stop();
			_flashTimer.Start();
		}
	}
	
	private int GetSignIndexByName(string name)
	{
		for (int i=0; i < _signs.Count; i++)
		{
			if (_signs[i].name == name)
				return i;
		}
		
		return -1;
	}
	
	private void UpdateDisplay()
	{
		if (_signTimer.State == TimerStates.Started)
		{
			// Let the sign flash
			if (_flashTimer.State == TimerStates.Expired)
			{
				_guiTexture.enabled = !_guiTexture.enabled;
				
				_flashTimer.Stop();
				_flashTimer.Start();
			}
		}
		
		// Stop displaying the sign and stop the timers when the time is over
		if (_signTimer.State == TimerStates.Expired)
		{
			_guiTexture.enabled = false;
			
			_signTimer.Stop ();
			_flashTimer.Stop ();
		}
	}
}

