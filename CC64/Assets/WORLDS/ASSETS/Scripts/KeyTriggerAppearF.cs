using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyTriggerAppearF : MonoBehaviour
{
    public GameObject target;


void Update()
    {
        if (Input.GetKeyDown("f"))
        {
            target.gameObject.SetActive(true);
        }

    }
  
}


