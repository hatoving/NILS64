using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogChangerLinear : MonoBehaviour
{
    public float fogDensityTarget;
    public float speed;
    public bool triggered;

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
            if (RenderSettings.fogEndDistance < fogDensityTarget)
            {
                RenderSettings.fogEndDistance += speed;
            }

            if (RenderSettings.fogEndDistance > fogDensityTarget)
            {
                RenderSettings.fogEndDistance -= speed;
            }
        }

    }

}