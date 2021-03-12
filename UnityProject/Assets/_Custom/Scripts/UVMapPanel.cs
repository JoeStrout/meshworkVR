/*
This component manages a UV map panel in a canvas.  Its job is to display the
texture and UV map (edges and vertices) for a particular mesh.

This script should be placed on a RawImage.  To make it paintable, we also need
a Quad positioned in the same place.  The Quad renderer can be turned off; the
display will actually be done via the RawImage, so it layers properly with other
UI elements.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PaintIn3D;

public class UVMapPanel : MonoBehaviour
{
	[Tooltip("Mesh whose texture we display")]
	public MeshRenderer meshRenderer;
	public MeshModel meshModel;
	
	public RectTransform edgePrototype;
	public MeshRenderer quad;
	
	RawImage image;
	
	Dictionary<int, RectTransform> edgeMap;	// key: EdgeKey; value: UI element displaying that edge
	
	protected void Awake() {
		image = GetComponentInChildren<RawImage>();
		Debug.Assert(image != null, "RawImage component not found in " + gameObject.name, gameObject);
	}
	
	protected void Start() {
		edgePrototype.gameObject.SetActive(false);
		quad.sharedMaterial = meshRenderer.sharedMaterial;
		Invoke("LateStart", 0.01f);
	}
	
	protected void OnEnable() {
		meshModel.onUVChanged.AddListener(NoteUVChanged);
	}
	
	protected void OnDisable() {
		meshModel.onUVChanged.RemoveListener(NoteUVChanged);
	}
	
	void LateStart() {
		BuildDisplay();
		foreach (var pt in meshRenderer.GetComponent<P3dPaintable>().PaintableTextures) {
			quad.GetComponent<P3dPaintable>().PaintableTextures.Add(pt);
		}
	}
	
	protected void Update() {
		// Ensure we're displaying the correct texture, and quad is referencing the correct material
		if (meshRenderer.sharedMaterial == null) return;
		if (image.texture != meshRenderer.sharedMaterial.mainTexture) {
			image.texture = meshRenderer.sharedMaterial.mainTexture;
		}
		quad.sharedMaterial = meshRenderer.sharedMaterial;
	}
	
	/// <summary>
	/// Return a unique key for the given pair of vertices (regardless of order).
	/// </summary>
	/// <param name="vertIndex0"></param>
	/// <param name="vertIndex1"></param>
	/// <returns></returns>
	int EdgeKey(MeshModel.MeshEdge edge) {
		int key = (edge.index0 < edge.index1 ? edge.index0 << 16 + edge.index1 : edge.index1 << 16 + edge.index0);
		return key;
	}
	
	void ClearDisplay() {
		if (edgeMap == null) return;
		foreach (RectTransform e in edgeMap.Values) {
			Destroy(e.gameObject);
		}
		edgeMap.Clear();
	}
	
	/// <summary>
	/// Rebuild our display of vertices and lines from the mesh in UV space.
	/// </summary>
	void BuildDisplay() {
		if (edgeMap == null) edgeMap = new Dictionary<int, RectTransform>();
		else edgeMap.Clear();
		for (int i=0; i<meshModel.edgeCount; i++) {
			var edge = meshModel.Edge(i);			
			int key = EdgeKey(edge);
			if (edgeMap.ContainsKey(key)) continue;
			
			var line = Instantiate(edgePrototype, edgePrototype.parent);
			ArrangeEdgeLine(edge, line);
			line.gameObject.SetActive(true);
			edgeMap[key] = line;
		}
	}
	
	void ArrangeEdgeLine(MeshModel.MeshEdge edge, RectTransform line) {
		Vector2 containerSize = (edgePrototype.transform.parent as RectTransform).sizeDelta;
		var uv0 = meshModel.UV(edge.index0);
		var uv1 = meshModel.UV(edge.index1);
		float uvDist = Vector2.Distance(uv0, uv1);
		line.anchoredPosition = new Vector2(uv0.x * containerSize.x, uv0.y * containerSize.y);
		line.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(uv1.y - uv0.y, uv1.x - uv0.x) * Mathf.Rad2Deg);
		line.sizeDelta = new Vector2(uvDist * containerSize.x, line.sizeDelta.y);
		line.gameObject.name = $"Edge from {uv0} to {uv1}";
	}
	
	void NoteUVChanged(int vertexNum) {
		for (int i=0; i<meshModel.edgeCount; i++) {
			var edge = meshModel.Edge(i);			
			if (edge.index0 != vertexNum && edge.index1 != vertexNum) continue;
			var line = edgeMap[EdgeKey(edge)];
			ArrangeEdgeLine(edge, line);
		}
	}
}
