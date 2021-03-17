/*
This script creates and manages all the menus in the main Meshwork UI.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meshwork.UI;

public class MeshworkMenus : MonoBehaviour
{
	public Menu mainMenu;
	
	
	protected void Start() {
		PrepareMenu(mainMenu, "Main Menu Test", false);
		mainMenu.AddItem("Scene", true);
		mainMenu.AddItem("Edit", true);
		mainMenu.AddItem("Selection", true);
		mainMenu.AddItem("Create", true);
		mainMenu.AddItem("Modify", true);
		mainMenu.AddItem("Tools...", false);
	}
	
	void PrepareMenu(Menu menu, string title, bool showTitle) {
		menu.titleText.SetString(title);
		menu.ClearItems();
		menu.ShowTitle(showTitle);
	}
}
