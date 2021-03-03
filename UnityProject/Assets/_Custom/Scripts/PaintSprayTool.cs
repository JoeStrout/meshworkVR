/*

*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PaintIn3D;

public class PaintSprayTool : Tool
{
	public enum SprayType {
		Beam,
		Cone
	}
	public SprayType sprayType = SprayType.Beam;

	public float beamWidth = 0.1f;
	
	public bool forceApply = false;
	
	P3dHitBetween hitBetweenComponent;
	P3dPaintDecal paintDecalComponent;
	HandTracker handTracker;
	LineRenderer lineRenderer;
	ParticleSystem hitParticles;
	AudioSource audio;
	Transform beamEndPoint;
	
	
	public Color color {
		get {
			return paintDecalComponent.Color;
		}
		set {
			if (hitBetweenComponent == null) Awake();
			paintDecalComponent.Color = value;
			if (sprayType == SprayType.Beam) {
				lineRenderer.SetColors(value, value);
			} else {
				Color clear = value;
				clear.a = 0;
				lineRenderer.SetColors(value, clear);
			}
			paintDecalComponent.Color = value;
			hitParticles.startColor = value;
			var startColorModule = hitParticles.main.startColor;
			startColorModule.colorMin = value;
			startColorModule.colorMax = Color.Lerp(value, Color.white, 0.5f);
		}
	}
	
	public float beamLength {
		get { return beamEndPoint.localPosition.z; }
		set {
			Vector3 pos = beamEndPoint.localPosition;
			pos.z = value;
			beamEndPoint.localPosition = pos;
		}
	}
	
	protected void Awake() {
		hitBetweenComponent = GetComponent<P3dHitBetween>();
		beamEndPoint = hitBetweenComponent.PointB;
		paintDecalComponent = GetComponent<P3dPaintDecal>();
		Debug.Assert(hitBetweenComponent != null && paintDecalComponent != null);
		
		lineRenderer = GetComponentInChildren<LineRenderer>();
		Debug.Assert(lineRenderer != null);
		
		handTracker = transform.parent.GetComponentInChildren<Grabber>().handTracker;
		Debug.Assert(handTracker != null);		
		
		hitParticles = GetComponentInChildren<ParticleSystem>();

		UpdateLineWidth();
		
		audio = GetComponent<AudioSource>();
	}
	
	protected void Update() {
		hitBetweenComponent.Pressure = handTracker.trigger * handTracker.trigger;
		if (forceApply) hitBetweenComponent.Pressure = 1;
		
		//hitBetweenComponent.Interval = (handTracker.trigger > 0.05f ? 0 : -1);
		hitBetweenComponent.Preview = (hitBetweenComponent.Pressure < 0.0025f);
		Color c = paintDecalComponent.Color;
		if (hitBetweenComponent.Preview) {
			if (hitParticles.isEmitting) hitParticles.Stop();
			c.a	= 0.1f;
			if (audio) audio.Stop();
		} else {
			if (!hitParticles.isEmitting) hitParticles.Play();
			c.a = Mathf.Lerp(0.1f, 1f, hitBetweenComponent.Pressure);
			if (audio) {
				audio.volume = hitBetweenComponent.Pressure;
				if (!audio.isPlaying) audio.Play();
			}
		}
		
		if (sprayType == SprayType.Beam) {
			// Beam spray is constant opacity, dependent only upon trigger pressure
			paintDecalComponent.Opacity = hitBetweenComponent.Pressure;
			lineRenderer.startColor = lineRenderer.endColor = c;
		} else {
			// With a cone spray, the opacity of the paint attenuates with distance
			float dist = hitParticles.transform.localPosition.z;
			float attenuation = 1f - (dist / beamLength);
			paintDecalComponent.Opacity = attenuation * hitBetweenComponent.Pressure;
			lineRenderer.startColor = c;
			c.a *= attenuation;
			lineRenderer.endColor = c;
		}
		if (hitBetweenComponent.Preview) paintDecalComponent.Opacity = 0.5f;
		
		float dSize = handTracker.thumbStick.x;
		if (Mathf.Abs(dSize) > 0.1f) {
			beamWidth = Mathf.Clamp(beamWidth + dSize * 0.25f * Time.deltaTime, 0.01f, 0.5f);
		}
		float dLen = handTracker.thumbStick.y;
		if (Mathf.Abs(dLen) > 0.1f) {
			beamLength = Mathf.Clamp(beamLength + dLen * 0.5f * Time.deltaTime, 0.05f, 3f);
		}
		UpdateLineWidth();
	}
	
	void UpdateLineWidth() {
		float w = paintDecalComponent.Radius * 2f;
		if (sprayType == SprayType.Beam) {
			// Beam: constant width
			lineRenderer.startWidth = lineRenderer.endWidth = beamWidth;
			paintDecalComponent.Radius = beamWidth * 0.5f;
		} else {
			// Cone: starts at zero width; increases with distance, up to beam width * 2
			float dist = hitParticles.transform.localPosition.z;
			float widthFactor = 2f * dist / beamLength;
			lineRenderer.startWidth = 0;
			lineRenderer.endWidth = beamWidth * widthFactor;
			paintDecalComponent.Radius = beamWidth * widthFactor;
		}
		
	}
	
	public void SetTypeBeam() { sprayType = SprayType.Beam; }
	public void SetTypeCone() { sprayType = SprayType.Cone; }
}
