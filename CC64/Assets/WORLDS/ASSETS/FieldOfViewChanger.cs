using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfViewChanger : MonoBehaviour
{
    public float FOVTarget;
    public float speed;
    public bool triggered;
    public Camera CameraTarget;

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
            if (CameraTarget.fieldOfView < FOVTarget)
            {
                CameraTarget.fieldOfView += speed;
            }

            if (CameraTarget.fieldOfView > FOVTarget)
            {
                CameraTarget.fieldOfView -= speed;
            }
        }

    }

}