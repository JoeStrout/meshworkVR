// Exports a mesh (or many meshes) to OBJ format.
// Based originally on http://wiki.unity3d.com/index.php?title=ObjExporter
// ...but substantially corrected and improved.

using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
 
public class ObjExporter {
 
	public static string MeshToString(MeshFilter mf, bool useWorldCoords=true) {
		return MeshToString(mf.sharedMesh, mf.GetComponent<Renderer>());
	}
	
	public static string MeshToString(Mesh m, Renderer rend, bool useWorldCoords=true) {
		Material[] mats = null;
		if (rend != null) mats = rend.sharedMaterials;
		
		Transform t = rend.transform;
		
		StringBuilder sb = new StringBuilder();
		
		// We must be careful with the coordinate system: OBJ appears to assume
		// a different one than Unity uses.  We need to invert X, and then since
		// that changes the triangle winding, flip the triangles as well.
		
		sb.Append("g ").Append(m.name).Append("\n");
		foreach (Vector3 viter in m.vertices) {
			Vector3 v = viter;
			if (useWorldCoords) v = t.TransformPoint(v); 
			sb.Append(string.Format("v {0} {1} {2}\n", -v.x,v.y,v.z));
		}
		sb.Append("\n");
		foreach (Vector3 viter in m.normals) {
			Vector3 v = viter;
			if (useWorldCoords) v = t.TransformDirection(v);
			sb.Append(string.Format("vn {0} {1} {2}\n", -v.x,v.y,v.z));
		}
		sb.Append("\n");
		foreach (Vector3 v in m.uv) {
			sb.Append(string.Format("vt {0} {1}\n",v.x,v.y));
		}
		int vcount = m.vertices.Length;
		for (int material=0; material < m.subMeshCount; material ++) {
			sb.Append("\n");
			if (mats != null && mats[material] != null) {
				sb.Append("usemtl ").Append(mats[material].name).Append("\n");
				sb.Append("usemap ").Append(mats[material].name).Append("\n");
			}
			
			int[] triangles = m.GetTriangles(material);
			// We'll use negative vertex indices, which count backwards from the last
			// vertex added, so that we can have several models (groups) in the same file
			// and not have to offset the indices.  Also this lets users cut files apart
			// or paste them together without having to renumber all the vertices too.
			for (int i=0;i<triangles.Length;i+=3) {
				sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", 
					triangles[i] - vcount, triangles[i+2] - vcount, triangles[i+1] - vcount));
			}
		}
		return sb.ToString();
	}

 
	public static void MeshToFile(MeshFilter mf, string filename) {
		using (StreamWriter sw = new StreamWriter(filename)) {
			sw.Write(MeshToString(mf, false));
		}
	}
	
	public static void MeshToFile(Mesh m, Renderer rend, string filename) {
		using (StreamWriter sw = new StreamWriter(filename)) {
			sw.Write(MeshToString(m, rend, false));
		}
	}
	
	public static void ExportToFile(Transform[] rootTransforms, string filename) {
		using (StreamWriter sw = new StreamWriter(filename)) {
			foreach (Transform root in rootTransforms) {
				foreach (MeshRenderer mr in root.GetComponentsInChildren<MeshRenderer>()) {
					MeshFilter mf = mr.GetComponent<MeshFilter>();
					if (mf != null) sw.Write(MeshToString(mf.sharedMesh, mr, true));
				}
			}
		}
	}
}