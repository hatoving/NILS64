/// All in One Game Kit - Easy Ledge Climb Character System
/// ItemRotation.cs
///
/// This script allows an item to:
/// 1. Rotate towards a set direction (the higher the number, the faster the item will rotate).
///
/// (C) 2015-2018 Grant Marrs

using UnityEngine;
using System.Collections;

public class ItemRotation : MonoBehaviour {
	
	public Vector3 direction; //the direction to rotate towards
	
	// Use this for initialization
	void Start () {
		
	}
	
	void LateUpdate () {
		
		//rotating item
		transform.Rotate(direction.x, direction.y, direction.z);
		
	}
	
}