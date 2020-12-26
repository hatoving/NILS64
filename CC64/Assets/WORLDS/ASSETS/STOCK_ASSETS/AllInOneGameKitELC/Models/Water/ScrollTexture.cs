/// All in One Game Kit - Easy Ledge Climb Character System
/// ScrollTexture.cs
///
/// Automatically rolls the texture of the object this script is attached to.
///
/// (C) 2015-2018 Grant Marrs

using UnityEngine;
using System.Collections;

public class ScrollTexture : MonoBehaviour {
	
	//scrolling variables
	public float scrollSpeed = 0.05f; 
	public bool scrollHorizontally = true; 
	public bool scrollVertically = true;
	private Vector2 value;
	
	// Use this for initialization
	void Start () {
		
		//getting the original texture offset
		value = GetComponent<Renderer>().material.mainTextureOffset;
		
	}
	
	// Update is called once per frame
	void Update () {
		
		//creating a new texture offset
		float offset = Time.deltaTime * scrollSpeed;
		value = GetComponent<Renderer>().material.mainTextureOffset;	
		
		//scrolling the texture
		if (scrollHorizontally && scrollVertically){ 
			GetComponent<Renderer>().material.SetTextureOffset ("_MainTex", new Vector2(value.x + offset, value.y + offset)); 
		} 
		else if (scrollHorizontally){ 
			GetComponent<Renderer>().material.SetTextureOffset ("_MainTex", new Vector2(value.x + offset, GetComponent<Renderer>().material.mainTextureOffset.y)); 
		} 
		else if (scrollVertically){ 
			GetComponent<Renderer>().material.SetTextureOffset ("_MainTex", new Vector2(GetComponent<Renderer>().material.mainTextureOffset.x, value.y + offset)); 
		}
		
	}
	
}