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
	[Export] Label detailsLabel;


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
			$"[{name}]\n" +
			$"A: {nodeAName}  ({pointA.X:F2}, {pointA.Y:F2}, {pointA.Z:F2})\n" +
			$"B: {nodeBName}  ({pointB.X:F2}, {pointB.Y:F2}, {pointB.Z:F2})\n" +
			$"Distance: {distance:F3}\n" +
			$"Vector (B - A): ({vector.X:F2}, {vector.Y:F2}, {vector.Z:F2})\n" +
			$"Unit direction: ({direction.X:F2}, {direction.Y:F2}, {direction.Z:F2})\n" +
			"Line equation:\n" + equation;
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

}
#endif