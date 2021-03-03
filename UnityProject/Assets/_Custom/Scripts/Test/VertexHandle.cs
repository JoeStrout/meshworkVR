using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexHandle : Grabbable
{
	public VectrosityTest owner;
	public int vertexNum;
	
	protected override void AfterGrabbedUpdate() {
		owner.MoveVertexTo(vertexNum, transform.localPosition);
	}
	
	protected override void AfterRelease() {
		owner.RebuildFaceCollider();
	}
}
