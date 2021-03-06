/*
This tool allows you to grab and drag individual vertices in a mesh.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexTweakTool : Tool
{
	public Transform endPoint;
	public LayerMask tweakableMask;

	public bool forceApply;
	
	bool isDragging = false;
	
	bool wasDown = false;
	
	Collider[] tempColliders = new Collider[32];
	MeshModel dragMesh;
	int dragIndex;
	Vector3 dragStartToolPos;
	Vector3 dragStartVPos;
	Vector3 lastToolWorldPos;

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
			lastToolWorldPos = endPoint.position;
			dragStartToolPos = dragMesh.transform.InverseTransformPoint(endPoint.position);
			dragStartVPos = dragMesh.Vertex(dragIndex);
			audio.Play();
			Debug.Log($"dragging {dragIndex} of {dragMesh}, starting at {dragStartVPos}");
		}
		
	}
	
	void Drag() {
		Vector3 toolPos = dragMesh.transform.InverseTransformPoint(endPoint.position);
		Vector3 delta = toolPos - dragStartToolPos;
		Vector3 newVPos = dragStartVPos + delta;
		
		dragMesh.ShiftVertexTo(dragIndex, newVPos);
		if (audio != null) {
			float moveDist = Vector3.Distance(lastToolWorldPos, endPoint.position);
			audio.volume = Mathf.MoveTowards(audio.volume, Mathf.Clamp01(moveDist/Time.deltaTime), 5*Time.deltaTime);
		}
		lastToolWorldPos = endPoint.position;
	}
	
	void EndDrag() {
		Debug.Log($"dragged {dragIndex} of {dragMesh} to {dragMesh.Vertex(dragIndex)}");

		audio.Stop();
		isDragging = false;
	}
}
