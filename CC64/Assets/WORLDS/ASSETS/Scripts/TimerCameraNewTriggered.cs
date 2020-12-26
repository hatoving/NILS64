using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerCameraNewTriggered : MonoBehaviour
{
    public GameObject target;
    public int counter;
    public bool NewTriggered;
    public Transform NewCameraTarget;
    public float NewcameraDistance = 2.95f;
    public float NewcameraHeight = 1f;
    public float NewplayerHeight;
    public float NewSpeed = 0.1f;

    void Update()
    {
        counter -= 1;

        if (counter <= 0)
        {
            target.GetComponent<CameraController>().NewTriggered = NewTriggered;
            target.GetComponent<CameraController>().NewCameraTarget = NewCameraTarget;
            target.GetComponent<CameraController>().NewcameraDistance = NewcameraDistance;
            target.GetComponent<CameraController>().NewcameraHeight = NewcameraHeight;
            target.GetComponent<CameraController>().NewplayerHeight = NewplayerHeight;
            target.GetComponent<CameraController>().NewSpeed = NewSpeed;
        }

    }


}