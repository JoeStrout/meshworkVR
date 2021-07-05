/*
This tool allows you to grab and drag parts of a mesh around: either vertices,
or edges, or faces, depending on the current editing mode.

It also allows for simple selection/deselection.  (We may not need a separate
Selection Tool after all!)
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshTweakTool : Tool
{
	public Transform endPoint;
	public LayerMask tweakableMask;
	public FormatText label;
	public FormatText infoTitle;
	public FormatText infoText;
	public AudioSource extrudeSound;
	
	public MeshEditMode mode = MeshEditMode.Vertex;
	
	bool isDragging = false;
	bool isSelecting = false;
	bool isDeselecting = false;
	
	bool grabWasDown = false;
	
	float toolRayLength;
	Collider[] tempColliders = new Collider[32];
	MeshModel curMesh;			// mesh we are currently tweaking
	int curIndex;				// index of the vertex, edge, or face which we are tweaking
	MeshDisplay display;		// MeshDisplay associated with curMesh
	Vector3 dragStartToolPos;	// local (to curMesh) position of the tool at the start of the drag
	Vector3 dragStartVPos;		// local position of the grabbed vertex at the start of the drag
	Vector3 lastToolWorldPos;	// world position of the tool on last update

	bool hadCenter;	// flag indicating we had the thumb stick centered since we last did anything with it

	Dictionary<int, Vector3> toolRelativePositions;

	protected override void Awake() {
		base.Awake();
		SetMode(mode);
		hadCenter = false;
		toolRelativePositions = new Dictionary<int, Vector3>();
		toolRayLength = Vector3.Distance(transform.position, endPoint.position) + 0.01f;
	}

	protected void Update() {
		base.Update();
				
		bool isDown = (handTracker.trigger > (grabWasDown ? 0.4f : 0.6f));
		if (isDown && !grabWasDown) StartTool();
		else if (grabWasDown && !isDown) EndTool();
		else if (isDragging) Drag();
		grabWasDown = isDown;
		
		float stickX = handTracker.thumbStick.x;
		if (isDragging) {
			// Additional functions you can do while dragging:
			
			// Extrude (button X)
			if (handTracker.GetButtonDown(HandTracker.Button.X)) {
				extrudeSound.Play();
				curMesh.DoExtrude();
			}
			
			// Scale (thumb stick)
			float stickY = handTracker.thumbStick.y;
			float scaleInput = (Mathf.Abs(stickX) > Mathf.Abs(stickY) ? stickX : stickY);
			if (scaleInput > 0.1f) ScaleSelection(Mathf.InverseLerp(0.1f, 1f, scaleInput));
			else if (scaleInput < -0.1f) ScaleSelection(-Mathf.InverseLerp(-0.1f, -1f, scaleInput));
		} else if (isSelecting || isDeselecting) {
			// Extend a selection or deselection.
			MeshModel model;
			int index;
			if (FindIndexHitByTool(out model, out index) && model == curMesh && index != curIndex) {
				display.SetSelected(mode, index, isSelecting);
				curIndex = index;
			}
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
	
	void SetMode(MeshEditMode newMode) {
		mode = newMode;
		string modeStr = mode.ToString().ToLower();
		label.SetString(modeStr);
		infoTitle.SetString(modeStr);
		infoText.SetString(modeStr);
	}
	
	void ShiftMode(int delta) {
		int modeNum = (int)mode;
		int count = System.Enum.GetValues(typeof(MeshEditMode)).Length;
		SetMode((MeshEditMode)((modeNum + delta + count) % count));
	}
	
	[ContextMenu("Next Mode")] void NextMode() { ShiftMode(1); }
	
	void StartTool() {
		MeshModel meshHit;
		int indexHit;
		bool hit = FindIndexHitByTool(out meshHit, out indexHit);	// ToDo: other modes.
		if (!hit) {
			// We couldn't find any, so deselect all on last mesh, then bail out.
			if (display != null) display.DeselectAll(mode);
			display = null;
			curMesh = null;
			return;
		}
		curMesh = meshHit;
		curIndex = indexHit;
		display = curMesh.GetComponent<MeshDisplay>();
		bool isSelected = display.IsSelected(mode, curIndex);
		
		// The behavior now depends on whether the hit thing was already selected,
		// and the state of the modifier buttons.
		if (handTracker.GetButton(HandTracker.Button.X)) {
			Debug.Log("Toggling selection " + (isSelected ? "off" : "on"));
			// With A/X button held: just toggle selection.
			isSelecting = !isSelected;
			isDeselecting = isSelected;
			isDragging = false;
			display.SetSelected(mode, curIndex, isSelecting);
		} else if (display.IsSelected(mode, curIndex)) {
			// An already-selected thing: start dragging.
			Debug.Log("Dragging in mode " + mode);
			GrabRelevantVertices();
			isDragging = true;
			isSelecting = isDeselecting = false;
			lastToolWorldPos = endPoint.position;
			dragStartToolPos = curMesh.transform.InverseTransformPoint(endPoint.position);
			dragStartVPos = curMesh.Vertex(curIndex);
			if (audio != null) audio.Play();
		} else {
			// Not previously selected: deselect all, then select thing clicked.
			Debug.Log($"Deselecting all, then selecting index {curIndex} in mode {mode}");
			display.DeselectAll(mode);
			isSelecting = true;
			isDeselecting = isDragging = false;
			display.SetSelected(mode, curIndex, true);
		}
		
	}

	/// <summary>
	/// Find the index of the face, edge, or vertex (depending on our current mode)
	/// which is hit by this tool.
	/// </summary>
	/// <param name="outModel">receives model containing the item hit</param>
	/// <param name="index">receives index of the item hit</param>
	/// <returns>true if any item is hit; false if none</returns>
	bool FindIndexHitByTool(out MeshModel outModel, out int index) {
		outModel = null; 
		index = -1;
		switch (mode) {
		case MeshEditMode.Face:
			return SelectionUtils.FindFaceHitByTool(transform, toolRayLength, tweakableMask, out outModel, out index);
		case MeshEditMode.Edge:
			return SelectionUtils.FindEdgeHitByTool(transform, toolRayLength, tweakableMask, out outModel, out index);
		case MeshEditMode.Vertex:
			return false;  // ToDo
		}
		return false;
	}

	/// <summary>
	/// Grab the vertices of the set of faces that should be dragged along with the given (hit) triangle.
	/// </summary>
	/// <param name="mesh"></param>
	/// <param name="triangleIndex"></param>
	void GrabFaces(MeshModel mesh, int triangleIndex) {
		toolRelativePositions.Clear();

		// First, check whether the hit triangle is selected.  If so, then we just
		// need to grab all the vertices in the selection.
		var disp = mesh.GetComponent<MeshDisplay>();
		if (disp.IsSelected(MeshEditMode.Face, triangleIndex)) {
			mesh.FindSelectionVertices(MeshEditMode.Face, toolRelativePositions, endPoint);
		} else {
			// If the hit triangle is not selected, then just find the vertices that
			// are in the given "face".
			mesh.FindFaceVertices(triangleIndex, toolRelativePositions, endPoint);			
		}
	}
	
	/// <summary>
	/// Grab the set of vertices connected to our selection (in any mode).
	/// Store their tool-relative positions in toolRelativePositions, so we can
	/// then drag or scale them as the tool is moved by the user.
	/// </summary>
	void GrabRelevantVertices() {
		toolRelativePositions.Clear();
		curMesh.FindSelectionVertices(mode, toolRelativePositions, endPoint);			
	}
	
	void Drag() {
		Vector3 toolPos = curMesh.transform.InverseTransformPoint(endPoint.position);
		if (mode == MeshEditMode.Vertex) {
			Vector3 delta = toolPos - dragStartToolPos;
			Vector3 newVPos = dragStartVPos + delta;			
			curMesh.ShiftVertexTo(curIndex, newVPos);
		} else {
			int i = 1;	// count as we go, so we know when we've reached the last one
			foreach (var kv in toolRelativePositions) {
				int vidx = kv.Key;
				Vector3 v = curMesh.transform.InverseTransformPoint(endPoint.TransformPoint(kv.Value));
				curMesh.ShiftVertexTo(vidx, v, i == toolRelativePositions.Count);	// update mesh on last one
				i++;
			}
		}
		
		if (audio != null) {
			float moveDist = Vector3.Distance(lastToolWorldPos, endPoint.position);
			audio.volume = Mathf.MoveTowards(audio.volume, Mathf.Clamp01(moveDist/Time.deltaTime), 5*Time.deltaTime);
		}
		lastToolWorldPos = endPoint.position;
	}
	
	void EndTool() {
		if (audio != null) audio.Stop();
		if (curMesh != null && isDragging) curMesh.RecalcBoundsAndNormals();
		isDragging = isSelecting = isDeselecting = false;
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
