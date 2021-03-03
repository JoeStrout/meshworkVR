/*
This component goes on each hand, and manages what tool is active based
on what's going on.  For example, if your hand is pointing at a UI canvas,
it always disables the current tool and enables the UI interactor.  Or
while there is a modal dialog up needing text input, it enables the
typing sticks.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolModeManager : MonoBehaviour
{
	// Currently selected tool — note that it may not be active if it
	// is overridden by some UI thing.
	public Tool selectedTool { get; private set; }
	
	// Currently active tool: this may change based on context.
	public Tool activeTool { get; private set; }
	
	// Default tool, selected upon startup
	public Tool defaultTool;
	
	// Special context-activated tools:
	public UIInteractionTool uiTool;

	protected void Start() {
		uiTool.Deactivate();
		SelectTool(defaultTool);
	}

	protected void Update() {
		// check whether the UI tool is pointed at a UI
		bool doUI = uiTool.CanApply();
		if (doUI && activeTool != uiTool) {
			// Enable UI interaction
			Activate(uiTool);
		} else if (!doUI && activeTool == uiTool) {
			// Disable UI interaction, by activating the selected tool instead
			Activate(selectedTool);
		}
	}

	public void SelectTool(Tool tool) {
		selectedTool = tool;
		if (activeTool != uiTool) Activate(tool);
	}
	
	void Activate(Tool tool) {
		if (activeTool == tool) return;
		if (activeTool != null) activeTool.Deactivate();
		activeTool = tool;
		if (activeTool != null) activeTool.Activate();
	}

}
