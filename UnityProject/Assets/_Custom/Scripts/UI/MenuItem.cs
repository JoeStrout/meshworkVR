/*
This script controls one menu item in the Meshwork UI system.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meshwork.UI {

public class MenuItem : MonoBehaviour
{
	public FormatText itemText;
	public GameObject submenuIndicator;
	
	public void Configure(string text, bool hasSubmenu) {
		itemText.SetString(text);
		submenuIndicator.SetActive(hasSubmenu);
		gameObject.name = text + " (MenuItem)";
	}
}


}