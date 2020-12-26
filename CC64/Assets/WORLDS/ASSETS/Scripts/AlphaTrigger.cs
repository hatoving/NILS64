using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlphaTrigger : MonoBehaviour
{
    public GameObject target;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (target.gameObject.activeInHierarchy == true)
            { 
            target.GetComponent<AlphaChanger>().triggered = true;
            }
        }

    }

}


