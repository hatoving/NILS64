using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTriggerObjectMoveChanger : MonoBehaviour
{
    public GameObject maincam;
    public bool switchit;
    public float speed = 10f;
    public Transform fixorigin;
    public Transform target;
    public float lookatspeed = 5f;
    public Transform lookat;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            maincam.GetComponent<MoveToTarget2>().switched = switchit;
            maincam.GetComponent<MoveToTarget2>().speed = speed;
            maincam.GetComponent<MoveToTarget2>().fixorigin = fixorigin;
            maincam.GetComponent<MoveToTarget2>().target = target;
            maincam.GetComponent<MoveToTarget2>().lookatspeed = lookatspeed;
            maincam.GetComponent<MoveToTarget2>().lookat = lookat;
        }

    }
  
}


