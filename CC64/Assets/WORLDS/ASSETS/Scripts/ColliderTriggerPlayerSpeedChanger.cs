using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTriggerPlayerSpeedChanger : MonoBehaviour
{
    public GameObject player;
    public float newforwardspeed;
    public float newsidespeed;
    public float newbackspeed;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player.GetComponent<PlayerController>().movement.forwardSpeed = newforwardspeed;
            player.GetComponent<PlayerController>().movement.sideSpeed = newsidespeed;
            player.GetComponent<PlayerController>().movement.backSpeed = newbackspeed;

        }

    }
  
}


