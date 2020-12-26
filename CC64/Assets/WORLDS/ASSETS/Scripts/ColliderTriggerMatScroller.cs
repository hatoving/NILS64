using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTriggerMatScroller : MonoBehaviour
{
    public GameObject target;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            target.GetComponent<ScrollTexture>().scrollHorizontally = true;
        }

    }
  
}


