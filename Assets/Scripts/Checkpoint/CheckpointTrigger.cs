using UnityEngine;
using System.Collections;
using AssemblyCSharp;

public class CheckpointTrigger : MonoBehaviour 
{
	/**
	 * Editor variables (only for initialization)
	 */
	public Checkpoint e_Checkpoint;
	public TriggerTypes e_Position;
	
	/**
	 * Member variables
	 */
	private Checkpoint _checkpoint;
	private TriggerTypes _position;
	
	void Start()
	{
		if (e_Checkpoint != null)
			_checkpoint = e_Checkpoint;
		else
			throw new UnassignedReferenceException("The editor member 'e_Checkpoint' of CheckpointTrigger must be assigned.");
			
		_position = e_Position;
	}
	
	void OnTriggerEnter(Collider other)
	{
		_checkpoint.LastTrigger = _position;
	}
}
