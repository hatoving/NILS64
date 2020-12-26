using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderCameraChangeDampening : MonoBehaviour
{
    public GameObject target;
    public float changespeed = 0.001f;
    public bool triggered;
    public float damptarget = 0.2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            triggered = true;
            }

    }

    private void Update()
    {
        if (triggered == true)
        { 
        if  (target.GetComponent<CameraController>().follow.movementDampening > damptarget)
        {
            target.GetComponent<CameraController>().follow.movementDampening -= changespeed;
        }
            if (target.GetComponent<CameraController>().follow.movementDampening < damptarget)
            {
                target.GetComponent<CameraController>().follow.movementDampening += changespeed;
            }

        }
    }

        }
