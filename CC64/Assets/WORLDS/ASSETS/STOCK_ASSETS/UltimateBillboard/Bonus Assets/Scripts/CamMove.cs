using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamMove : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.up * Time.fixedDeltaTime * 60f * Input.GetAxis("Horizontal"));
        transform.Translate(Vector3.forward * Time.fixedDeltaTime * 5f * Input.GetAxis("Vertical"));
	}
}
