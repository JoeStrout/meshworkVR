/*
This class makes a 3D line, which is to say a skinny cylinder, between two points.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class Line3D : MonoBehaviour
{

	const int sides = 6;
	const float minRadius = 0.005f;

	Mesh mesh;
	Vector3[] vertices;
	
	public Vector3[] endPoints = new Vector3[2];
	Vector3[] prevEndpoints = new Vector3[2];
	
	protected void Start() {
		// Set up the cylindrical mesh.  Actual vertex positions will be set
		// later, in UpdateMesh.
		vertices = new Vector3[sides * 2];
		mesh = new Mesh();
		mesh.name = gameObject.name + " Mesh";
		mesh.vertices = vertices;
		var tris = new List<int>();
		for (int i=0; i<sides; i++) {
			AddQuad(tris, i, (i+1)%sides, (i+1)%sides + sides, i + sides);
		}
		mesh.triangles = tris.ToArray();
		
		UpdateMesh();
		
		GetComponent<MeshFilter>().sharedMesh = mesh;
		var mc = GetComponent<MeshCollider>();
		if (mc != null) mc.sharedMesh = mesh;
	}
	
	protected void Update() {
		if (endPoints[0] != prevEndpoints[0] || endPoints[1] != prevEndpoints[1]) {
			UpdateMesh();
			prevEndpoints[0] = endPoints[0];
			prevEndpoints[1] = endPoints[1];
		}
	}
	
	void UpdateMesh() {
		// Update the positions of the vertices.
		UpdateVerts(vertices, 0, endPoints[0], endPoints[1]);
		
		// Wrap up.
		mesh.vertices = vertices;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
	}
	
	public static void UpdateVerts(Vector3[] vertices, int startIdx, Vector3 p0, Vector3 p1) {
		// Start by finding a perpendicular to the axis.
		Vector3 axis = p1 - p0;
		float length = axis.magnitude;
		if (length < 0.00001f) length = 0.00001f;
		axis /= length;
		Vector3 n;
		if (Mathf.Abs(axis.x) > Mathf.Abs(axis.z)) n = new Vector3(-axis.y - axis.z, axis.x, axis.x);
		else n = new Vector3(axis.z, axis.z, -axis.x - axis.y);
		n.Normalize();
		
		// Find the proper radius.
		float r = minRadius;
		if (length < r) r = length;
		n *= r;
		
		// Now we can rotate this around the axis for each side.
		float angle = 360 / sides;
		for (int i=0; i<sides; i++) {
			Vector3 rp = MathUtils.Vector3Rotate(n, axis, angle * i);
			vertices[startIdx + i] = p0+ rp;
			vertices[startIdx + i + sides] = p1 + rp;
		}

	}
	
	public static void AddQuad(List<int> tris, int a, int b, int c, int d) {
		tris.Add(a); tris.Add(b); tris.Add(c);
		tris.Add(c); tris.Add(d); tris.Add(a);
	}
}
