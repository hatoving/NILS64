using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MatChanger2MatOnlyFlicker : MonoBehaviour
{
    public Material[] Materials;
    public bool switched;
    public float counter = 0;
    public float interval = 5;


    // Use this for initialization
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        counter += 1;

        if (counter >= interval)
        {
            switched = !switched;
        }

        if (switched == true)
        {
            Material[] currentlyAssignedMaterials = GetComponent<Renderer>().materials;

            currentlyAssignedMaterials[0] = Materials[0];
            currentlyAssignedMaterials[1] = Materials[0];
            GetComponent<Renderer>().materials = currentlyAssignedMaterials;
        }
        if (switched == false)
        {
            Material[] currentlyAssignedMaterials = GetComponent<Renderer>().materials;

            currentlyAssignedMaterials[0] = Materials[1];
            currentlyAssignedMaterials[1] = Materials[1];
            GetComponent<Renderer>().materials = currentlyAssignedMaterials;
        }
   
    }
}