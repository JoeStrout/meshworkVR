/*
This script controls one menu in the Meshwork UI system.  A menu is a collection
of menu items, stacked vertically.  This can be popped up from a hardware control
or from some other interface control (like a higher-level menu).  It can be 
grabbed and torn off, and then either left in space or docked to either arm
(like any other UI panel).  If not torn off, then it disappears as soon as the
user finishes using it, or sufficiently ignores it.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meshwork.UI {

public class Menu : MonoBehaviour
{
	public FormatText titleText;
	public MenuItem menuItemPrototype;
	
	public float width { get; private set; }
	
	public bool isOpen { get { return gameObject.activeInHierarchy; } }
	
	public Menu parent { get { 
		if (transform.parent == null) return null;
		return transform.parent.GetComponentInParent<Menu>(); 
	} }
	
	public Menu topLevelMenu {
		get {
			Menu menu = this;
			Menu parent = menu.parent;
			while (parent != null) {
				menu = parent;
				parent = menu.parent;
			}
			return menu;
		}
	}

	Canvas canvas;
	List<MenuItem> items;
	
	bool needsResize = true;
	
	protected void Awake() {
		canvas = GetComponentInChildren<Canvas>();
		if (canvas.worldCamera == null) canvas.worldCamera = Camera.main;
		items = new List<MenuItem>();
	}
	
	protected void Update() {
		if (needsResize) Resize();
	}
	
	public void ClearItems() {
		if (items != null) {
			foreach (var item in items) Destroy(item.gameObject);
			items.Clear();
		} else {
			items = new List<MenuItem>();
		}
		menuItemPrototype.gameObject.SetActive(false);
		needsResize = true;
	}
	
	public void ShowTitle(bool showIt) {
		titleText.gameObject.SetActive(showIt);
		needsResize = true;
	}
	
	public MenuItem AddItem(string itemText, bool hasSubmenu=false, MenuItem.Action action=null) {
		var noob = Instantiate(menuItemPrototype, menuItemPrototype.transform.parent);
		noob.Configure(itemText, hasSubmenu, action);
		float y = 0;
		if (titleText.gameObject.activeSelf) y -= titleText.RectTransform().sizeDelta.y;
		foreach (var item in items) {
			y -= item.RectTransform().sizeDelta.y;
		}
		var rt = noob.RectTransform();
		rt.anchoredPosition = rt.anchoredPosition.WithY(y);
		noob.gameObject.SetActive(true);
		items.Add(noob);
		return noob;
	}
	
	void Resize() {
		width = 0;
		float y = 0;
		if (titleText.gameObject.activeSelf) {
			y -= titleText.RectTransform().sizeDelta.y;
		}
		foreach (var item in items) {
			width = Mathf.Max(width, item.PreferredWidth());
			y -= item.RectTransform().sizeDelta.y;
		}
		var rt = canvas.RectTransform();
		rt.sizeDelta = new Vector3(width, -y);
		width = width * rt.localScale.x;	// publicly, width is always in world coordinates
		
		canvas.GetComponent<CanvasColliderAdjuster>().AdjustColliders();
		needsResize = false;
	}
	
	public void Close() {
		CloseSubmenus();
		gameObject.SetActive(false);
	}
	
	public void Show() {
		gameObject.SetActive(true);
	}
	
	public void CloseSubmenus() {
		for (int i=0; i<transform.childCount; i++) {
			var submenu = transform.GetChild(i).GetComponent<Menu>();
			if (submenu != null) submenu.Close();
		}
	}
	
}

}
