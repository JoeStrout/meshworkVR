/*
This tool allows you to select/deselect faces, edges, or vertices.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionTool : Tool
{
	public Transform endPoint;
	public LayerMask selectableMask;
	public FormatText label;
	public FormatText infoTitle;
	public FormatText infoText;
	
	public static MeshEditMode mode = MeshEditMode.Face;
	
	enum DragMode {
		Idle,
		Selecting,
		Deselecting
	}
	
	bool wasDown = false;
	DragMode dragMode;
	
	float toolRayLength;
	Collider[] tempColliders = new Collider[32];
	MeshModel curMesh;			// mesh we are currently selecting
	MeshDisplay display;		// MeshDisplay associated with curMesh
	int curIndex;				// index of the vertex, edge, or face which we most recently hit

	bool hadCenter;	// flag indicating we had the thumb stick centered since we last did anything with it

	AudioSource audioSrc;
	
	protected override void Awake() {
		base.Awake();
		audioSrc = GetComponent<AudioSource>();
		SetMode(mode);
		hadCenter = false;
		toolRayLength = Vector3.Distance(transform.position, endPoint.position) + 0.01f;
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
		
		bool isDown = (handTracker.trigger > (wasDown ? 0.4f : 0.6f));
		if (isDown && !wasDown) BeginDrag();
		else if (wasDown && !isDown) EndDrag();
		else if (dragMode != DragMode.Idle) Drag();
		wasDown = isDown;
		
		if (handTracker.GetButtonDown(HandTracker.Button.X) && display != null) {
			if (display.DeselectAll(mode)) {
				audioSrc.pitch = 0.5f;
				audioSrc.Play();	// ToDo: play this at the location of the deselected triangle, in case it's far away!
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
	
	/// <summary>
	/// Find the face which the tool is currently pointing at or touching.
	/// </summary>
	/// <returns>true if face found; false if no face is hit</returns>
	bool FindFaceHitByTool(out MeshModel outModel, out int outTriIndex) {
		return SelectionUtils.FindFaceHitByTool(transform, toolRayLength, selectableMask, out outModel, out outTriIndex);
	}
	
	/// <summary>
	/// Find the edge which the tool is currently pointing at or touching.
	/// </summary>
	/// <returns>true if edge found; false if no edge is hit</returns>
	bool FindEdgeHitByTool(out MeshModel outModel, out int outEdgeIndex) {
		return SelectionUtils.FindEdgeHitByTool(transform, toolRayLength, selectableMask, out outModel, out outEdgeIndex);
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
			return SelectionUtils.FindFaceHitByTool(transform, toolRayLength, selectableMask, out outModel, out index);
		case MeshEditMode.Edge:
			return SelectionUtils.FindEdgeHitByTool(transform, toolRayLength, selectableMask, out outModel, out index);
		case MeshEditMode.Vertex:
			return false;  // ToDo
		}
		return false;
	}
	
	void ApplyDragMode() {
		bool isSelected = display.IsSelected(mode, curIndex);
		if (Input.GetKeyDown(KeyCode.LeftShift)) Debug.Log($"ApplyDragMode {dragMode} on index {curIndex} when isSelected={isSelected}");
		if (isSelected && dragMode == DragMode.Deselecting) {
			// Deselect!
			display.SetSelected(mode, curIndex, false);
			audioSrc.pitch = 0.8f;
			audioSrc.Play();	// ToDo: play this at the location of the deselected triangle, in case it's far away!
		} else if (!isSelected && dragMode == DragMode.Selecting) {
			// Select!
			display.SetSelected(mode, curIndex, true);
			audioSrc.pitch = 1.2f;
			audioSrc.Play();
		}
	}
	
	void BeginDrag() {
		if (!FindIndexHitByTool(out curMesh, out curIndex)) {
			dragMode = DragMode.Idle;
			return;
		}
		Debug.Log($"Hit mesh {curMesh.gameObject.name} on triangle {curIndex}");
		display = curMesh.GetComponent<MeshDisplay>();
		if (display.IsSelected(mode, curIndex)) dragMode = DragMode.Deselecting;
		else dragMode = DragMode.Selecting;
		ApplyDragMode();
	}
	
	void Drag() {
		if (FindIndexHitByTool(out curMesh, out curIndex)) {
			ApplyDragMode();
		}
	}
	
	void EndDrag() {
		dragMode = DragMode.Idle;
	}
	
}
