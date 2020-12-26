using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateAtStart : MonoBehaviour
{

    public Transform Spawnpoint;
    public GameObject Prefab;
    public Vector3 Offset;
    public float xrot;
    public float yrot;
    public float zrot;

    void Start()
    {
        Instantiate(Prefab, (Spawnpoint.position + Offset), Quaternion.Euler(xrot, yrot, zrot));
    }
}
