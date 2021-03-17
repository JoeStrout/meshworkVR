using UnityEngine;
using UnityEngine.Events;
using Unity.Collections;
using System.Collections.Generic;

public static class MeshUtils {
	
	// Class to represent info about overlapping vertices in a mesh.
	// Handy for welding them together, or making sure you move them all together, etc.
	public class OverlapSet {
		public List<int> indexes;
		public Vector3 meanNormal;
		
		public OverlapSet(int firstIndex, Vector3 firstNormal) {
			indexes = new List<int>(4);
			indexes.Add(firstIndex);
			meanNormal = firstNormal;
		}
	}
	
	public static bool RayTriangleIntersect(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, out Vector3 hitPoint) {
		float t = 0;
		return RayTriangleIntersect(ray, v0, v1, v2, out hitPoint, out t);
	}
	
	/// <summary>
	/// Clone this mesh, returning an independent copy.
	/// </summary>
	public static Mesh Clone(this Mesh mesh, bool makeReadable=false) {
		if (makeReadable == mesh.isReadable) {
			// OK, turns out this is really easy:
			return Object.Instantiate(mesh);
		} else {
			Mesh newmesh = new Mesh();
			newmesh.vertices = mesh.vertices;
			newmesh.triangles = mesh.triangles;
			newmesh.uv = mesh.uv;
			newmesh.normals = mesh.normals;
			newmesh.colors = mesh.colors;
			newmesh.tangents = mesh.tangents;
			newmesh.boneWeights = mesh.boneWeights;
			newmesh.bindposes = mesh.bindposes;
			if (mesh.blendShapeCount == 0) return newmesh;
			Vector3[] deltaVerts = new Vector3[mesh.vertexCount];
			Vector3[] deltaNorms = new Vector3[mesh.vertexCount];
			Vector3[] deltaTangents = new Vector3[mesh.vertexCount];			
			for (int i=0; i<mesh.blendShapeCount; i++) {
				string name = mesh.GetBlendShapeName(i);
				for (int f=0; f<mesh.GetBlendShapeFrameCount(i); f++) {
					mesh.GetBlendShapeFrameVertices(i, f, deltaVerts, deltaNorms, deltaTangents);
					newmesh.AddBlendShapeFrame(name, mesh.GetBlendShapeFrameWeight(i,f), 
						deltaVerts, deltaNorms, deltaTangents);
				}
			}
			return newmesh;
		}
	}
	
