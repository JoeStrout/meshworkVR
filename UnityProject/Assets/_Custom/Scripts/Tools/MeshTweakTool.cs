﻿/*
This tool allows you to grab and drag parts of a mesh around: either vertices,
or edges, or faces, depending on the current editing mode.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
	public AudioSource extrudeSound;
	
	public Mode mode = Mode.Vertex;
	
	bool isDragging = false;
	
	bool grabWasDown = false;
	
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
				
		bool isDown = (handTracker.trigger > (grabWasDown ? 0.4f : 0.6f));
		if (isDown && !grabWasDown) BeginDrag();
		else if (grabWasDown && !isDown) EndDrag();
		else if (isDragging) Drag();
		grabWasDown = isDown;
		
		float stickX = handTracker.thumbStick.x;
		if (isDragging) {
			// Additional functions you can do while dragging:
			
			// Extrude (button X)
			if (handTracker.GetButtonDown(HandTracker.Button.X)) {
				extrudeSound.Play();
				dragMesh.DoExtrude();
			}
			
			// Scale (thumb stick)
			float stickY = handTracker.thumbStick.y;
			float scaleInput = (Mathf.Abs(stickX) > Mathf.Abs(stickY) ? stickX : stickY);
			if (scaleInput > 0.1f) ScaleSelection(Mathf.InverseLerp(0.1f, 1f, scaleInput));
			else if (scaleInput < -0.1f) ScaleSelection(-Mathf.InverseLerp(-0.1f, -1f, scaleInput));
		} else {
			// While not dragging: check stick to change mode.
			if (stickX > -0.3 && stickX < 0.3) hadCenter = true;
			else if (stickX < -0.7f && hadCenter) {
				ShiftMode(-1);
				hadCenter = false;
			} else if (stickX > 0.7f && hadCenter) {
				ShiftMode(1);
				hadCenter = false;
			}
		}
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
				GrabFaces(mesh, idx);
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
	
	/// <summary>
	/// Grab the set of faces that should be dragged along with the given (hit) triangle.
	/// </summary>
	/// <param name="mesh"></param>
	/// <param name="triangleIndex"></param>
	void GrabFaces(MeshModel mesh, int triangleIndex) {
		toolRelativePositions.Clear();

		// First, check whether the hit triangle is selected.  If so, then we just
		// need to grab all the vertices in the selection.
		var disp = mesh.GetComponent<MeshDisplay>();
		if (disp.IsSelected(SelectionTool.Mode.Face, triangleIndex)) {
			mesh.FindSelectionVertices(toolRelativePositions, transform);
		} else {
			// If the hit triangle is not selected, then just find the vertices that
			// are in the given "face".
			mesh.FindFaceVertices(triangleIndex, toolRelativePositions, transform);			
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
		if (dragMesh != null) dragMesh.RecalcBoundsAndNormals();
	}
	
	/// <summary>
	/// Scale the selection (and any welded vertices) in the given direction and speed:
	/// +1 = scale up, -1 = scale down, 0 = no scaling.
	/// </summary>
	void ScaleSelection(float direction) {
		float scaleFactor = 1;
		if (direction > 0) {
			// If scaling up at full speed, double in size over 1 second; 2^(1/60) = 1.011619
			scaleFactor = 1f + 0.011619f * direction;
		} else {
			// If scaling down at full speed, halve the size over 1 second; 0.5^(1/60) = 0.011486
			scaleFactor = 1f + 0.011486f * direction;
		}

		foreach (int idx in toolRelativePositions.Keys.ToArray()) {
			toolRelativePositions[idx] *= scaleFactor;
		}
	}
}
