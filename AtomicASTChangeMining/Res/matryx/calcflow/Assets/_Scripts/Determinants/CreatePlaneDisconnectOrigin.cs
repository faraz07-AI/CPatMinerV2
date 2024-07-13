﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePlaneDisconnectOrigin : MonoBehaviour {

	public Transform FirstPoint;
    public Transform SecondPoint;
	public Transform ThirdPoint;
	
	// Use this for initialization
	void Update () {
        var firstposition = FirstPoint.localPosition;
        var secondposition = SecondPoint.localPosition;
        var thirdposition = ThirdPoint.localPosition;

		var mesh = new Mesh();
        gameObject.GetComponent<MeshFilter>().mesh = mesh;

		var vertices = new Vector3[4]
        {
            firstposition,
			firstposition+secondposition,
			firstposition+thirdposition,
			firstposition+secondposition+thirdposition,
        };
        mesh.vertices = vertices;

        var tris = new int[6]
        {
            // lower left triangle
            0, 1, 2,
            // upper right triangle
            1, 3, 2,
        };
        mesh.triangles = tris;

        var normals = new Vector3[4]
        {
            Vector3.forward,
            Vector3.forward,
            Vector3.forward,
            Vector3.forward
        };
        mesh.normals = normals;

        var uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;
	}
}
