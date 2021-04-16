/*
This script creates and manages all the menus in the main Meshwork UI.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meshwork.UI;

public class MeshworkMenus : MonoBehaviour
{
	public Menu menuPrefab;
	public Grabbable toolsPanel;
	public Grabbable sceneTransformPanel;
	public Grabbable loadReference2DPanel;
	public Grabbable loadReference3DPanel;
	public Grabbable loadRefFromFilePanel;
	public Grabbable importModelPanel;
	public Grabbable saveFilePanel;
	
	protected Menu mainMenu { get {
		if (_mainMenu == null) {
			_mainMenu = Instantiate(menuPrefab, null);
			LoadMainMenu(_mainMenu);
		}
		return _mainMenu;
	}}
	Menu _mainMenu = null;

	protected Menu sceneMenu { get {
		if (_sceneMenu == null) {
			_sceneMenu = Instantiate(menuPrefab, mainMenu.transform);
			LoadSceneMenu(_sceneMenu);
		}
		return _sceneMenu;
	}}
	Menu _sceneMenu = null;
	
	protected Menu loadRefMenu { get {
		if (_loadRefMenu == null) {
			_loadRefMenu = Instantiate(menuPrefab, mainMenu.transform);
			LoadReferenceMenu(_loadRefMenu);
		}
		return _loadRefMenu;
	}}
	Menu _loadRefMenu = null;
	
	protected Menu editMenu { get {
		if (_editMenu == null) {
			_editMenu = Instantiate(menuPrefab, mainMenu.transform);
			LoadEditMenu(_editMenu);
		}
		return _editMenu;
	}}
	Menu _editMenu = null;
	
	protected Menu selectionMenu { get {
		if (_selectionMenu == null) {
			_selectionMenu = Instantiate(menuPrefab, mainMenu.transform);
			LoadSelectionMenu(_selectionMenu);
		}
		return _selectionMenu;
	}}
	Menu _selectionMenu = null;
	
	protected Menu modeMenu { get {
		if (_modeMenu == null) {
			_modeMenu = Instantiate(menuPrefab, mainMenu.transform);
			LoadModeMenu(_modeMenu);
		}
		return _modeMenu;
	}}
	Menu _modeMenu = null;
	
	protected Menu createMenu { get {
		if (_createMenu == null) {
			_createMenu = Instantiate(menuPrefab, mainMenu.transform);
			LoadCreateMenu(_createMenu);
		}
		return _createMenu;
	}}
	Menu _createMenu = null;
	
	protected Menu modifyMenu { get {
		if (_modifyMenu == null) {
			_modifyMenu = Instantiate(menuPrefab, mainMenu.transform);
			LoadModifyMenu(_modifyMenu);
		}
		return _modifyMenu;
	}}
	Menu _modifyMenu = null;
	
	protected void Start() {
		mainMenu.Close();
	}
	
	protected void Update() {
		if (GlobalRefs.instance.leftHandTracker.GetButtonDown(HandTracker.Button.Start)) {
			ToggleMenu();
		}
	}
	
	[ContextMenu("Toggle Menu")]
	public void ToggleMenu() {
		// Show/hide the main menu.
		// Start by calculating where the menu should be.
		Vector3 pos = GlobalRefs.instance.leftHandTracker.handTransform.position;
		Vector3 camPos = Camera.main.transform.position;
		Vector3 dpos = pos - camPos;
		dpos.y = 0;
		dpos = dpos.normalized * 0.5f;
		pos = camPos + dpos - Vector3.up * 0.35f;
		
		// If it's open and within 1.5 meters of that, close it.  Otherwise,
		// open it (if needed) and move it to that spot.
		if (Vector3.Distance(mainMenu.transform.position, pos) < 1.5f && mainMenu.isOpen) {
			Debug.Log("Closing main menu");
			mainMenu.Close();
		} else {
			Debug.Log("Opening main menu");
			mainMenu.transform.position = pos;
			mainMenu.transform.rotation = Quaternion.LookRotation(dpos, Vector3.up);
			mainMenu.CloseSubmenus();
			mainMenu.Show();
		}
	}
	
	void PrepareMenu(Menu menu, string title, bool showTitle=false) {
		menu.gameObject.name = title + " (Menu)";
		menu.titleText.SetString(title);
		menu.ClearItems();
		menu.ShowTitle(showTitle);
	}
	
	protected void LoadMainMenu(Menu menu) {
		PrepareMenu(menu, "Main Menu", false);
		menu.AddItem("Scene", true, (item,left) => { item.ShowSubmenu(sceneMenu); } );
		menu.AddItem("Edit", true, (item,left) => { item.ShowSubmenu(editMenu); } );
		menu.AddItem("Selection", true, (item,left) => { item.ShowSubmenu(selectionMenu); });
		menu.AddItem("Create", true, (item,left) => { item.ShowSubmenu(createMenu); });
		menu.AddItem("Modify", true, (item,left) => { item.ShowSubmenu(modifyMenu); });
		menu.AddItem("Tools...", false, (item,left) => { item.ShowPanel(toolsPanel); });
	}
	
	protected void LoadSceneMenu(Menu menu) {
		PrepareMenu(menu, "Scene");
		menu.AddItem("Load...");
		menu.AddItem("Save...", false, (MenuItem,left) => { MenuItem.ShowPanel(saveFilePanel); } );
		menu.AddItem("Import Model...", false, (item,left) => { item.ShowPanel(importModelPanel); } );
		menu.AddItem("Transform...", false, (item,left) => { item.ShowPanel(sceneTransformPanel); });
		menu.AddItem("Background...");
		menu.AddItem("Add Reference", true, (item,left) => { item.ShowSubmenu(loadRefMenu); });
	}
	
	protected void LoadReferenceMenu(Menu menu) {
		PrepareMenu(menu, "Load Reference");
		menu.AddItem("2D Image...", false, (item,left) => { item.ShowPanel(loadReference2DPanel); });
		menu.AddItem("3D Model...", false, (item,left) => { item.ShowPanel(loadReference3DPanel); });
		menu.AddItem("From File...", false, (item,left) => { item.ShowPanel(loadRefFromFilePanel); });
	}
	
	protected void LoadEditMenu(Menu menu) {
		PrepareMenu(menu, "Edit");
		menu.AddItem("Undo");
		menu.AddItem("Redo");
		menu.AddItem("Cut");
		menu.AddItem("Copy");
		menu.AddItem("Paste");
		menu.AddItem("Delete");		
	}
	
	protected void LoadSelectionMenu(Menu menu) {
		PrepareMenu(menu, "Selection");
		menu.AddItem("Mode", true, (item,left) => { item.ShowSubmenu(modeMenu); });
		menu.AddItem("Transform...");
		menu.AddItem("Select All");
		menu.AddItem("Deselect");
		menu.AddItem("Invert Selection");
		menu.AddItem("Hide");
		menu.AddItem("Unhide All");
		menu.AddItem("Invert Hidden");
	}
	
	protected void LoadModeMenu(Menu menu) {
		PrepareMenu(menu, "Mode");
		menu.AddItem("Objects");
		menu.AddItem("Faces");
		menu.AddItem("Edges");
		menu.AddItem("Points");		
	}
	
	protected void LoadCreateMenu(Menu menu) {
		PrepareMenu(menu, "Create");
		menu.AddItem("Box...");
		menu.AddItem("Cylinder...");
		menu.AddItem("Cone...");
		menu.AddItem("Sphere...");
	}
	
	
	protected void LoadModifyMenu(Menu menu) {
		PrepareMenu(menu, "Modify");
		menu.AddItem("Twist...");
		menu.AddItem("Spherify...");
		menu.AddItem("Symmetry...");
		menu.AddItem("Clean Up...");
	}
}
