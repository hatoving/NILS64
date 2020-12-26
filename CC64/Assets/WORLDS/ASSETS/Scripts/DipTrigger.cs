using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DipTrigger : MonoBehaviour
{
    public GameObject target;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            target.GetComponent<Dipper>().triggered = true;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            target.GetComponent<Dipper>().triggered = false;
        }

    }

}


