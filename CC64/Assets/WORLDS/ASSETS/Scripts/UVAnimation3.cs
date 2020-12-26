using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVAnimation3 : MonoBehaviour {

    public int uvTileY = 4;
    public int uvTileX = 4;

    public int fps = 4;

    private int index;

	// Update is called once per frame
	void Update () 

    {
        index = (int)(Time.time * fps);

        index = index % (uvTileY * uvTileX);

        Vector2 size = new Vector2(1.0f / uvTileY, 1.0f / uvTileX);

        var uIndex = index % uvTileX;
        var vIndex = index / uvTileX;

        //v coordinate is at the bottom of the image in openGL, so we invert it

        Vector2 offset = new Vector2(uIndex * size.x, 1.0f - size.y - vIndex * size.y);

        GetComponent<Renderer>().material.SetTextureOffset ("_MainTex", offset);
        GetComponent<Renderer>().material.SetTextureScale ("_MainTex", size);
                                
	}
}
