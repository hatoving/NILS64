using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MaterialChangeWithKeyPress : MonoBehaviour
{
    public int CurrentMaterial = 0;
    public Material[] Materials;
    // Use this for initialization
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("1"))
        {
            Material[] currentlyAssignedMaterials = GetComponent<Renderer>().materials;
            currentlyAssignedMaterials[0] = Materials[1];
            currentlyAssignedMaterials[1] = Materials[1];
            currentlyAssignedMaterials[2] = Materials[1];
            currentlyAssignedMaterials[3] = Materials[1];
            currentlyAssignedMaterials[4] = Materials[1];
            currentlyAssignedMaterials[5] = Materials[1];
            GetComponent<Renderer>().materials = currentlyAssignedMaterials;
        }
        if (Input.GetKeyDown("0"))
        {
            Material[] currentlyAssignedMaterials = GetComponent<Renderer>().materials;
            currentlyAssignedMaterials[0] = Materials[0];
            currentlyAssignedMaterials[1] = Materials[0];
            currentlyAssignedMaterials[2] = Materials[0];
            currentlyAssignedMaterials[3] = Materials[0];
            currentlyAssignedMaterials[4] = Materials[0];
            currentlyAssignedMaterials[5] = Materials[0];
            GetComponent<Renderer>().materials = currentlyAssignedMaterials;
        }
        if (Input.GetKeyDown("2"))
        {
            Material[] currentlyAssignedMaterials = GetComponent<Renderer>().materials;
            currentlyAssignedMaterials[0] = Materials[2];
            currentlyAssignedMaterials[1] = Materials[2];
            currentlyAssignedMaterials[2] = Materials[2];
            currentlyAssignedMaterials[3] = Materials[2];
            currentlyAssignedMaterials[4] = Materials[2];
            currentlyAssignedMaterials[5] = Materials[2];
            GetComponent<Renderer>().materials = currentlyAssignedMaterials;
        }
        if (Input.GetKeyDown("3"))
        {
            Material[] currentlyAssignedMaterials = GetComponent<Renderer>().materials;
            currentlyAssignedMaterials[0] = Materials[3];
            currentlyAssignedMaterials[1] = Materials[3];
            currentlyAssignedMaterials[2] = Materials[3];
            currentlyAssignedMaterials[3] = Materials[3];
            currentlyAssignedMaterials[4] = Materials[3];
            currentlyAssignedMaterials[5] = Materials[3];
            GetComponent<Renderer>().materials = currentlyAssignedMaterials;
        }
        if (Input.GetKeyDown("4"))
        {
            Material[] currentlyAssignedMaterials = GetComponent<Renderer>().materials;
            currentlyAssignedMaterials[0] = Materials[4];
            currentlyAssignedMaterials[1] = Materials[4];
            currentlyAssignedMaterials[2] = Materials[4];
            currentlyAssignedMaterials[3] = Materials[4];
            currentlyAssignedMaterials[4] = Materials[4];
            currentlyAssignedMaterials[5] = Materials[4];
            GetComponent<Renderer>().materials = currentlyAssignedMaterials;
        }
    }
}