/*
This tool allows you to select/deselect faces, edges, or vertices.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionTool : Tool
{
	public enum Mode {
		Vertex,
		Edge,
		Face
	}
	public Transform endPoint;
	public LayerMask selectableMask;
	public FormatText label;
	public FormatText infoTitle;
	public FormatText infoText;
	
	public static Mode mode = Mode.Face;
	
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
	
	/// <summary>
	/// Find the face which the tool is currently pointing at or touching.
	/// </summary>
	/// <returns>true if face found; false if no face is hit</returns>
	bool FindFaceHitByTool(out MeshModel outModel, out int outTriIndex) {
		outModel = null;
		outTriIndex = 0;
		RaycastHit hit;
		if (!Physics.Raycast(transform.position, transform.forward, out hit, toolRayLength, 
			selectableMask, QueryTriggerInteraction.Collide)) return false;
		outModel = hit.collider.GetComponentInParent<MeshModel>();
		if (outModel == null) return false;
		outTriIndex = hit.triangleIndex;
		if (outTriIndex < 0) Debug.LogWarning($"triangleIndex = {outTriIndex} on {hit.collider}");
		return true;
	}
	
	void ApplyDragMode() {
		bool isSelected = display.IsSelected(mode, curIndex);
		if (isSelected && dragMode == DragMode.Deselecting) {
			// Deselect!
			Debug.Log($"Deselecting triangle {curIndex} of {curMesh.gameObject.name}");
			display.SetSelected(mode, curIndex, false);
			audioSrc.pitch = 0.9f;
			audioSrc.Play();
		} else if (!isSelected && dragMode == DragMode.Selecting) {
			// Select!
			Debug.Log($"Selecting triangle {curIndex} of {curMesh.gameObject.name}");
			display.SetSelected(mode, curIndex, true);
			audioSrc.pitch = 1.1f;
			audioSrc.Play();
		}
	}
	
	void BeginDrag() {
		if (!FindFaceHitByTool(out curMesh, out curIndex)) {
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
		if (FindFaceHitByTool(out curMesh, out curIndex)) {
			ApplyDragMode();
		}
	}
	
	void EndDrag() {
		dragMode = DragMode.Idle;
	}
	
}
