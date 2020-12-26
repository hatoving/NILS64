using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyTriggerFTargetPlaceLook : MonoBehaviour
{
    public GameObject moveobject;
    public GameObject lookattarget;
    public Vector3 position;

void Update()
    {
        if (Input.GetKeyDown("e"))
        {
            moveobject.gameObject.SetActive(true);
            Vector3 targetPosition = new Vector3(lookattarget.transform.position.x,
                                     transform.position.y,
                                     lookattarget.transform.position.z);
            moveobject.transform.LookAt(targetPosition);
                     moveobject.transform.position = position;
        }

    }
  
}


