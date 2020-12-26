using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimKeyTriggerC : MonoBehaviour
{
    [SerializeField] private Animator myAnimationController;
    [SerializeField] private Animator myAnimationController2;

    void Update ()
    {
        if (Input.GetKeyDown("c"))
        {
            myAnimationController.SetBool("trig", true);
            myAnimationController2.SetBool("trig", true);
        }

    }
}


