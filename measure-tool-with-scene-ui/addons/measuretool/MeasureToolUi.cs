#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
[Tool]
public partial class MeasureToolUi : Control
{
	public measuretool measureTool;
	[Export] Label infoLabel;
	[Export] Label DistanceLabel;
	[Export] Button measureButton;
	[Export] LineEdit nameEdit;
	[Export] Button saveButton;
	[Export] ItemList measurementList;
	[Export] Button removeButton;
	[Export] Button clearAllButton;
	[Export] RichTextLabel detailsLabel;


	public void SetInfoText(string text) => infoLabel.Text = text;
	public void SetDistanceText(string text) => DistanceLabel.Text = text;
	public void SetMeasureButtonEnabled(bool on) => measureButton.Disabled = !on;
	public void SetSaveButtonEnabled(bool on) => saveButton.Disabled = !on;

	public void ClearDetails() => detailsLabel.Text = "Select a saved measurement to see its details.";



    public override void _EnterTree()
    {
        measureButton.Pressed += OnMeasurePressed;
		saveButton.Pressed += OnSavePressed;
		removeButton.Pressed += OnRemovePressed;
		clearAllButton.Pressed += OnClearAllPressed;
		measurementList.ItemSelected += OnListItemSelected;
		measurementList.EmptyClicked += OnListEmptyClicked;
    }

	public void RefreshMeasurementList(List<Measurement> measurements)
	{
		measurementList.Clear();
		foreach (var m in measurements)
			measurementList.AddItem($"{m.Name}  ({m.Distance:F2})");
		removeButton.Disabled = true;
		clearAllButton.Disabled = measurements.Count == 0;
	}

	public void ShowDetails(string name, string nodeAName, Vector3 pointA, string nodeBName,
		Vector3 pointB, float distance, Vector3 vector, Vector3 direction, string equation)
	{
		detailsLabel.Text =
			$"[{name}]\n\n" +
			"<Math informations>\n\n" +
			$"A: {nodeAName}  ({pointA.X:F2}, {pointA.Y:F2}, {pointA.Z:F2})\n" +
			$"B: {nodeBName}  ({pointB.X:F2}, {pointB.Y:F2}, {pointB.Z:F2})\n\n" +
			$"Distance: {distance:F3}\n\n" +
			$"Vector (B - A): ({vector.X:F2}, {vector.Y:F2}, {vector.Z:F2})\n\n" +
			$"Unit direction: ({direction.X:F2}, {direction.Y:F2}, {direction.Z:F2})\n\n" +
			$"Line equation:\n{equation}\n\n" +
			"<Code informations>\n\n" +
			$"Vector3 a = new Vector3({pointA.X:F4}f, {pointA.Y:F4}f, {pointA.Z:F4}f);\n" +
			$"Vector3 b = new Vector3({pointB.X:F4}f, {pointB.Y:F4}f, {pointB.Z:F4}f);\n" +
			$"Vector3 dir = (b - a).Normalized();\n" +
			$"float dist = (b - a).Length();\n" +
			$"// P(t) = a + t * dir, t in [0, 1]";
	}

	private void OnMeasurePressed()
	{
		if (measureTool == null) return;
		float dist = measureTool.MeasureCurrentPair();
		SetDistanceText($"Distance:{dist:F2}");
	}

	private void OnSavePressed()
	{
		if (measureTool == null) return;
		measureTool.SaveMeasurement(nameEdit.Text);
		nameEdit.Text = "";
	}
	private void OnRemovePressed()
	{
		if (measureTool == null) return;
		var selected = measurementList.GetSelectedItems();
		if (selected.Length > 0)
			measureTool.RemoveMeasurement((int)selected[0]);
	}

	private void OnClearAllPressed()
	{
		if (measureTool == null) return;
		measureTool.ClearAllMeasurements();
	}

	private void OnListItemSelected(long index)
	{
		if (measureTool == null) return;
		removeButton.Disabled = false;
		measureTool.SelectMeasurementForDetails((int)index);
	}

	private void OnListEmptyClicked(Vector2 atPosition, long mouseButtonIndex)
	{
		if (measureTool == null) return;
		removeButton.Disabled = true;
		measureTool.ClearSelection();
	}

}
#endif