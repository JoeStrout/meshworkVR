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
	// But also keep track of which vertices are welded to which others:
	int[] weldGroup;	// index of lowest-numbered vertex this one is welded to

	MeshDisplay display;
	MeshCollider meshCollider;

	public struct MeshEdge {
		public MeshEdge(int a=0, int b=0) { index0 = a; index1 = b; }
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
	
	public void LoadMesh() {
		mesh = GetComponent<MeshFilter>().sharedMesh;
		vertices = mesh.vertices;
		uv = mesh.uv;
		triangles = mesh.triangles;
		RecalcWeldGroups();
		
		edges = new List<MeshEdge>();
		var tris = mesh.triangles;
		for (int i=0; i<tris.Length; i+=3) {
			edges.Add(new MeshEdge() { index0=tris[i+0], index1=tris[i+1] } );
			edges.Add(new MeshEdge() { index0=tris[i+1], index1=tris[i+2] } );
			edges.Add(new MeshEdge() { index0=tris[i+2], index1=tris[i+0] } );
		}
		Debug.Log($"{gameObject.name} with {tris.Length/3} triangles; {edges.Count} edges; {vertices.Length} vertices ");
		
		display = GetComponent<MeshDisplay>();
	}
	
	void RecalcWeldGroups() {
		if (weldGroup == null || weldGroup.Length != vertices.Length) weldGroup = new int[vertices.Length];
		for (int i=0; i<vertices.Length; i++) {
			if (i > 0 && weldGroup[i] > 0) continue;
			weldGroup[i] = i;
			Vector3 pos = vertices[i];
			for (int j=i+1; j<vertices.Length; j++) {
				if (weldGroup[j] == 0 && vertices[j] == pos) weldGroup[j] = i;
			}
		}
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
	public bool FindFace(Vector3 worldPos, Vector3 toolBase, float maxDist, out int outTriangle, out float outDistance) {
		outTriangle = -1;
		outDistance = Mathf.Infinity;
		Vector3 localEnd = transform.InverseTransformPoint(worldPos);
		Vector3 localBase = transform.InverseTransformPoint(toolBase);
		maxDist /= transform.lossyScale.x;
		var ray = new Ray(localBase, localEnd - localBase);
		Vector3 hitPoint;
		float hitDistance;
		int triBaseIndex;
		if (!MeshUtils.RayMeshIntersect(ray, vertices, triangles, out hitPoint, out hitDistance, out triBaseIndex)) return false;
		if (hitDistance > maxDist) return false;
		outTriangle = triBaseIndex / 3;
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
	
	public void RecalcBoundsAndNormals() {
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
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
		Vector3 v0 = vertices[triangles[triIndex*3+0]];
		Vector3 v1 = vertices[triangles[triIndex*3+1]];
		Vector3 v2 = vertices[triangles[triIndex*3+2]];
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
	
	/// Add all vertices in the selection to a dictionary with the vertex index as the key, 
	/// and the vertex position (relative to the given transform) as the value.
	/// Note that we include only vertices of the actual selected triangles, and NOT any
	/// other vertices welded to them.
	public void FindSelectionVertices(Dictionary<int, Vector3> positions, Transform relativeTo) {
		var disp = GetComponent<MeshDisplay>();
		for (int i=0; i<vertices.Length; i++) {
			if (disp.IsSelected(MeshEditMode.Vertex, i)) {
				Vector3 v = vertices[i];
				if (relativeTo != null && relativeTo != transform) {
					v = relativeTo.InverseTransformPoint(transform.TransformPoint(v));
				}
				positions[i] = v;
			}
		}
	}
	
	/// Return a list of triangle indexes for all selected triangles.
	public List<int> FindSelectedTriangles() {
		var disp = GetComponent<MeshDisplay>();
		var tris = new List<int>();
		for (int i=0; i<triangles.Length/3; i++) {
			if (disp.IsSelected(MeshEditMode.Face, i)) tris.Add(i);
		}
		return tris;
	}
	
	/// Find the edges along the outside of the current selection.  That is, all edges
	/// that belong to exactly one selected triangle.
	List<MeshEdge> FindSelectionEdges(List<int> tris) {
		var result = new List<MeshEdge>();
		
		// For each edge of each triangle, see if it is shared by any other triangles
		// (considering position and not just vertex numbers).
		// ToDo: is there some way to do this by weld group instead of position?
		for (int i=0; i<tris.Count; i++) {
			int iBase = tris[i]*3;
			Vector3 v0 = vertices[triangles[iBase + 0]];
			Vector3 v1 = vertices[triangles[iBase + 1]];
			Vector3 v2 = vertices[triangles[iBase + 2]];
			bool v0v1Edge = true, v1v2Edge = true, v2v0Edge = true;
			for (int j=0; j<tris.Count && (v0v1Edge || v1v2Edge || v2v0Edge); j++) {
				if (i == j) continue;
				int jBase = tris[j] * 3;
				if (HasEdge(jBase, v0, v1)) v0v1Edge = false;
				if (HasEdge(jBase, v1, v2)) v1v2Edge = false;
				if (HasEdge(jBase, v2, v0)) v2v0Edge = false;
			}
			// ...and store each edge NOT shared with any other triangle
			if (v0v1Edge) result.Add(new MeshEdge(iBase + 0, iBase + 1));
			if (v1v2Edge) result.Add(new MeshEdge(iBase + 1, iBase + 2));
			if (v2v0Edge) result.Add(new MeshEdge(iBase + 2, iBase + 0));
		}
		Debug.Log($"FindSelectionEdges found {result.Count} edges");
		return result;
	}
	
	// Do one extrusion step, i.e., duplicate all the edges around the selection
	// and connect them with quads to the old edges.
	public void DoExtrude() {
		var sb = new System.Text.StringBuilder();
		var disp = GetComponent<MeshDisplay>();
		for (int i=0; i<vertices.Length; i++) if (disp.IsSelected(MeshEditMode.Vertex,i)) {
			sb.Append($"{i}({weldGroup[i]}) ");
		}
		Debug.Log("Selection (before extrude): " + sb.ToString());

		// Start by finding the edges on the outside of the selection.
		List<int> selectedTriangles = FindSelectedTriangles();
		List<MeshModel.MeshEdge> edges = FindSelectionEdges(selectedTriangles);
		if (edges.Count == 0) return;
		
		// Now we need to create a quad for each of those by creating four
		// new vertices.  (You might think we could share some of the existing vertices,
		// but the wireframe renderer requires completely separate triangles anyway,
		// and it simplifies our data management to just do it now.)
		var newVerts = new List<Vector3>(vertices);
		var newUV = new List<Vector2>(uv);
		var newTris = new List<int>(triangles);
		var newColors = new List<Color32>(mesh.colors32);
		Color32 clear = Color.clear;
		Vector3 smallShift = new Vector3(0, 0.1f, 0);
		foreach (var edge in edges) {
			int idx = newVerts.Count;
			newVerts.Add(vertices[edge.index0]);	newUV.Add(uv[edge.index0]);		newColors.Add(clear);
			newVerts.Add(vertices[edge.index1]);	newUV.Add(uv[edge.index1]);		newColors.Add(clear);
			newVerts.Add(vertices[edge.index0]+smallShift);	newUV.Add(uv[edge.index0]);		newColors.Add(clear);
			newVerts.Add(vertices[edge.index1]+smallShift);	newUV.Add(uv[edge.index1]);		newColors.Add(clear);

			newTris.Add(idx+0); newTris.Add(idx+1); newTris.Add(idx+2);
			newTris.Add(idx+2); newTris.Add(idx+1); newTris.Add(idx+3);
			
			Debug.Log($"Extrude: created new quad {idx}, {idx+1}, {idx+2}, {idx+3}");
		}
		
		// Drat, that's not good enough.  We need to actually shift the vertices in
		// the selection set so they no longer match the ones we don't want to move.
		foreach (int triIdx in selectedTriangles) {
			Debug.Log($"Shifting {newTris[triIdx*3]+0}, {newTris[triIdx*3]+1}, and {newTris[triIdx*3]+2}");
			newVerts[newTris[triIdx*3]+0] += smallShift;
			newVerts[newTris[triIdx*3]+1] += smallShift;
			newVerts[newTris[triIdx*3]+2] += smallShift;		
		}
		Debug.Log($"after shift: 24@{newVerts[24]}, 25@{newVerts[25]}, 36@{newVerts[36]}, 37@{newVerts[37]}");
		
		// Update the mesh.
		mesh.vertices = newVerts.ToArray();
		mesh.uv = newUV.ToArray();
		mesh.triangles = newTris.ToArray();
		mesh.colors32 = newColors.ToArray();
		mesh.RecalculateNormals();

		sb = new System.Text.StringBuilder();
		for (int i=0; i<vertices.Length; i++) if (mesh.colors32[i].a > 128) {
			sb.Append($"{i} ");
		}
		Debug.Log("Selection (from mesh.colors before bake): " + sb.ToString());

		GetComponent<MeshDisplay>().Rebake();

		Debug.Log($"after bake: 24@{mesh.vertices[24]}, 24@{mesh.vertices[24]}, 36@{mesh.vertices[24]}, 37@{mesh.vertices[24]}");

		Debug.Log($"Weld groups: 25->{weldGroup[25]}, 26->{weldGroup[26]}, 36->{weldGroup[36]}, 37->{weldGroup[37]}");
		sb = new System.Text.StringBuilder();
		for (int i=0; i<vertices.Length; i++) if (disp.IsSelected(MeshEditMode.Vertex,i)) {
			sb.Append($"{i}({weldGroup[i]}) ");
		}
		Debug.Log("Selection (after extrude): " + sb.ToString());
	}

	/// <summary>
	/// Return whether the triangle starting at triBaseIndex has an edge matching v0,v1.
	/// </summary>
	public bool HasEdge(int triBaseIndex, Vector3 v0, Vector3 v1) {
		Vector3 a = vertices[triBaseIndex+0];
		Vector3 b = vertices[triBaseIndex+1];
		Vector3 c = vertices[triBaseIndex+2];
		if (a == v0 && b == v1 || (a == v1 && b == v0)) return true;
		if (b == v0 && c == v1 || (b == v1 && c == v0)) return true;
		if (c == v0 && a == v1 || (c == v1 && a == v0)) return true;
		return false;
	}
	
	public Vector3 TriangleNormal(int triBaseIndex) {
		return TriangleNormal(
			vertices[triangles[triBaseIndex+0]],
			vertices[triangles[triBaseIndex+1]],
			vertices[triangles[triBaseIndex+2]]);
	}
	
	public Vector3 TriangleNormal(Vector3 v0, Vector3 v1, Vector3 v2) {
		return Vector3.Cross(v1 - v0, v2 - v0);
	}
	
	/// <summary>
	/// Add the given delta to the UV of the given vertex, and any other
	/// vertices in the same weld group AND using the same UV.
	/// </summary>
	/// <param name="index"></param>
	/// <param name="dUV"></param>
	public void ShiftUV(int index, Vector2 dUV) {
		Vector3 vpos = vertices[index];
		Vector2 oldUV = uv[index];
		Vector2 newUV = oldUV + dUV;
		int weld = weldGroup[index];
		for (int i=0; i<uv.Length; i++) {
			if (weldGroup[i] == weld && uv[i] == oldUV) {
				uv[i] = newUV;
				onUVChanged.Invoke(i);
			}
		}
		mesh.uv = uv;
	}
	
	/// <summary>
	/// Add the given delta to the position of the given vertex, and
	/// any other vertices in the same weld group.
	/// </summary>
	public void ShiftVertexTo(int index, Vector3 newPos, bool updateMesh=true) {
		int weld = weldGroup[index];
		for (int i=0; i<uv.Length; i++) {
			if (weldGroup[i] == weld) vertices[i] = newPos;
		}
		if (!updateMesh) return;
		mesh.vertices = vertices;
		meshCollider.sharedMesh = mesh;	// (forces collider to re-cook)
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
