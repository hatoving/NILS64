using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionChanger : MonoBehaviour
{
        void Start()
        {
            // Switch to 640 x 480 full-screen at 60 hz
            Screen.SetResolution(640, 480, true, 60);
        }

}
