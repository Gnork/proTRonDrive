//Christoph Jansen
using UnityEngine;
using System.Collections;

public class ModifiedCamera : MonoBehaviour {
	
	public static float velocity = 0;
	public Transform target;
	public float height = 5.0f;
	public float heightDamping = 2.0f;
	public float rotationDamping = 3.0f;
	public float velocityInfluence = 0.02f;	
	public float distance = 10.0f;
	
	private float distanceChangePerFrame = 0.1f;
	private float oldDistance;
	
	void Start(){
		oldDistance = distance;
	}

	void LateUpdate () {
		if (!target)
			return;
		
		float wantedRotationAngle = target.eulerAngles.y;
		float wantedHeight = target.position.y + height;
			
		float currentRotationAngle = transform.eulerAngles.y;
		float currentHeight = transform.position.y;
		
		float currentDistance = distance + velocity * velocityInfluence;
		if(currentDistance > oldDistance + distanceChangePerFrame){
			currentDistance = oldDistance + distanceChangePerFrame;
		}else if(currentDistance < oldDistance - distanceChangePerFrame){
			currentDistance = oldDistance - distanceChangePerFrame;
		}
		oldDistance = currentDistance;
		
		currentRotationAngle = Mathf.LerpAngle (currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
	
		currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);
	
		var currentRotation = Quaternion.Euler (0, currentRotationAngle, 0);
		
		transform.position = target.position;
		transform.position -= currentRotation * Vector3.forward * currentDistance;
	
		transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
		
		transform.LookAt(target);
	}
}
