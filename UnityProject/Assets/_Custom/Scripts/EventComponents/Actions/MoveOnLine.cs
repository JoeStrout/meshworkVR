using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MoveOnLine : MonoBehaviour {
	#region Public Properties
	[Tooltip("One end of the line.")]
	public Vector3 positionA;
	
	[Tooltip("Other end of the line.")]
	public Vector3 positionB;
	
	[Tooltip("Speed at which to move, in units/second.")]
	public float speed = 5;

	[Tooltip("If true, don't allow movement past the endpoints.")]
	public bool limitRange = true;

	public float distanceToA {
		get {
			return (transform.position - positionA).magnitude;
		}
	}

	public float distanceToB {
		get {
			return (transform.position - positionB).magnitude;
		}
	}

	#endregion
	//--------------------------------------------------------------------------------
	#region Private Properties
	float direction;	// -1 to move towards A; 1 to move towards B; 0 if not moving

	#endregion
	//--------------------------------------------------------------------------------
	#region MonoBehaviour Events
	void Update() {
		if (direction == 0) return;
		if (limitRange) {
			float maxMove = speed * Time.deltaTime;
			if (direction > 0) {
				transform.position = Vector3.MoveTowards(transform.position,
				                                         positionB, maxMove);
			} else {
				transform.position = Vector3.MoveTowards(transform.position,
				                                         positionA, maxMove);
			}
		} else {
			Vector3 dpos = (positionB - positionA).normalized * direction;
			transform.position += dpos * speed * Time.deltaTime;
		}
	}

//	void OnDrawGizmos() {
//		Gizmos.color = Color.green;
//		Gizmos.DrawLine(positionA, positionB);
//	}
	
	#endregion
	//--------------------------------------------------------------------------------
	#region Public Methods

	public void MoveInDirectionA() {
		direction = -1;
	}

	public void MoveInDirectionB() {
		direction = 1;
	}

	public void MoveToNearest() {
		if (distanceToA < distanceToB) MoveInDirectionA();
		else MoveInDirectionB();
	}

	public void MoveToFarthest() {
		if (distanceToA > distanceToB) MoveInDirectionA();
		else MoveInDirectionB();
	}
	
	public void StopMoveInDirectionA() {
		if (direction < 0) direction = 0;
	}

	public void StopMoveInDirectionB() {
		if (direction > 0) direction = 0;
	}

	public void SetSpeed(float speed) {
		this.speed = speed;
	}

	public void JumpToA() {
		transform.position = positionA;
	}

	public void JumpToB() {
		transform.position = positionB;
	}

	#endregion
	//--------------------------------------------------------------------------------
	#region Private Methods

	[ContextMenu("Set Position A to Current")]
	void SetPosAToCurrent() {
		positionA = transform.position;
	}
	
	[ContextMenu("Set Position B to Current")]
	void SetPosBToCurrent() {
		positionB = transform.position;
	}
	
	
	#endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(MoveOnLine))]
class MoveOnLineEditor : Editor {
	void OnSceneGUI() {
		var mol = target as MoveOnLine;

		Handles.DrawLine(mol.positionA, mol.positionB);

		mol.positionA = Handles.FreeMoveHandle(mol.positionA,
	       Quaternion.identity,
	       HandleUtility.GetHandleSize(mol.positionA) * 0.1f,
	       Vector3.zero,
			Handles.CubeHandleCap);
		Handles.Label(mol.positionA, "A");
		
		mol.positionB = Handles.FreeMoveHandle(mol.positionB,
	       Quaternion.identity,
	       HandleUtility.GetHandleSize(mol.positionA) * 0.1f,
	       Vector3.zero,
			Handles.CubeHandleCap);
		Handles.Label(mol.positionB, "B");
		
		if (GUI.changed) EditorUtility.SetDirty(target);
	}
}
#endif
