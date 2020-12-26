using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTriggerMatChange : MonoBehaviour
{
    public GameObject target;

    public Material[] Materials;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Material[] currentlyAssignedMaterials = target.GetComponent<Renderer>().materials;

            currentlyAssignedMaterials[0] = Materials[0];
            currentlyAssignedMaterials[1] = Materials[0];
            currentlyAssignedMaterials[2] = Materials[0];
            currentlyAssignedMaterials[3] = Materials[0];
            target.GetComponent<Renderer>().materials = currentlyAssignedMaterials;
        }

    }
  
}