	/// <summary>
	/// Linearly interpolate between two meshes that have the same topology, but
	/// may differ in vertex positions and weights.
	/// </summary>
	/// <returns>a new Mesh that is the interpolation of meshA and meshB</returns>
	public static Mesh Lerp(Mesh meshA, Mesh meshB, float t) {
		if (meshA.vertexCount != meshB.vertexCount) {
			Debug.LogError("Mesh.Lerp: meshes do not match (" + meshA.name + " has " + meshA.vertexCount
				+ " vertices, but " + meshB.name + " has " + meshB.vertexCount);
			return null;
		}
		
		// Start by cloning mesh A.  Note that we take the bind poses from mesh A,
		// always, even when interpolating all the way to mesh B... we assume that
		// all we really want to interpolate is vertex positions, normals, and tangents.
		Mesh result = meshA.Clone(true);
		if (t == 0) return result;
		if (t == 1) {
			result.vertices = meshB.vertices;
			result.normals = meshB.normals;
			result.tangents = meshB.tangents;
			result.RecalculateBounds();
			return result;
		}
		
		// Interpolate vertices and normals
		Vector3[] verticesA = meshA.vertices;
		Vector3[] verticesB = meshB.vertices;
		Vector3[] vertices = result.vertices;
		Debug.Assert(vertices.Length == verticesB.Length);
		for (int i=0; i<vertices.Length; i++) {
			vertices[i] = Vector3.LerpUnclamped(verticesA[i], verticesB[i], t);
		}
		result.vertices = vertices;
		
		Vector3[] normalsA = meshA.normals;
		Vector3[] normalsB = meshB.normals;
		Vector3[] normals = result.normals;
		for (int i=0; i<normals.Length; i++) {
			normals[i] = Vector3.LerpUnclamped(normalsA[i], normalsB[i], t);
			normals[i].Normalize();
		}
		result.normals = normals;
				
		// Interpolate vertex bone weights
		var bonesPerVertexA = meshA.GetBonesPerVertex();
		var boneWeightsA = meshA.GetAllBoneWeights();
		var bonesPerVertexB = meshB.GetBonesPerVertex();
		var boneWeightsB = meshB.GetAllBoneWeights();
		var boneWeights = new List<BoneWeight1>();
		byte[] bonesPerVertex = new byte[vertices.Length];
		int idxA = 0, idxB = 0;
		BoneWeight1[] tempWeights = new BoneWeight1[8];
		for (int i=0; i<vertices.Length; i++) {
			//if (i == 9) Debug.Log("Inspecting vertex 9: weightsA=" + bonesPerVertexA[i] + ", weightsB=" + bonesPerVertexB[i]);
			// First check for the easy case: same bones used for this vertex on both meshes.
			bool match = (bonesPerVertexA[i] == bonesPerVertexB[i]);
			for (int j=0; match && j < bonesPerVertexA[i]; j++) {
				if (boneWeightsA[idxA+j].boneIndex != boneWeightsB[idxB+j].boneIndex) match = false;
			}
			if (match) {
				//if (i == 9) Debug.Log("Hooray, easy case");
				// Hooray, easy case!  Same bones used on both.  Loop again and interpolate weight.
				for (int j=0; j < bonesPerVertexA[i]; j++) {
					BoneWeight1 resultBW = new BoneWeight1();
					resultBW.boneIndex = boneWeightsA[idxA].boneIndex;
					resultBW.weight = Mathf.LerpUnclamped(boneWeightsA[idxA++].weight, boneWeightsB[idxB++].weight, t);
					boneWeights.Add(resultBW);
				}	
				bonesPerVertex[i] = bonesPerVertexA[i];
			} else {
				//if (i == 9) Debug.Log("Boo, hard case");
				// Boo, easy case does not apply.  This means the two source meshes have a different
				// number or order of bone weights.  We need to find the top few matches after
				// interpolating, and then store them in the result in sorted order.
				for (int j=0; j<8; j++) tempWeights[j].weight = 0;	// clear all temp weights
				int tempWeightCount = 0;
				// First loop over weights A, interpolating to the weights in mesh B.
				for (int j=0; j < bonesPerVertexA[i]; j++) {
					int boneIdx = boneWeightsA[idxA+j].boneIndex;
					float wA = boneWeightsA[idxA+j].weight;
					float wB = FindBoneWeight(boneWeightsB, idxB, bonesPerVertexB[i], boneIdx);
					tempWeights[tempWeightCount].boneIndex = boneIdx;
					tempWeights[tempWeightCount++].weight = Mathf.LerpUnclamped(wA, wB, t);
				}
				// Then loop over weights B, interpolating any that we haven't already handled.
				for (int j=0; j < bonesPerVertexB[i]; j++) {
					int boneIdx = boneWeightsB[idxB+j].boneIndex;
					float wA = FindBoneWeight(boneWeightsA, idxA, bonesPerVertexA[i], boneIdx);
					if (wA == 0) {
						tempWeights[tempWeightCount].boneIndex = boneIdx;
						tempWeights[tempWeightCount++].weight = Mathf.LerpUnclamped(0, boneWeightsB[idxB+j].weight, t);
					}
				}
				// Now we need to sort our temporary results by weight, in descending order, and copy to the output.
				System.Array.Sort(tempWeights, (a,b) => -a.weight.CompareTo(b.weight));
				//if (i == 9) {
				//	Debug.Log("After combining, we have "+ tempWeightCount + " weights");
				//	for (int j=0; j<tempWeightCount; j++) Debug.Log("#" + tempWeights[j].weight + " on bone " + tempWeights[j].boneIndex);
				//}
				if (tempWeightCount > 4) tempWeightCount = 4;	// use no more than 4 output weights
				for (int j=0; j<tempWeightCount; j++) {
					boneWeights.Add(tempWeights[j]);
				}
				bonesPerVertex[i] = (byte)tempWeightCount;
				idxA += bonesPerVertexA[i];
				idxB += bonesPerVertexB[i];
			}
		}
		result.SetBoneWeights(
			new NativeArray<byte>(bonesPerVertex, Allocator.Temp),
			new NativeArray<BoneWeight1>(boneWeights.ToArray(), Allocator.Temp));
		
		return result;		
	}
	
	/// <summary>
	/// Search the given range of the given bone weight array, looking for a bone
	/// index equal to boneToFind.  If found, return the corresponding weight;
	/// otherwise return 0.
	/// </summary>
	static float FindBoneWeight(NativeArray<BoneWeight1> boneWeights, int startIdx, int count, int boneToFind) {
		for (int i=0; i<count; i++) {
			if (boneWeights[startIdx+i].boneIndex == boneToFind) return boneWeights[startIdx+i].weight;
		}
		return 0;
	}

