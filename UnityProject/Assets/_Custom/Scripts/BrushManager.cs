/*
BrushManager loads all the paint brushes we know about, and provides them
to painting tools etc. upon demand.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushManager : MonoBehaviour
{
	public static BrushManager instance { get; private set; }
	
	public string[] brushFolders;
	
	public static List<GimpBrush> brushes;
	
	protected void Awake() {
		instance = this;
		
		LoadBrushes();
	}
	
	protected void OnDestroy() {
		if (instance == this) instance = null;
	}
	
	void LoadBrushes() {
		brushes = new List<GimpBrush>();
		foreach (string path in brushFolders) {
			int count = 0;
			Debug.Log($"Attempting to load brushes from: {path}");
			Object[] assets = Resources.LoadAll(path, typeof(TextAsset));
			for (int i=0; i<assets.Length; i++) {
				if (!assets[i].name.EndsWith(".gbr") && !assets[i].name.EndsWith(".gih")) continue;
				var brush = GimpBrushParser.LoadBrush(assets[i] as TextAsset);
				if (brush.isValid) {
					brushes.Add(brush);
					count++;
				}
			}
			Debug.Log($"Loaded {count} brushes from {path}");
		}
	}
}
