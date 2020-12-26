using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTriggerRotaterChanger : MonoBehaviour
{
    public GameObject maincam;
    public int XRot;
    public int YRot;
    public int ZRot;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            maincam.GetComponent<Rotater>().XRot = XRot;
            maincam.GetComponent<Rotater>().YRot = YRot;
            maincam.GetComponent<Rotater>().ZRot = ZRot;
        }

    }
  
}


