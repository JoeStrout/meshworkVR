/*
This script represents the data model for a single mesh, including vertices and
edges that are not actually attached to triangles yet.  All the editing tools and
display components should go through this object to get data about the mesh.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshModel : MonoBehaviour
{
	public IntEvent onUVChanged;
	
	// For now, we'll just wrap a Unity mesh.
	Mesh mesh;
	Vector3[] vertices;
	Vector2[] uv;
	int[] triangles;

	MeshDisplay display;

	MeshCollider meshCollider;

	public struct MeshEdge {
		public int index0;
		public int index1;
	}

	List<MeshEdge> edges;

	public int vertexCount {
		get { return vertices.Length; }
	}
	public int edgeCount {
		get { return edges.Count; }
	}
	
	protected void OnValidate() {
		MeshFilter mf = GetComponent<MeshFilter>();
		if (mf == null) Debug.LogError("MeshFilter needed on " + gameObject.name, gameObject);
		else if (mf.sharedMesh == null) Debug.LogError($"Meshfilter on " +gameObject.name + " needs a mesh", gameObject);
		else if (!mf.sharedMesh.isReadable)  Debug.LogError($"Meshfilter on " +gameObject.name + " needs Read/Write Enabled", gameObject);
	}
	
	protected void Awake() {
		meshCollider = GetComponent<MeshCollider>();
	}
	
	protected void Start() {
		mesh = GetComponent<MeshFilter>().sharedMesh;
		vertices = mesh.vertices;
		uv = mesh.uv;
		triangles = mesh.triangles;
		
		edges = new List<MeshEdge>();
		var tris = mesh.triangles;
		for (int i=0; i<tris.Length; i+=3) {
			edges.Add(new MeshEdge() { index0=tris[i+0], index1=tris[i+1] } );
			edges.Add(new MeshEdge() { index0=tris[i+1], index1=tris[i+2] } );
			edges.Add(new MeshEdge() { index0=tris[i+2], index1=tris[i+0] } );
		}
		Debug.Log($"{gameObject.name} with {tris.Length/3} triangles: {edges.Count} edges");
		
		display = GetComponent<MeshDisplay>();
	}
	
	
	public Vector3 Vertex(int idx) { return vertices[idx]; }
	
	public Vector2 UV(int idx) { return uv[idx]; }
	
	public MeshEdge Edge(int idx) { return edges[idx]; }
	
	/// <summary>
	/// Find the vertex closest to the given position, by casting a ray onto the mesh
	/// and then moving to the closest vertex of the triangle hit.
	/// </summary>
	public bool FindIndex(Vector3 worldPos, Vector3 toolBase, float maxDist, out int outIndex) {
		outIndex = -1;
		Vector3 localEnd = transform.InverseTransformPoint(worldPos);
		Vector3 localBase = transform.InverseTransformPoint(toolBase);
		maxDist /= transform.lossyScale.x;
		var ray = new Ray(localBase, localEnd - localBase);
		Vector3 hitPoint;
		float hitDistance;
		int hitTriangle;
		if (!MeshUtils.RayMeshIntersect(ray, vertices, triangles, out hitPoint, out hitDistance, out hitTriangle)) return false;
		if (hitDistance > maxDist) return false;
		int bestIdx = 0;
		float bestDist = Vector3.Distance(vertices[triangles[hitTriangle]], localEnd);
		for (int i=1; i<3; i++) {
			float dist = Vector3.Distance(vertices[triangles[hitTriangle+i]], localEnd);
			if (dist < bestDist) {
				bestDist = dist;
				bestIdx = i;
			}
		}
		if (bestDist > maxDist) return false;
		outIndex = triangles[hitTriangle+bestIdx];
		return true;
	}
	
	/// <summary>
	/// Add the given delta to the UV of the given vertex, and any other
	/// vertices which share the same position and UV coordinates.
	/// </summary>
	/// <param name="index"></param>
	/// <param name="dUV"></param>
	public void ShiftUV(int index, Vector2 dUV) {
		Vector3 vpos = vertices[index];
		Vector2 oldUV = uv[index];
		Vector2 newUV = oldUV + dUV;
		for (int i=0; i<uv.Length; i++) {
			if (i == index || (vertices[i] == vpos && uv[i] == oldUV)) {
				uv[i] = newUV;
				onUVChanged.Invoke(i);
			}
		}
		mesh.uv = uv;
	}
	
	/// <summary>
	/// Add the given delta to the position of the given vertex, and
	/// any other vertices that share the same position.
	/// </summary>
	public void ShiftVertexTo(int index, Vector3 newPos) {
		Vector3 oldPos = vertices[index];
		for (int i=0; i<uv.Length; i++) {
			if (i == index || vertices[i] == oldPos) {
				vertices[i] = newPos;
			}
		}
		mesh.vertices = vertices;
		meshCollider.sharedMesh = mesh;	// (forces collider to re-cook)
		if (display != null) display.ShiftVertexTo(oldPos, newPos);
	}
}