	/// <summary>
	/// Search the given range of the given bone weight array, looking for a bone
	/// index equal to boneToFind.  If found, return the corresponding weight;
	/// otherwise return 0.
	/// </summary>
	static float FindBoneWeight(BoneWeight1[] boneWeights, int startIdx, int count, int boneToFind) {
		for (int i=0; i<count; i++) {
			if (boneWeights[startIdx+i].boneIndex == boneToFind) return boneWeights[startIdx+i].weight;
		}
		return 0;
	}


	public static bool RayTriangleIntersect(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, out Vector3 hitPoint, out float t) {
		// Based on: http://gamedev.stackexchange.com/questions/114955/m%c3%b6ller-trumbore-intersection-point
		hitPoint = Vector3.zero;
		t = 0;
		
		float kEpsilon = 0.000001f;
		Vector3 v0v1 = v1 - v0;
		Vector3 v0v2 = v2 - v0;
		Vector3 pvec = Vector3.Cross(ray.direction, v0v2);
		
		float det = Vector3.Dot(v0v1, pvec);
		
		if (det < kEpsilon) return false;	// Note: we check only one direction, to cull backfaces.
		
		float invDet = 1f / det;
		
		Vector3 tvec = ray.origin - v0;
		float u = Vector3.Dot(tvec, pvec) * invDet;
		if (u < 0 || u > 1) return false;
		
		Vector3 qvec = Vector3.Cross(tvec, v0v1);
		float v = Vector3.Dot(ray.direction, qvec) * invDet;
		if (v < 0 || u + v > 1) return false;
		
		// OK, at this point we know we have a hit.
		// We'll compute t and from that, compute the hit point.
		// But note that u and v are really useful too, particularly for interpolating
		// other attributes of the points (UV coordinates, normals, etc.).  See:
		// http://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/moller-trumbore-ray-triangle-intersection
		
		t = Vector3.Dot(v0v2, qvec) * invDet;
		hitPoint = ray.origin + ray.direction * t;
		return true;
	}
	
	/// <summary>
	/// Find where a UV point of interest falls on a UV map mesh.
	/// </summary>
	/// <param name="poi">UV point of interest</param>
	/// <param name="uvs">uv array of mesh</param>
	/// <param name="triangles">triangles of mesh</param>
	/// <param name="triBaseIndex">receives base index of triangle hit</param>
	/// <param name="st">receives S,T coordinates within that triangle</param>
	/// <returns>true if hit found; false if poi is not found in the UV map</returns>
	public static bool MeshPointOfUV(Vector2 poi, Vector2[] uvs, int[] triangles, out int triBaseIndex, out Vector2 st) {
		st = Vector2.zero;
		
		for (triBaseIndex = 0; triBaseIndex < triangles.Length; triBaseIndex += 3) {
			
			// Find the corners of our triangle.
			Vector2 a = uvs[triangles[triBaseIndex+0]];
			Vector2 b = uvs[triangles[triBaseIndex+1]];
			Vector2 c = uvs[triangles[triBaseIndex+2]];
			
			// Find the barycentric coordinates s,t of our point in that triangle
			// (http://gamedev.stackexchange.com/questions/23743)
			Vector2 v0 = b - a, v1 = c - a, v2 = poi - a;
			float d00 = Vector2.Dot(v0, v0);
			float d01 = Vector2.Dot(v0, v1);
			float d11 = Vector2.Dot(v1, v1);
			float d20 = Vector2.Dot(v2, v0);
			float d21 = Vector2.Dot(v2, v1);
			float denom = d00 * d11 - d01 * d01;
			float s = (d11 * d20 - d01 * d21) / denom;
			float t = (d00 * d21 - d01 * d20) / denom;
			
			// Range check -- if out of bounds, then go on to the next triangle.
			if (s < 0 || s > 1 || t < 0 || t > 1 || s+t > 1) continue;
			
			// Otherwise, we found it, and quite efficiently too!
			st.x = s;
			st.y = t;
			return true;
		}
		return false;
	}
	
	/// <summary>
	/// Find the closest (smallest dist > 0) intersection of the given ray with the given mesh.
	/// </summary>
	/// <param name="ray"></param>
	/// <param name="vertices"></param>
	/// <param name="triangles"></param>
	/// <param name="hitPoint"></param>
	/// <param name="dist"></param>
	/// <returns></returns>
	public static bool RayMeshIntersect(Ray ray, Vector3[] vertices, int[] triangles, 
				out Vector3 hitPoint, out float dist)
	{
		int whoCares;
		return MeshUtils.RayMeshIntersect(ray, vertices, triangles,
					out hitPoint, out dist, out whoCares);
	}
	
