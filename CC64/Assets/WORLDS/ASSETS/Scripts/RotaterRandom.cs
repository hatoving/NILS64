using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotaterRandom : MonoBehaviour {

    public int XRotMax = 0;
    public int XRotMin = 0;
    public int YRotMax = 0;
    public int YRotMin = 0;
    public int ZRotMax = 0;
    public int ZRotMin = 0;
    private float XRot = 0;
    private float YRot = 0;
    private float ZRot = 0;


    void Start()
    {
    XRot = (Random.Range(XRotMin, XRotMax));
YRot = (Random.Range(YRotMin, YRotMax));
ZRot = (Random.Range(ZRotMin, ZRotMax));
    }


    void Update () 
    {
        transform.Rotate(new Vector3(XRot, YRot, ZRot) * Time.deltaTime);	
	}
}
