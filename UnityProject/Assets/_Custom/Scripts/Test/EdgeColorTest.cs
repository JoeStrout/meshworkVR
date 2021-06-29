/*
Experiments in coloring the wireframe lines via a texture using uv2.

HOW THIS WORKS:
- Special coordinates are stored in UV2 for each triangle:
	for vertex 0, we store 0, 0;
	for vertex 1, we store 1, 0;
	for vertex 2, we store 0.5, 1.

- The triangle color (i.e., vertex color for all three verts in the triangle)
is then set to a special color that indicates which combination of edges are
currently selected, by storing 0 (unselected) or 255 (selected) in each of the
three color channels: red for edge 0, green for edge 1, and blue for edge 2.

- The CustomMeshworkEdge shader then uses the UV2 coordinates and color of each
fragment to figure out whether to use the standard or highlighted wire color.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeColorTest : MonoBehaviour
{
	protected void Start() {
		Invoke("ApplyColors", 0.1f);
	}
	
	void ApplyColors() {
		Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
		Debug.Log("Found mesh with " + mesh.vertexCount + " vertices");
		
		// Assign UVs in a special pattern per triangle, which the shader can
		// use to figure out which side of the triangle it's on.
		var uv2 = new Vector2[6];
		int[] tris = mesh.triangles;
		for (int trinum=0; trinum<tris.Length; trinum += 3) {
			uv2[tris[trinum]] = Vector2.zero;
			uv2[tris[trinum+1]] = new Vector2(1,0);
			uv2[tris[trinum+2]] = new Vector2(0.5f, 1);
		}
		mesh.uv2 = uv2;
		
		InvokeRepeating("SelNextEdge", 0, 1);
	}
	
	int stepNum = 0;
	void SelNextEdge() {
		stepNum = (stepNum + 1) % 8;

		Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
		int[] tris = mesh.triangles;

		// Now indicate which of the three edges are selected via the vertex
		// color: red indicates first edge, green second edge, blue third edge.
		Color32[] colors = mesh.colors32;
		for (int trinum=0; trinum<tris.Length; trinum += 3) {
			Color32 c = Color.black;
			if ((stepNum & 1) > 0) c.r = 255;
			if ((stepNum & 2) > 0) c.g = 255;
			if ((stepNum & 4) > 0) c.b = 255;
			
			colors[tris[trinum]] = colors[tris[trinum+1]] = colors[tris[trinum+2]] = c;
		}
		mesh.colors32 = colors;
		
	}
}
