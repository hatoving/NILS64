using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotater : MonoBehaviour {

    public int XRot = 0;
    public int YRot = 0;
    public int ZRot = 0;


	void Update () 
    {
        transform.Rotate(new Vector3(XRot, YRot, ZRot) * Time.deltaTime);	
	}
}
