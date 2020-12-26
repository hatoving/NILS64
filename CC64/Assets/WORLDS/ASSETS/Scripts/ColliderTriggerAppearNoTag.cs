using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTriggerAppearNoTag : MonoBehaviour
{
    public GameObject target;


    private void OnTriggerEnter(Collider other)
    {
            target.gameObject.SetActive(true);
    }
  
}


