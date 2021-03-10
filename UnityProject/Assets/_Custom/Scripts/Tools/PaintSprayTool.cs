/*
Represents several kinds of paint tools: beam, cone, and brush.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PaintIn3D;

public class PaintSprayTool : Tool
{
	public enum SprayType {
		Beam,
		Cone,
		Brush		// (which is really just inverted cone
	}
	public SprayType sprayType = SprayType.Beam;

	public float beamWidth = 0.1f;
	
	public bool forceApply = false;
	
	public BrushPanel brushPanel;
	
	P3dHitBetween hitBetweenComponent;
	P3dPaintDecal paintDecalComponent;
	LineRenderer lineRenderer;
	Transform hitPoint;
	ParticleSystem hitParticles;
	Transform beamEndPoint;
	
	bool _eraseMode;
	bool eraseMode {
		get { return _eraseMode; }
		set { _eraseMode = value; UpdateBrush(); }
	}
	
	public Color color {
		get {
			return paintDecalComponent.Color;
		}
		set {
			if (hitBetweenComponent == null) Awake();
			paintDecalComponent.Color = value;
			if (sprayType == SprayType.Beam && sprayType == SprayType.Brush) {
				lineRenderer.SetColors(value, value);
			} else {
				Color clear = value;
				clear.a = 0;
				lineRenderer.SetColors(value, clear);
			}
			paintDecalComponent.Color = value;
			if (hitParticles != null) {
				hitParticles.startColor = value;
				var startColorModule = hitParticles.main.startColor;
				startColorModule.colorMin = value;
				startColorModule.colorMax = Color.Lerp(value, Color.white, 0.5f);
			}
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
	
	protected override void Awake() {
		base.Awake();
		hitBetweenComponent = GetComponent<P3dHitBetween>();
		beamEndPoint = hitBetweenComponent.PointB;
		paintDecalComponent = GetComponent<P3dPaintDecal>();
		Debug.Assert(hitBetweenComponent != null && paintDecalComponent != null);
		
		lineRenderer = GetComponentInChildren<LineRenderer>();
		Debug.Assert(lineRenderer != null);
		
		hitPoint = transform.Find("Hit Point");
		hitParticles = GetComponentInChildren<ParticleSystem>();

		UpdateLineWidth();
	}
	
	protected void Update() {
		base.Update();
		
		if (handTracker.GetButton(HandTracker.Button.X)) {
			if (!eraseMode) eraseMode = true;
		} else if (eraseMode) eraseMode = false;
		
		hitBetweenComponent.Pressure = handTracker.trigger * handTracker.trigger;
		if (forceApply) hitBetweenComponent.Pressure = 1;
		
		//hitBetweenComponent.Interval = (handTracker.trigger > 0.05f ? 0 : -1);
		hitBetweenComponent.Preview = (hitBetweenComponent.Pressure < 0.0025f);
		Color c = eraseMode ? Color.white : paintDecalComponent.Color;
		if (hitBetweenComponent.Preview) {
			if (hitParticles != null && hitParticles.isEmitting) hitParticles.Stop();
			c.a	= 0.1f;
			if (audio) audio.Stop();
		} else {
			if (hitParticles != null && !hitParticles.isEmitting && !eraseMode) hitParticles.Play();
			c.a = Mathf.Lerp(0.1f, 1f, hitBetweenComponent.Pressure);
			if (audio) {
				audio.volume = hitBetweenComponent.Pressure;
				if (!audio.isPlaying) audio.Play();
			}
		}
		
		if (sprayType == SprayType.Beam || sprayType == SprayType.Brush) {
			// Beam/brush spray is constant opacity, dependent only upon trigger pressure
			paintDecalComponent.Opacity = hitBetweenComponent.Pressure;
			lineRenderer.startColor = lineRenderer.endColor = c;
		} else {
			// With a cone spray, the opacity of the paint attenuates with distance
			float dist = hitPoint.localPosition.z;
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
			if (sprayType == SprayType.Brush) {
				beamLength = Mathf.Clamp(beamLength + dLen * 0.05f * Time.deltaTime, 0.01f, 0.2f);				
			} else {
				beamLength = Mathf.Clamp(beamLength + dLen * 0.5f * Time.deltaTime, 0.05f, 3f);
			}
		}
		UpdateLineWidth();
	}
	
	void UpdateLineWidth() {
		float w = paintDecalComponent.Radius * 2f;
		if (sprayType == SprayType.Beam) {
			// Beam: constant width
			lineRenderer.startWidth = lineRenderer.endWidth = beamWidth;
			paintDecalComponent.Radius = beamWidth * 0.5f;
			var br = brushPanel.currentBrush;
			if (br != null) hitBetweenComponent.HitSpacing = beamWidth * br.spacing * 0.01f;
		} else if (sprayType == SprayType.Cone) {
			// Cone: starts at zero width; increases with distance, up to beam width * 2
			float dist = hitPoint.localPosition.z;
			float widthFactor = 2f * dist / beamLength;
			lineRenderer.startWidth = 0;
			lineRenderer.endWidth = beamWidth * widthFactor;
			paintDecalComponent.Radius = beamWidth * widthFactor;
			var br = brushPanel.currentBrush;
			if (br != null) hitBetweenComponent.HitSpacing = beamWidth * widthFactor * br.spacing * 0.01f;
		} else {
			// Brush: starts at beam width; decreases to 0 at beam length
			float dist = hitPoint.localPosition.z;
			float widthFactor = Mathf.Clamp01(1f - dist / beamLength);
			lineRenderer.startWidth = beamWidth;
			lineRenderer.endWidth = beamWidth * widthFactor;
			paintDecalComponent.Radius = beamWidth * widthFactor;
			var br = brushPanel.currentBrush;
			if (br != null) hitBetweenComponent.HitSpacing = beamWidth * widthFactor * br.spacing * 0.01f;
		}
		
	}
	
	public void SetTypeBeam() { sprayType = SprayType.Beam; }
	public void SetTypeCone() { sprayType = SprayType.Cone; }
	
	public void UpdateBrush() {
		var br = brushPanel.currentBrush;
		if (br == null || paintDecalComponent == null) return;
		paintDecalComponent.Shape = br.texture;
		UpdateLineWidth();
		
		if (eraseMode) {
			paintDecalComponent.BlendMode = P3dBlendMode.Subtractive(new Vector4(0,0,0,1));
		} else {
			paintDecalComponent.BlendMode = P3dBlendMode.AlphaBlend(Vector4.one);
		}
		
		P3dModifyTextureRandom randTexMod = paintDecalComponent.Modifiers[0] as P3dModifyTextureRandom;
		randTexMod.Textures.Clear();
		for (int i=0; i<br.textures.Count; i++) {
			randTexMod.Textures.Add(br.textures[i]);
		}
	}
}
