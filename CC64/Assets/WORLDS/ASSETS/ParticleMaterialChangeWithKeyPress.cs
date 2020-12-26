using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ParticleMaterialChangeWithKeyPress : MonoBehaviour
{
    public Material newMat;
 
 void Update()
    {
        if (Input.GetKeyDown("q"))
        {
            GetComponent<ParticleSystemRenderer>().material = newMat;
        }
    }

}