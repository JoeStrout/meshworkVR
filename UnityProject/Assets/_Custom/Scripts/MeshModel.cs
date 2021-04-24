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
	public bool FindVertexIndex(Vector3 worldPos, Vector3 toolBase, float maxDist, out int outIndex, out float outDistance) {
		outIndex = -1;
		outDistance = Mathf.Infinity;
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
		outDistance = bestDist * transform.lossyScale.x;
		return true;
	}
	
	/// <summary>
	/// Find the face closest to the given position, by casting a ray onto the mesh.
	/// It must be within maxDist; return the actual distance in outDistance.
	/// </summary>
	public bool FindFace(Vector3 worldPos, Vector3 toolBase, float maxDist, out int outIndex, out float outDistance) {
		outIndex = -1;
		outDistance = Mathf.Infinity;
		Vector3 localEnd = transform.InverseTransformPoint(worldPos);
		Vector3 localBase = transform.InverseTransformPoint(toolBase);
		maxDist /= transform.lossyScale.x;
		var ray = new Ray(localBase, localEnd - localBase);
		Vector3 hitPoint;
		float hitDistance;
		int hitTriangle;
		if (!MeshUtils.RayMeshIntersect(ray, vertices, triangles, out hitPoint, out hitDistance, out hitTriangle)) return false;
		if (hitDistance > maxDist) return false;
		outIndex = hitTriangle;
		outDistance = hitDistance;
		return true;
	}
	
	/// <summary>
	/// Add the vertexes involved in the given triangle to a dictionary with the vertex index 
	/// as the key,  and the vertex position (relative to the given transform) as the value.
	/// </summary>
	public void AddTriVertices(int triIndex, Dictionary<int, Vector3> positions, Transform relativeTo) {
		for (int i=0; i<3; i++) {
			int vertexIdx = triangles[triIndex + i];
			Vector3 v = vertices[vertexIdx];
			if (relativeTo != null && relativeTo != transform) {
				v = relativeTo.InverseTransformPoint(transform.TransformPoint(v));
			}
			positions[vertexIdx] = v;
		}
	}
	
	/// <summary>
	/// Find the vertexes involved in the given triangle (and if it's part of a quad, the
	/// partner triangle).  Add these to a dictionary with the vertex index as the key, 
	/// and the vertex position (relative to the given transform) as the value.
	/// </summary>
	public void FindFaceVertices(int triIndex, Dictionary<int, Vector3> positions, Transform relativeTo) {
		// Add the given triangle.
		AddTriVertices(triIndex, positions, relativeTo);
		// Now search for another triangle with a shared edge, the same normal,
		// and the most acute corner angles on that edge.
		Vector3 v0 = vertices[triangles[triIndex+0]];
		Vector3 v1 = vertices[triangles[triIndex+1]];
		Vector3 v2 = vertices[triangles[triIndex+2]];
		Vector3 normal = TriangleNormal(v0, v1, v2);
		for (int t=0; t<triangles.Length; t += 3) {
			if (t == triIndex) continue;
			Vector3 n = TriangleNormal(t);
			if (!n.ApproximatelyEqual(normal)) continue;
			int shared = 0;
			for (int j=0; j<3; j++) {
				Vector3 tv = vertices[triangles[t+j]];
				if (tv == v0 || tv == v1 || tv == v2) shared++;
			}
			if (shared != 2) continue;
			// ToDo: also check angles and when there are multiple shared coplanar
			// triangles, pick the best one.
			// For now, we'll just go wit the first such triangle we find.
			AddTriVertices(t, positions, relativeTo);
			break;
		}
	}
	
	public Vector3 TriangleNormal(int triIndex) {
		return TriangleNormal(
			vertices[triangles[triIndex+0]],
			vertices[triangles[triIndex+1]],
			vertices[triangles[triIndex+2]]);
	}
	
	public Vector3 TriangleNormal(Vector3 v0, Vector3 v1, Vector3 v2) {
		return Vector3.Cross(v1 - v0, v2 - v0);
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
	public void ShiftVertexTo(int index, Vector3 newPos, bool updateMesh=true) {
		Vector3 oldPos = vertices[index];
		for (int i=0; i<uv.Length; i++) {
			if (i == index || vertices[i] == oldPos) {
				vertices[i] = newPos;
			}
		}
		if (!updateMesh) return;
		mesh.vertices = vertices;
		meshCollider.sharedMesh = mesh;	// (forces collider to re-cook)
		if (display != null) display.ShiftVertexTo(oldPos, newPos);
	}
	
	/// <summary>
	/// Shift the given triangle face by the given delta in local coordinates.
	/// </summary>
	public void ShiftFace(int triIndex, Vector3 localDelta) {
		for (int i=0; i<3; i++) {
			int vertIdx = triangles[triIndex + i];
			ShiftVertexTo(vertIdx, vertices[vertIdx] + localDelta, false);
		}
		mesh.vertices = vertices;
		meshCollider.sharedMesh = mesh;	// (forces collider to re-cook)
	}
}
