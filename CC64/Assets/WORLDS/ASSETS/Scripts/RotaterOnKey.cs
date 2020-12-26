using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotaterOnKey : MonoBehaviour {

    public int XRot = 0;
    public int YRot = 0;
    public int ZRot = 0;
    public float XTarget = 0;
    public float YTarget = 0;
    public float ZTarget = 0;
    public bool Switched = false;

    void Update () 
    {
        if (Switched == true) 
        { 
        transform.Rotate(new Vector3(XRot, YRot, ZRot) * Time.deltaTime);
        XTarget -= XRot;
        YTarget -= YRot;
        ZTarget -= ZRot;
        }

        if (Input.GetKeyDown("e"))
        {
            Switched = !Switched;
        }

        if (XTarget <= 0)
        {
            if (YTarget <= 0)
            {
                if (ZTarget <= 0)
                {
                    Switched = false;
                }
            }
        }
    }


}
