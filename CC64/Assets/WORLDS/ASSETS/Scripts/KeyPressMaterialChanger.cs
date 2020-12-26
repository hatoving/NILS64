using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Pin2 : MonoBehaviour
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

    }
}