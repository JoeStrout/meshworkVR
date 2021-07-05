/*
This module contains various utility functions related to selecting and deselecting
faces, edges, and vertices among all the currently loaded mesh models.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SelectionUtils
{
	/// <summary>
	/// Find the face which the tool is currently pointing at or touching.
	/// </summary>
	/// <returns>true if face found; false if no face is hit</returns>
	public static bool FindFaceHitByTool(Transform tool, float toolRayLength, int mask, 
		   out MeshModel outModel, out int outTriIndex) {
		outModel = null;
		outTriIndex = 0;
		RaycastHit hit;
		if (!Physics.Raycast(tool.position, tool.forward, out hit, toolRayLength, 
			mask, QueryTriggerInteraction.Collide)) return false;
		outModel = hit.collider.GetComponentInParent<MeshModel>();
		if (outModel == null) return false;
		outTriIndex = hit.triangleIndex;
		if (outTriIndex < 0) Debug.LogWarning($"triangleIndex = {outTriIndex} on {hit.collider}");
		return true;
	}
	
	/// <summary>
	/// Find the edge which the tool is currently pointing at or touching.
	/// </summary>
	/// <returns>true if edge found; false if no edge is hit</returns>
	public static bool FindEdgeHitByTool(Transform tool, float toolRayLength, int mask,
		   out MeshModel outModel, out int outEdgeIndex) {
		outModel = null;
		outEdgeIndex = 0;
		RaycastHit hit;
		if (Input.GetKeyDown(KeyCode.LeftShift)) Debug.Log($"FindEdgeHitByTool({tool.name})");
		if (!Physics.Raycast(tool.position, tool.forward, out hit, toolRayLength, 
			mask, QueryTriggerInteraction.Collide)) return false;
		if (Input.GetKeyDown(KeyCode.LeftShift)) Debug.Log($"got hit {hit.collider} at {hit.point}, tri {hit.triangleIndex}");
		outModel = hit.collider.GetComponentInParent<MeshModel>();
		if (Input.GetKeyDown(KeyCode.LeftShift)) Debug.Log($"found MeshModel");
		if (outModel == null) return false;
		// Now we know we hit this model at hit.triangleIndex; but which edge of that triangle
		// are we closest to?
		outEdgeIndex = -1;
		float bestDist = 999;
		Vector3[] cornerPoints = outModel.TriangleWorldPos(hit.triangleIndex);
		for (int i=0; i<3; i++) {
			Vector3 pointA = cornerPoints[i];
			Vector3 pointB = cornerPoints[(i+1)%3];
			float dist = MathUtils.DistanceToLineSegment(pointA, pointB, hit.point);
			if (Input.GetKeyDown(KeyCode.LeftShift)) Debug.Log($"distance to {pointA},{pointB} is {dist}");
			if (outEdgeIndex < 0 || dist < bestDist) {
				outEdgeIndex = hit.triangleIndex*3 + i;
				bestDist = dist;
			}
		}
		if (Input.GetKeyDown(KeyCode.LeftShift)) Debug.Log($"returning true with {outEdgeIndex}");
		if (outEdgeIndex < 0) Debug.LogWarning($"outEdgeIndex = {outEdgeIndex} on {hit.collider}");
		return true;
	}
	
}
