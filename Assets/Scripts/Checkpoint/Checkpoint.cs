using UnityEngine;
using System;
using AssemblyCSharp;

public class Checkpoint : MonoBehaviour
{
	/**
	 * Editor variables (only used for initialization)
	 */
	public MonoBehaviour e_CheckpointReceiver = null; // must implement ICheckReceiver
	public bool e_SendToMasterCheckpointReceiver = true;
	
	/**
	 * Member variables
	 */
	private bool _sendToMaster;
	private static IMasterCheckReceiver _masterCheckReceiver = null; // must implement IAnyCheckReceiver and will receive all checkpoint passes in the game
	
	private ICheckReceiver _receiver = null; // If not assigned, only the anyCheckReceiver will be notified
	private TriggerTypes _lastTrigger; // The last trigger passed by the car
	
	/**
	 * Properties
	 */
	public static IMasterCheckReceiver MasterCheckReceiver
	{
		set { _masterCheckReceiver = value; }
	}
	
	
	void Start()
	{		
		_sendToMaster = e_SendToMasterCheckpointReceiver;
		
		if (e_CheckpointReceiver is ICheckReceiver)
			_receiver = (ICheckReceiver) e_CheckpointReceiver;
		else
			throw new InvalidCastException("The receiver must implement the interface ICheckReceiver.");
	}
	
	public TriggerTypes LastTrigger // The trigger will assign this propertie to its position
	{
		set
		{
			if (value == TriggerTypes.ActualCheckpoint)
			{
				// If last checkpoint was the front one and now the car passed the back one,
				// that means the car is passing the checkpoint, driving forward
				if (_lastTrigger == TriggerTypes.SetNextPassToForward)
				{
					_receiver.CheckpointPassed(true);
					
					if ((_masterCheckReceiver != null) && (_sendToMaster))
						_masterCheckReceiver.AnyCheckpointPassed(true);
				}
				
				// If the last checkpoint was not the front one, that means the car is not driving forward
				else if (_lastTrigger == TriggerTypes.SetNextPassToReverse)
				{
					_receiver.CheckpointPassed(false);
					
					if ((_masterCheckReceiver != null) && (_sendToMaster))
						_masterCheckReceiver.AnyCheckpointPassed(false);
				}
			}
		
			this._lastTrigger = value;
		}
	}
}

