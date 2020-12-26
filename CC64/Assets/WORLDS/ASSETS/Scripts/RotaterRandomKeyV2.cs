using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotaterRandomKeyV2 : MonoBehaviour {

    public int XRotMax = 360;
    public int XRotMin = 0;
    public int YRotMax = 360;
    public int YRotMin = 0;
    public int ZRotMax = 360;
    public int ZRotMin = 0;
    public bool Rotating = false;


    void Update () 
    {
        if (Input.GetKeyDown("g"))
        {
            Rotating = !Rotating;
        }

        if (Rotating == true)
        {
            transform.Rotate(new Vector3(Random.Range(XRotMin, XRotMax), Random.Range(YRotMin, YRotMax), Random.Range(ZRotMin, ZRotMax)));
        } 
        }
}
