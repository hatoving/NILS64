using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotaterSetRandom : MonoBehaviour {

    public float XRot;
    public float YRot;
    public float ZRot;

    void Start()
    {

    }


    void Update () 
    {
        XRot = (Random.Range(0, 360));
        YRot = (Random.Range(0, 360));
        ZRot = (Random.Range(0, 360));
        transform.Rotate(new Vector3(XRot, YRot, ZRot));	
	}
}
