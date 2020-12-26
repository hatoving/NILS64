using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTriggerAppear : MonoBehaviour
{
    public GameObject target;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            target.gameObject.SetActive(true);
        }

    }
  
}