	/// <summary>
	/// Find the closest (smallest dist > 0) intersection of the given ray with the given mesh.
	/// </summary>
	/// <param name="ray"></param>
	/// <param name="vertices"></param>
	/// <param name="triangles"></param>
	/// <param name="hitPoint"></param>
	/// <param name="dist"></param>
	/// <param name="triBaseIndex">receives the index of the triangle hit in the triangles array</param>
	/// <returns></returns>
	public static bool RayMeshIntersect(Ray ray, Vector3[] vertices, int[] triangles, 
		out Vector3 hitPoint, out float dist, out int triBaseIndex)
	{
		hitPoint  = Vector3.zero;
		dist = -1;
		triBaseIndex = -1;
		int triCount = triangles.Length;
		for (int j=0; j<triCount; j+=3) {
			Vector3 pt;
			float d;
			if (MeshUtils.RayTriangleIntersect(ray, 
				vertices[triangles[j]], vertices[triangles[j+1]], vertices[triangles[j+2]],
				out pt, out d)
				&& d > 0 && (dist < 0 || d < dist)) {
				dist = d;
				hitPoint = pt;
				triBaseIndex = j;
				}
		}
		return dist >= 0;
	}
	
	/// <summary>
	/// Return the vertex of the given triangle which is closest to the given point.
	/// </summary>
	public static Vector3 NearestTriangleVertex(Vector3[] vertices, int[] triangles, int triBaseIndex, Vector3 point, out int vertIndex) {
		vertIndex = triangles[triBaseIndex];
		Vector3 bestPt = vertices[vertIndex];
		float bestDsqr = (point - bestPt).sqrMagnitude;
		for (int i=triBaseIndex+1; i<triBaseIndex+3; i++) {
			Vector3 pt = vertices[triangles[i]];
			float dsqr = (pt - point).sqrMagnitude;
			if (dsqr < bestDsqr) {
				bestPt = pt;
				bestDsqr = dsqr;
				vertIndex = triangles[i];
			}
		}
		return bestPt;
	}
	
	/// <summary>
	/// Return the vertex of the given triangle which is closest to the given point.
	/// </summary>
	public static Vector3 NearestTriangleVertex(Vector3[] vertices, int[] triangles, int triBaseIndex, Vector3 point) {
		int whoCares;
		return NearestTriangleVertex(vertices, triangles, triBaseIndex, point, out whoCares);
	}
		
		
	/// <summary>
	/// Find all the overlaps in the given set, and return a map that
	/// will let you quickly look up the overlap (if any) for any index
	/// in the original list.
	/// </summary>
	/// <returns>dictionary with key: vertex index; value: OverlapSet it belongs to</returns>
	public static Dictionary<int, OverlapSet> FindOverlaps(Vector3[] vertices, Vector3[] normals, float epsilon=0.01f) {
		var result = new Dictionary<int, OverlapSet>();
		float sqrEps = epsilon * epsilon;
		for (int i=0; i<vertices.Length; i++) {
			if (result.ContainsKey(i)) continue;	// already noted
			Vector3 v = vertices[i];
			for (int j=i+1; j<vertices.Length; j++) {
				if (Vector3.SqrMagnitude(vertices[j] - v) < sqrEps) {
					OverlapSet set;
					if (!result.TryGetValue(i, out set)) {
						set = new OverlapSet(i, normals[i]);
						result[i] = set;
					}
					set.indexes.Add(j);
					set.meanNormal += normals[j];
					result[j] = set;
				}
			}
		}
		foreach (OverlapSet set in result.Values) {
			set.meanNormal /= set.indexes.Count;
			set.meanNormal.Normalize();
		}
		return result;
	}
	
	public static void TransformPoints(this Transform t, Vector3[] points) {
		int count = points.Length;
		for (int i=0; i<count; i++) points[i] = t.TransformPoint(points[i]);
	}
	
	public static void InverseTransformPoints(this Transform t, Vector3[] points) {
		int count = points.Length;
		for (int i=0; i<count; i++) points[i] = t.InverseTransformPoint(points[i]);
	}
	
	public static void TransformDirections(this Transform t, Vector3[] points) {
		int count = points.Length;
		for (int i=0; i<count; i++) points[i] = t.TransformDirection(points[i]);
	}
	
	
}
