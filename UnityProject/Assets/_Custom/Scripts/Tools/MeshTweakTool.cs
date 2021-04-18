/*
This tool allows you to grab and drag parts of a mesh around: either vertices,
or edges, or faces, depending on the current editing mode.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTweakTool : Tool
{
	public enum Mode {
		Vertex,
		Edge,
		Face
	}
	public Transform endPoint;
	public LayerMask tweakableMask;
	public FormatText label;
	public FormatText infoTitle;
	public FormatText infoText;
	
	public Mode mode = Mode.Vertex;

	public bool forceApply;
	
	bool isDragging = false;
	
	bool wasDown = false;
	
	Collider[] tempColliders = new Collider[32];
	MeshModel dragMesh;			// mesh we are currently tweaking
	int dragIndex;				// index of the vertex, edge, or face which we are tweaking
	Vector3 dragStartToolPos;	// local (to dragMesh) position of the tool at the start of the drag
	Vector3 dragStartVPos;		// local position of the grabbed vertex at the start of the drag
	Vector3 lastToolWorldPos;	// world position of the tool on last update

	bool hadCenter;	// flag indicating we had the thumb stick centered since we last did anything with it

	Dictionary<int, Vector3> toolRelativePositions;

	protected override void Awake() {
		base.Awake();
		SetMode(mode);
		hadCenter = false;
		toolRelativePositions = new Dictionary<int, Vector3>();
	}

	protected void Update() {
		base.Update();
		
		float stickX = handTracker.thumbStick.x;
		if (stickX > -0.3 && stickX < 0.3) hadCenter = true;
		else if (stickX < -0.7f && hadCenter) {
			ShiftMode(-1);
			hadCenter = false;
		} else if (stickX > 0.7f && hadCenter) {
			ShiftMode(1);
			hadCenter = false;
		}
		
		bool isDown = forceApply || (handTracker.trigger > (wasDown ? 0.4f : 0.6f));
		if (isDown && !wasDown) BeginDrag();
		else if (wasDown && !isDown) EndDrag();
		else if (isDragging) Drag();
		wasDown = isDown;
	}
	
	void SetMode(Mode newMode) {
		mode = newMode;
		string modeStr = mode.ToString().ToLower();
		label.SetString(modeStr);
		infoTitle.SetString(modeStr);
		infoText.SetString(modeStr);
	}
	
	void ShiftMode(int delta) {
		int modeNum = (int)mode;
		int count = System.Enum.GetValues(typeof(Mode)).Length;
		SetMode((Mode)((modeNum + delta + count) % count));
	}
	
	void BeginDrag() {
		// Find a mesh corner near the vertex.
		float radius = 0.1f;
		int count = Physics.OverlapSphereNonAlloc(endPoint.position, radius, tempColliders,
			tweakableMask, QueryTriggerInteraction.Collide);
		
		dragMesh = null;
		dragIndex = -1;
		float bestDist = radius;
		for (int i=0; i<count; i++) {
			var mesh = tempColliders[i].GetComponentInParent<MeshModel>();
			if (mesh == null) continue;
			int idx = -1;
			float dist = Mathf.Infinity;
			switch (mode) {
			case Mode.Vertex:
				if (!mesh.FindVertexIndex(endPoint.position, transform.position, bestDist, out idx, out dist)) continue;
				break;
			case Mode.Face:
				if (!mesh.FindFace(endPoint.position, transform.position, bestDist, out idx, out dist)) continue;
				toolRelativePositions.Clear();
				mesh.FindFaceVertices(idx, toolRelativePositions, transform);
				break;
			}
			dragIndex = idx;
			dragMesh = mesh;
			bestDist = dist;
		}
			
		if (dragMesh != null && dragIndex >= 0) {		
			isDragging = true;
			lastToolWorldPos = endPoint.position;
			dragStartToolPos = dragMesh.transform.InverseTransformPoint(endPoint.position);
			dragStartVPos = dragMesh.Vertex(dragIndex);
			if (audio != null) audio.Play();
		}
		
	}
	
	void Drag() {
		Vector3 toolPos = dragMesh.transform.InverseTransformPoint(endPoint.position);
		if (mode == Mode.Vertex) {
			Vector3 delta = toolPos - dragStartToolPos;
			Vector3 newVPos = dragStartVPos + delta;			
			dragMesh.ShiftVertexTo(dragIndex, newVPos);
		} else if (mode == Mode.Face) {
			int i = 1;
			foreach (var kv in toolRelativePositions) {
				int vidx = kv.Key;
				Vector3 v = dragMesh.transform.InverseTransformPoint(transform.TransformPoint(kv.Value));
				dragMesh.ShiftVertexTo(vidx, v, i == toolRelativePositions.Count);
				i++;
			}
		}
		
		if (audio != null) {
			float moveDist = Vector3.Distance(lastToolWorldPos, endPoint.position);
			audio.volume = Mathf.MoveTowards(audio.volume, Mathf.Clamp01(moveDist/Time.deltaTime), 5*Time.deltaTime);
		}
		lastToolWorldPos = endPoint.position;
	}
	
	void EndDrag() {
		if (audio != null) audio.Stop();
		isDragging = false;
	}
}
