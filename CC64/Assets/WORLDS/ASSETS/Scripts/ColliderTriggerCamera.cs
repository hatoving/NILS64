using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTriggerCamera : MonoBehaviour
{
    public Camera controlledtarget;
    public Camera maintarget;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            controlledtarget.GetComponent<CameraController>().NewTriggered = true;
            maintarget.GetComponent<MoveToTarget2>().switched = false;
        }

    }
  
}


