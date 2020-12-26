using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationOffset : MonoBehaviour
{

    public float myOffset;
    [SerializeField] private Animator myAnimationController;

    void Start()
    {
        myAnimationController.SetFloat("Offset", myOffset);
    }


    void Update()
    {
        
    }
}
