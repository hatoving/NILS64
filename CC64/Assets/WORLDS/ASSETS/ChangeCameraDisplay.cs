using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCameraDisplay : MonoBehaviour
{
    public Camera cam1;
    public Camera cam2;
    public bool swapped;

void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            swapped = !swapped;
        }

        if (swapped == true)
        {
            cam1.targetDisplay = 1;
            cam2.targetDisplay = 0;
        }

        if (swapped == false)
        {
            cam1.targetDisplay = 0;
            cam2.targetDisplay = 1;
        }

    }

}
