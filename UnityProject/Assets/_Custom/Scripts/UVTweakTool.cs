/*
This tool allows you to "tweak" the UV mapping by clicking and dragging
any corner in the mesh to adjust its UV coordinates.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVTweakTool : Tool
{
	public Transform endPoint;
	public LayerMask tweakableMask;

	public bool forceApply;
	
	AudioSource audio;
	
	HandTracker handTracker;
	bool isDragging = false;
	
	bool wasDown = false;
	
	Collider[] tempColliders = new Collider[32];
	MeshModel dragMesh;
	int dragIndex;
	Vector3 dragPlaneUp;
	Vector3 dragPlaneRight;
	Vector2 dragStartPos;
	Vector2 dragStartUV;

	protected void Awake() {
		handTracker = transform.parent.GetComponentInChildren<Grabber>().handTracker;
		Debug.Assert(handTracker != null);		
		
		audio = GetComponent<AudioSource>();
	}
	
	protected void Update() {
		bool isDown = forceApply || (handTracker.trigger > (wasDown ? 0.4f : 0.6f));
		if (isDown && !wasDown) BeginDrag();
		else if (wasDown && !isDown) EndDrag();
		else if (isDragging) Drag();
		wasDown = isDown;
	}
	
	void BeginDrag() {
		// Find a mesh corner near the vertex.
		float radius = 0.1f;
		int count = Physics.OverlapSphereNonAlloc(endPoint.position, radius, tempColliders,
			tweakableMask, QueryTriggerInteraction.Collide);
		
		dragMesh = null;
		dragIndex = 0;
		float bestDist = radius;
		for (int i=0; i<count; i++) {
			var mesh = tempColliders[i].GetComponentInParent<MeshModel>();
			if (mesh == null) continue;
			int idx;
			if (!mesh.FindIndex(endPoint.position, transform.position, bestDist, out idx)) continue;
			dragIndex = idx;
			dragMesh = mesh;
			bestDist = Vector3.Distance(mesh.Vertex(idx), endPoint.position);
		}
			
		if (dragMesh != null) {		
			isDragging = true;
			dragPlaneUp = transform.up;
			dragPlaneRight = transform.right;
			dragStartPos = MathUtils.ProjectTo2D(dragPlaneUp, dragPlaneRight, endPoint.position);
			dragStartUV = dragMesh.UV(dragIndex);
			audio.Play();
		}
		
	}
	
	void Drag() {
		Vector2 newPos = MathUtils.ProjectTo2D(dragPlaneUp, dragPlaneRight, endPoint.position);
		Debug.Log($"Tool moved from {dragStartPos} to {newPos} in drag plane");
		Vector2 drag = newPos - dragStartPos;
		Vector2 newUV = dragStartUV + drag;	// <-- here is where we are making assumptions about UV axis direction!
		Vector2 uvDelta = newUV - dragMesh.UV(dragIndex);
				
		dragMesh.ShiftUV(dragIndex, uvDelta);
		audio.volume = Mathf.MoveTowards(audio.volume, Mathf.Clamp01(uvDelta.magnitude/Time.deltaTime), 5*Time.deltaTime);
	}
	
	void EndDrag() {
		audio.Stop();
		isDragging = false;
	}
}
