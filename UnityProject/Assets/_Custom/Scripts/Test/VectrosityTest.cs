using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

public class VectrosityTest : MonoBehaviour
{
	public Texture2D lineTexture;
	public Texture2D pointTexture;
	
	VectorLine multiline;	// Discrete type "line" that is actually all our line segments in one.
	VectorLine vertices;	// Points type "line" that draws a bunch of points
	
	Mesh mesh;
	
	protected void Start() {
		AddLine(new Vector3(0,0,0), new Vector3(0,1,1));
		AddLine(new Vector3(0,1,1), new Vector3(1,1,0));
		AddLine(new Vector3(1,1,0), new Vector3(0,0,0));
		
		// Let's also build a mesh out of those points!
		mesh = new Mesh();
		mesh.vertices = vertices.points3.ToArray();
		mesh.triangles = new int[] { 0, 1, 2 };
		mesh.uv = new Vector2[] { Vector2.zero, Vector2.up, Vector2.right };
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		GetComponent<MeshFilter>().sharedMesh = mesh;
		
		var handles = GetComponentsInChildren<VertexHandle>();
		for (int i=0; i<3; i++) {
			handles[i].owner = this;
			handles[i].vertexNum = i;
			handles[i].transform.localPosition = vertices.points3[i];
		}
		
		RebuildFaceCollider();
	}
	
	public void RebuildFaceCollider() {
		// Hmm.  Collider is actually kinda tricky.  A non-convex mesh does not do it.
		// Providing a second triangle with the same vertices in (opposite winding order)
		// does not work, either.  We have to offset them a bit.
		GetComponent<MeshCollider>().sharedMesh = MakeCollisionMesh(vertices.points3[0], vertices.points3[1], vertices.points3[2]);
	}
	
	/// <summary>
	/// Make a convex collision mesh to go around the given triangle.
	/// </summary>
	Mesh MakeCollisionMesh(Vector3 p0, Vector3 p1, Vector3 p2) {
		Vector3 n = Vector3.Cross(p1-p0, p2-p0).normalized * 0.01f;
		var m = new Mesh();
		m.vertices = new Vector3[] { p0+n, p1+n, p2+n, p0-n, p1-n, p2-n };
		m.triangles = new int[] { 0, 1, 2,  3, 5, 4 };
		m.RecalculateBounds();
		m.RecalculateNormals();
		return m;
	}
	
	void AddLine(Vector3 p0, Vector3 p1) {
		if (multiline == null) {
			List<Vector3> points = new List<Vector3>() {p0, p1};
			multiline = new VectorLine("Multiline", points, 2.5f, LineType.Discrete);
			multiline.color = Color.green;
			multiline.drawTransform = transform;
			multiline.texture = lineTexture;
			multiline.Draw3DAuto();
		} else {
			multiline.points3.Add(p0);
			multiline.points3.Add(p1);
		}
		
		if (vertices == null) {
			List<Vector3> points = new List<Vector3>() { p1 };
			vertices = new VectorLine("Vertices", points, 6, LineType.Points);
			vertices.color = Color.gray;
			vertices.drawTransform = transform;
			vertices.texture = pointTexture;
			vertices.Draw3DAuto();
		} else {
			vertices.points3.Add(p1);
		}
	}

	public void MoveVertexTo(int vertNum, Vector3 localPos) {
		multiline.points3[(vertNum*2+1)%6] = multiline.points3[(vertNum*2+2)%6] = localPos;
		vertices.points3[vertNum] = localPos;
		
		var verts = mesh.vertices;
		verts[vertNum] = localPos;
		mesh.vertices = verts;
	}
}
