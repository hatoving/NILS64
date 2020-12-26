using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotaterRandomV2 : MonoBehaviour {

    public int XRotMax = 4;
    public int XRotMin = 0;
    public int YRotMax = 4;
    public int YRotMin = 0;
    public int ZRotMax = 4;
    public int ZRotMin = 0;
    public int Multiplyer = 90;
    public float XRot = 0;
    public float YRot = 0;
    public float ZRot = 0;


    void Start()
    {
    XRot = (Random.Range(XRotMin, XRotMax)) * Multiplyer;
YRot = (Random.Range(YRotMin, YRotMax)) * Multiplyer;
ZRot = (Random.Range(ZRotMin, ZRotMax)) * Multiplyer;
        transform.Rotate(new Vector3(XRot, YRot, ZRot));
    }
}
