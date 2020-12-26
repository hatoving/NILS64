using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Pin : MonoBehaviour
{
    public int timer;
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
        timer++;
        if (timer >= 100)
        {
            timer = 0;
            if (CurrentMaterial == Materials.Length)
            {
                CurrentMaterial = 0;
            }
            else
            {
                CurrentMaterial++;
            }

            Material[] currentlyAssignedMaterials = GetComponent<Renderer>().materials;

            currentlyAssignedMaterials[1] = Materials[CurrentMaterial];
            GetComponent<Renderer>().materials = currentlyAssignedMaterials;
        }
    }
}