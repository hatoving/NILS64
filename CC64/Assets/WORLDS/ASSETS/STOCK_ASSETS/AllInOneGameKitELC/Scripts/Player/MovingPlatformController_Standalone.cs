/// All in One Game Kit - Easy Ledge Climb Character System
/// MovingPlatformController_Standalone.cs
///
/// As long as the player has a CharacterController or Rigidbody component, this script allows the player to:
/// 1. Ride moving and rotating platforms (Moving Platforms).
///
/// (C) 2015-2018 Grant Marrs

using UnityEngine;
using System.Collections;

public class MovingPlatformController_Standalone : MonoBehaviour {
	
	//Moving platform variables
	public bool allowMovingPlatformSupport = true; //determines whether or not the player can move with moving platforms
	public string movingPlatformTag = "Platform"; //the tag of the moving platform objects
	
	//private moving platform variables
	private float noCollisionWithPlatformTimer; //time since last collision with platform tag
	[HideInInspector]
	public GameObject oldParent; //the player's parent before coming in contact with a platform
	[HideInInspector]
	public GameObject emptyObject; //empty object that undoes the platform's properties that affect the player's scale
	private GameObject emptyObjectParent; //parent of the empty object (the platform itself)
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
		//Parenting and unparenting to moving platforms
		//if the player is not colliding with a platform (and he is not on the ledge of the platform), set the player's parent to what it was before
		if (allowMovingPlatformSupport && noCollisionWithPlatformTimer >= 5 && emptyObject != null){
			
			//unparenting player from platform
			if (transform.parent == emptyObject.transform){
				if (oldParent != null){
					transform.parent = oldParent.transform;
				}
				else {
					transform.parent = null;
				}
			}
			
			//deleting empty object once the player is no longer a child of it
			if (transform.parent != emptyObject.transform && emptyObject.transform.childCount == 0 && (transform.parent == oldParent || transform.parent == null)){
				//making sure we are no longer attached to the empty object (so that we don't delete ourself)
				transform.parent = null;
				//deleting the emptyObject
				if (transform.parent != emptyObject.transform){
					transform.parent = null;
					Destroy(emptyObject);
				}
				//setting parent back to normal
				if (oldParent != null){
					transform.parent = oldParent.transform;
				}
				else {
					transform.parent = null;
				}
			}
			
		}
		
	}
	
	void FixedUpdate () {
		
		//Moving on platforms
		MovingPlatformParenting();
		
	}
	
	void MovingPlatformParenting () {
		
		//moving with moving platforms
		//increase the noCollisionWithPlatformTimer (if there is a collision, the noCollisionWithPlatformTimer is later set to 0)
		noCollisionWithPlatformTimer++;
		
		//undoing parent's properties that affect the player's scale 
		if (emptyObject != null){
			emptyObjectParent = emptyObject.transform.parent.gameObject;
			emptyObject.transform.localScale = new Vector3(1/emptyObjectParent.transform.localScale.x, 1/emptyObjectParent.transform.localScale.y, 1/emptyObjectParent.transform.localScale.z);
			emptyObject.transform.localRotation = Quaternion.Euler(-emptyObjectParent.transform.localRotation.x, -emptyObjectParent.transform.localRotation.y, -emptyObjectParent.transform.localRotation.z);
		}
		else {
			emptyObjectParent = null;
		}
		
		//getting what the player's parent was before coming in contact with a platform
		if (transform.parent == null){
			oldParent = null;
		}
		else if (emptyObject == null || transform.parent != emptyObject.transform){
			oldParent = transform.parent.gameObject;
		}
		else {
			oldParent = null;
		}
		
	}
	
	void OnControllerColliderHit (ControllerColliderHit hit) {
		
		//moving with moving platforms
		if (hit.gameObject.tag == movingPlatformTag && allowMovingPlatformSupport){
			//since we are colliding with the platform, set the no collision timer to 0
			noCollisionWithPlatformTimer = 0;
			
			//create and parent empty object (so that we can undo the parent's properties that affect the player's scale)
			if (emptyObject == null){
				emptyObject = new GameObject();
				emptyObject.transform.position = hit.transform.position;
			}
			emptyObject.name = "PlatformPlayerConnector";
			emptyObject.transform.parent = hit.transform;
			
			//undoing parent's properties that affect the player's scale
			emptyObject.transform.localScale = new Vector3(1/hit.transform.localScale.x, 1/hit.transform.localScale.y, 1/hit.transform.localScale.z);
			emptyObject.transform.localRotation = Quaternion.Euler(-hit.transform.localRotation.x, -hit.transform.localRotation.y, -hit.transform.localRotation.z);
			
			//setting player's parent to the empty object
			transform.parent = emptyObject.transform;
		}
		
	}
	
	void OnCollisionStay (Collision hit) {
		
		//moving with moving platforms
		if (hit.gameObject.tag == movingPlatformTag && allowMovingPlatformSupport){
			//since we are colliding with the platform, set the no collision timer to 0
			noCollisionWithPlatformTimer = 0;
			
			//create and parent empty object (so that we can undo the parent's properties that affect the player's scale)
			if (emptyObject == null){
				emptyObject = new GameObject();
				emptyObject.transform.position = hit.transform.position;
			}
			emptyObject.name = "PlatformPlayerConnector";
			emptyObject.transform.parent = hit.transform;
			
			//undoing parent's properties that affect the player's scale
			emptyObject.transform.localScale = new Vector3(1/hit.transform.localScale.x, 1/hit.transform.localScale.y, 1/hit.transform.localScale.z);
			emptyObject.transform.localRotation = Quaternion.Euler(-hit.transform.localRotation.x, -hit.transform.localRotation.y, -hit.transform.localRotation.z);
			
			//setting player's parent to the empty object
			transform.parent = emptyObject.transform;
		}
		
	}
	
	void OnTriggerStay (Collider hit) {
		
		//moving with moving platforms
		if (hit.gameObject.tag == movingPlatformTag && allowMovingPlatformSupport){
			//since we are colliding with the platform, set the no collision timer to 0
			noCollisionWithPlatformTimer = 0;
			
			//create and parent empty object (so that we can undo the parent's properties that affect the player's scale)
			if (emptyObject == null){
				emptyObject = new GameObject();
				emptyObject.transform.position = hit.transform.position;
			}
			emptyObject.name = "PlatformPlayerConnector";
			emptyObject.transform.parent = hit.transform;
			
			//undoing parent's properties that affect the player's scale
			emptyObject.transform.localScale = new Vector3(1/hit.transform.localScale.x, 1/hit.transform.localScale.y, 1/hit.transform.localScale.z);
			emptyObject.transform.localRotation = Quaternion.Euler(-hit.transform.localRotation.x, -hit.transform.localRotation.y, -hit.transform.localRotation.z);
			
			//setting player's parent to the empty object
			transform.parent = emptyObject.transform;
		}
		
	}
	
}