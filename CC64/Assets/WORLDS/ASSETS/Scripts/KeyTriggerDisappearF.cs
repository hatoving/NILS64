using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyTriggerDisappearF : MonoBehaviour
{
    public GameObject target;


void Update()
    {
        if (Input.GetKeyDown("f"))
        {
            target.gameObject.SetActive(false);
        }

    }
  
}


