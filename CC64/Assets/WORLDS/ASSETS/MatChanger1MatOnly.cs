﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MatChanger1MatOnly : MonoBehaviour
{
    public Material[] Materials;
    // Use this for initialization
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("0"))
        {
            Material[] currentlyAssignedMaterials = GetComponent<Renderer>().materials;

            currentlyAssignedMaterials[0] = Materials[0];
            GetComponent<Renderer>().materials = currentlyAssignedMaterials;
        }
        if (Input.GetKeyDown("1"))
        {
            Material[] currentlyAssignedMaterials = GetComponent<Renderer>().materials;

            currentlyAssignedMaterials[0] = Materials[1];
            GetComponent<Renderer>().materials = currentlyAssignedMaterials;
        }
        if (Input.GetKeyDown("2"))
        {
            Material[] currentlyAssignedMaterials = GetComponent<Renderer>().materials;

            currentlyAssignedMaterials[0] = Materials[2];
            GetComponent<Renderer>().materials = currentlyAssignedMaterials;
        }
        if (Input.GetKeyDown("3"))
        {
            Material[] currentlyAssignedMaterials = GetComponent<Renderer>().materials;

            currentlyAssignedMaterials[0] = Materials[3];
            GetComponent<Renderer>().materials = currentlyAssignedMaterials;
        }
    }
}