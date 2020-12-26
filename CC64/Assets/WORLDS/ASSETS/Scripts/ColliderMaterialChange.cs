using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderMaterialChange : MonoBehaviour
{
    public Material[] material;
    Renderer rend;


    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.enabled = true;  
    }

    void Update()
    {
        if (Input.GetKeyDown("0"))
        {
            rend.sharedMaterial = material[0];
        }

        if (Input.GetKeyDown("1"))
        {
            rend.sharedMaterial = material[1];
        }

        if (Input.GetKeyDown("2"))
        {
            rend.sharedMaterial = material[2];
        }

        if (Input.GetKeyDown("3"))
        {
            rend.sharedMaterial = material[3];
        }

        if (Input.GetKeyDown("4"))
        {
            rend.sharedMaterial = material[1];
        }
    }

}
