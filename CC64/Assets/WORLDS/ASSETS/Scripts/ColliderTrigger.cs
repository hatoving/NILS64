using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTrigger : MonoBehaviour
{
    [SerializeField] private Animator myAnimationController;
    [SerializeField] private Animator myAnimationController2;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            myAnimationController.SetBool("doortrig", true);
            myAnimationController2.SetBool("doortrig", true);
        }

    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            myAnimationController.SetBool("doortrig", false);
            myAnimationController2.SetBool("doortrig", false);
        }

    }
}


