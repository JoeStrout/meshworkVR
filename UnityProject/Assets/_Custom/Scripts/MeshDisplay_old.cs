/*
Builds and maintains the visual display of a MeshModel.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

public class MeshDisplay_old : MonoBehaviour
{
	public Texture2D lineTexture;
	public Material lineMat;
	public Color lineColor = Color.white;
	public Texture2D pointTexture;

	
	MeshModel model;

	VectorLine multiline;	// Discrete type "line" that is actually all our line segments in one.
	VectorLine multipoint;	// Points type "line" that draws a bunch of points

	int[] vertToPointIdx;	// key: mesh vertex index; value: index in multipoint array

	// Define a fixed-point Vector3 struct, which can be compared for equality
	// We assume a minimum separation of 0.1 mm.
	public struct FixedVector3 {
		public int x, y, z;
		
		public FixedVector3(float x, float y, float z) {
			this.x = Mathf.RoundToInt(x * 10000);
			this.y = Mathf.RoundToInt(y * 10000);
			this.z = Mathf.RoundToInt(z * 10000);
		}
		public FixedVector3(Vector3 v) {
			this.x = Mathf.RoundToInt(v.x * 10000);
			this.y = Mathf.RoundToInt(v.y * 10000);
			this.z = Mathf.RoundToInt(v.z * 10000);
		}

		public Vector3 ToVector3() {
			return new Vector3(x * 0.0001f, y * 0.0001f, z * 0.0001f);
		}

		public bool Equals(FixedVector3 p)
		{
			if (Object.ReferenceEquals(p, null)) return false;
			if (Object.ReferenceEquals(this, p)) return true;
			return x == p.x && y == p.y && z == p.z;
		}

		public override int GetHashCode() {
			return x * 0x00100000 + y * 0x00000100 + z;
		}

		public static bool operator ==(FixedVector3 lhs, FixedVector3 rhs)
		{
			// Check for null on left side.
			if (Object.ReferenceEquals(lhs, null)) 	{
				if (Object.ReferenceEquals(rhs, null)) return true;
				return false;
			}
			// Equals handles case of null on right side.
			return lhs.Equals(rhs);
		}

		public static bool operator !=(FixedVector3 lhs, FixedVector3 rhs)
		{
			return !(lhs == rhs);
		}
	}

	protected void Start() {
		model = GetComponent<MeshModel>();

		List<Vector3> points = new List<Vector3>();
		multipoint = new VectorLine("Vertices", points, 6, LineType.Points);
		multipoint.color = Color.gray;
		multipoint.drawTransform = transform;
		multipoint.texture = pointTexture;
		//multipoint.Draw3DAuto();
		
		// Gather points, combining vertices at the same location into one.
		vertToPointIdx = new int[model.vertexCount];
		var posToPointIdx = new Dictionary<FixedVector3, int>();
		for (int i=0; i<model.vertexCount; i++) {
			var fv = new FixedVector3(model.Vertex(i));
			int pointIdx;
			if (posToPointIdx.TryGetValue(fv, out pointIdx)) {
				vertToPointIdx[i] = pointIdx;
			} else {
				posToPointIdx[fv] = vertToPointIdx[i] = multipoint.points3.Count;
				multipoint.points3.Add(model.Vertex(i));				
			}
		}
		Debug.Log($"{gameObject.name}: found {multipoint.points3.Count} unique points");
		
		// Now load edges, using the reduced set of points to avoid creating redundant lines.
		var alreadyMade = new HashSet<int>();
		for (int i=0; i<model.edgeCount; i++) {
			var edge = model.Edge(i);
			int idx0 = vertToPointIdx[edge.index0];
			int idx1 = vertToPointIdx[edge.index1];
			if (idx0 > idx1) {
				int temp = idx0; idx0 = idx1; idx1 = temp;
			}
			int key = (idx0 << 16) + idx1;
			if (!alreadyMade.Contains(key)) {
				AddLine(model.Vertex(edge.index0), model.Vertex(edge.index1));
				alreadyMade.Add(key);
			}
		}
		Debug.Log($"{gameObject.name}.MeshDisplay: loaded {alreadyMade.Count} edges");
	}
	
	void AddLine(Vector3 p0, Vector3 p1) {
		if (multiline == null) {
			List<Vector3> points = new List<Vector3>() {p0, p1};
			multiline = new VectorLine("MeshDisplay edges", points, 2.5f, LineType.Discrete);
			multiline.color = lineColor;
			multiline.material = lineMat;
			multiline.drawTransform = transform;
			multiline.texture = lineTexture;
			multiline.Draw3DAuto();
		} else {
			multiline.points3.Add(p0);
			multiline.points3.Add(p1);
		}

	}

	public void ShiftVertexTo(Vector3 oldPos, Vector3 newPos) {
		for (int i=0; i<multiline.points3.Count; i++) {
			if (multiline.points3[i] == oldPos) multiline.points3[i] = newPos;
		}
		for (int i=0; i<multipoint.points3.Count; i++) {
			if (multipoint.points3[i] == oldPos) multipoint.points3[i] = newPos;
		}
	}
}
