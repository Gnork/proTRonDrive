using System;
using UnityEngine;
using AssemblyCSharp;
using System.IO;

public class ArrowShower : MonoBehaviour, ICheckReceiver
{
	/**
	 * Editor variables (for initialization only)
	 */
	public DriveMessages e_DriveMessages;
	public Arrows e_ArrowToShow;
	
	/**
	 * Member variables
	 */
	private DriveMessages _messages;	
	private Arrows _arrow;
	
	private bool _initialized = false;
	
	/**
	 * Methods
	 */
	void Start()
	{
		_messages = e_DriveMessages;
		_arrow = e_ArrowToShow;
	}
	
	void Update()
	{
		// Only do, if not initialized yet
		if (!_initialized)
		{
			// Wait for the DriveMessages object to be initialized
			if (_messages.IsInitialized)
			{
				foreach (string arrowName in Enum.GetNames(typeof(Arrows)))
				{
					try
					{
						_messages.AddSign(arrowName, false);
					}
					
					// Catch the standard exception and throw a new one with more detailed information
					catch (FileNotFoundException)
					{
						throw new FileNotFoundException("Could not find a picture file named '"+arrowName+"'."+
														"All values in the enum Arrows must have an equally "+
														"named picture file in the resources folder.");
					}
				}
				
				_initialized = true;
			}
		}
	}
	
	public void CheckpointPassed(bool isDrivingForward)
	{
		if (isDrivingForward)
		{
			_messages.ShowSign(_arrow.ToString());
		}
	}
}

