using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotaterRandomKey : MonoBehaviour {

    public int XRotMax = 10;
    public int XRotMin = 0;
    public int YRotMax = 10;
    public int YRotMin = 0;
    public int ZRotMax = 10;
    public int ZRotMin = 0;
    public bool Rotating = false;
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
        if (Input.GetKeyDown("e"))
        {
            Rotating = !Rotating;
        }

        if (Rotating == true)
        {
            transform.Rotate(new Vector3(XRot, YRot, ZRot) * Time.deltaTime);
        } 
        }
}
