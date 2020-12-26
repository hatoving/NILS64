/// All in One Game Kit - Easy Ledge Climb Character System
/// MovingPlatform.cs
///
/// This script allows the platform to:
/// 1. Move to and from a set position.
/// 2. Move at a set speed.
/// 3. Rotate towards a set direction.
/// 4. Wait before moving.
///
/// (C) 2015-2018 Grant Marrs

using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour {
	
	[System.Serializable]
	public class Movement {
		public Vector3 startPosition; //the Vector3 that the platform starts at (before moving to the end position)
		public Vector3 endPosition; //the Vector3 that the platform ends at (before moving back to the start position)
		public float movementSpeed = 3; //the speed of the moving platform
		public float waitingTimeBeforeMoving = 1; //the amount of time the platform waits (once it has reached the start or end position) before moving again
		public bool startMovingImmediately = true; //for the first time that the platform starts moving, do not wait
	}
	public Movement movement = new Movement(); //variables that determine the movement of the platform
	
	[System.Serializable]
	public class Rotation {
		public Vector3 rotationDirection = new Vector3(0, 0, 0); //the direction to rotate towards
		public bool onlyRotateWhenMoving = false; //only allows the platform to rotate if the platform is already moving/not waiting to move
	}
	public Rotation rotation = new Rotation(); //variables that determine the rotation of the platform
	
	//private variables
	private bool atStart; //determines if we are at the start position
	private float waitingTimer; //a timer that measures how long we have been at the start or end position
	
	// Use this for initialization
	void Start () {
		
		//setting the moving platform's position to the start position
		transform.localPosition = movement.startPosition;
		atStart = true;
		
		//if the platform is allowed to start moving immediately
		if (movement.startMovingImmediately){
			waitingTimer = movement.waitingTimeBeforeMoving;
		}
		
	}
	
	void FixedUpdate () {
		
		//if we are at the start position, set atStart to true
		if (Vector3.Distance(transform.localPosition, movement.startPosition) <= 0.01f){
			if (!atStart){
				waitingTimer = 0;
				atStart = true;
			}
		}
		//but if we are at the end position, set atStart to false
		else if (Vector3.Distance(transform.localPosition, movement.endPosition) <= 0.01f){
			if (atStart){
				waitingTimer = 0;
				atStart = false;
			}
		}
		
		//waiting before moving
		waitingTimer += 0.02f;
		
		//if the waiting timer is greater than or equal to the movement.waitingTimeBeforeMoving, start moving
		if (waitingTimer >= movement.waitingTimeBeforeMoving){
			
			//moving to end position (if we are at the start)
			if (atStart){
				transform.localPosition = Vector3.MoveTowards(transform.localPosition, movement.endPosition, movement.movementSpeed * Time.deltaTime);
			}
			//moving to start position (if we are at the end)
			else {
				transform.localPosition = Vector3.MoveTowards(transform.localPosition, movement.startPosition, movement.movementSpeed * Time.deltaTime);
			}
			
			//rotating item
			if (rotation.onlyRotateWhenMoving){
				transform.Rotate(rotation.rotationDirection.x, rotation.rotationDirection.y, rotation.rotationDirection.z);
			}
			
		}
		
		//rotating item
		if (!rotation.onlyRotateWhenMoving){
			transform.Rotate(rotation.rotationDirection.x, rotation.rotationDirection.y, rotation.rotationDirection.z);
		}
		
	}
}
