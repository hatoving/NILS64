/// All in One Game Kit - Easy Ledge Climb Character System
/// AutoDestroyParticleSystem.cs
///
/// Automatically destroys script holder once ParticleSystem has completed.
///
/// (C) 2015-2018 Grant Marrs

using UnityEngine;
using System.Collections;

public class AutoDestroyParticleSystem : MonoBehaviour 
{

    public void Update() 
    {
        if(GetComponent<ParticleSystem>())
        {
            if(!GetComponent<ParticleSystem>().IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }
	
}