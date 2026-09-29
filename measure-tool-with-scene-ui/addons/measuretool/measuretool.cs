#if TOOLS
using Godot;
using System;
using System.Collections.Generic;


[Tool]
public partial class measuretool : EditorPlugin
{



	//=================================UX=======================================================================
	// private VBoxContainer vbox;
	// private Label infoLabel;
	// private Label distanceLabel;
	// private EditorDock dock;
	// private Button measureButton;
	// private LineEdit nameEdit;
	// private Button saveButton;
	// private ItemList measurementList;
	// private Button removeButton;
	// private Button clearAllButton;
	// private Label detailsLabel;

	private MeasureToolUi ui;

	private EditorDock dock;

	//==================================DATA====================================================================
	private bool hasMeasurement;
	private Vector3 measuredA;
	private Vector3 measuredB;
	private string nameA;
	private string nameB;
	private float measuredDis;
	private List<Measurement> measurements = new List<Measurement>();
	private int selectedIndex = -1;

	

	//===================================NODES==========================================================================
	private Node3D pointA;
	private Node3D pointB;
	//===================================MEASUREMENT=========================================================================
	// public class Measurement
	// {
	// 	public string Name;
	// 	public string NodenameA;
	// 	public string NodenameB;

	// 	public Vector3 vecPointA;
	// 	public Vector3 vecPointB;

	// 	public Vector3 Vector => vecPointB - vecPointA;
	// 	public float Distance => Vector.Length();
	// 	public Vector3 Direction => Vector.Normalized();
	// 	public Vector3 Midpoint => (vecPointA + vecPointB)/2f;
		
	// }

	//=================================================================================================================================

	public override void _EnterTree()
	{
		GD.Print("Im in");

		//VBoxContainer automatically tide labels
		// vbox = new VBoxContainer();
		// infoLabel = new Label();
		// distanceLabel = new Label{Text = $"Distance: -"};

		// measureButton = new Button{Text = "Measure"};
		// measureButton.Disabled = true;

		// nameEdit = new LineEdit{PlaceholderText = "Measurement name"};

		// saveButton = new Button{Text = "Save"};
		// saveButton.Disabled = true;

		// measurementList = new ItemList();
		// measurementList.CustomMinimumSize = new Vector2(0, 120);

		// removeButton = new Button{Text = "Remove selected"};
		// removeButton.Disabled = true;

		// clearAllButton = new Button{Text = "Clear ALL"};
		// detailsLabel = new Label{Text = "Select a measurement to see more detail informations"};
		

		// vbox.AddChild(infoLabel);
		// vbox.AddChild(distanceLabel);
		// vbox.AddChild(measureButton);
		// vbox.AddChild(nameEdit);
		// vbox.AddChild(saveButton);
		// vbox.AddChild(measurementList);
		// vbox.AddChild(removeButton);
		// vbox.AddChild(clearAllButton);
		// vbox.AddChild(detailsLabel);
		
		// dock = new EditorDock();
		// dock.Title = "MeasureTool";
		// dock.DefaultSlot = EditorDock.DockSlot.RightUl;
		// dock.AddChild(vbox);

		dock = new EditorDock();
		dock.Title = "Measure Tool";
		dock.DefaultSlot = EditorDock.DockSlot.RightUl;
		ui = GD.Load<PackedScene>("res://addons/measuretool/measure_tool_ui.tscn").Instantiate<MeasureToolUi>();
		ui.measureTool = this;
		dock.AddChild(ui);
		AddDock(dock);

		//enable process
		SetForceDrawOverForwardingEnabled();
		SetProcess(true);
		


		//get the object infos
		var selection = EditorInterface.Singleton.GetSelection();
		selection.SelectionChanged += OnSelectionChanged;
		// measureButton.Pressed += OnMeasureButtonClicked;
		// saveButton.Pressed += OnSaveButtonClicked;
		// removeButton.Pressed += OnRemoveButtonClicked;
		// clearAllButton.Pressed += OnClearAllButtonClicked;
		// measurementList.ItemSelected += OnListItemSelected;

	}

	public override void _ExitTree()
	{
		GD.Print("Im out");
		var selection = EditorInterface.Singleton.GetSelection();
		selection.SelectionChanged -= OnSelectionChanged;
		// measureButton.Pressed -= OnMeasureButtonClicked;
		// saveButton.Pressed -= OnSaveButtonClicked;
		// removeButton.Pressed -= OnRemoveButtonClicked;
		// clearAllButton.Pressed -= OnClearAllButtonClicked;
		// measurementList.ItemSelected -= OnListItemSelected;
		
		RemoveDock(dock);
		dock.Free();
	}


    public override void _Process(double delta)
    {
        UpdateOverlays();
    }


	private void OnSelectionChanged()
	{
		List<Node3D> N = new List<Node3D>();
		var nodes = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
		
		foreach(var node in nodes)
		{
			if(node is Node3D n3d)
			{
				N.Add(n3d);
			}
		}

		int NodesNum = N.Count;

		if(NodesNum == 0)
		{
			ui.SetInfoText("No Node3D selected.\nSelect one object to inspect it,\nor two to measure the distance between them.");
		}
		else if(NodesNum ==1)
		{
			var objA = N[0];
			ui.SetInfoText($"Selected:{objA.Name}\nNode Position:{objA.GlobalPosition}");
			
		}
		else if (NodesNum ==2)
		{
			var objA = N[0];
			var objB = N[1];
			ui.SetInfoText($"Selected A:{objA.Name}\nNode Position:{objA.GlobalPosition}\n\n"+
						   $"Selected B:{objB.Name}\nNode Position:{objB.GlobalPosition}");



			pointA = objA;
			pointB = objB;
		}
		else
		{
			ui.SetInfoText("Selected too many nodes");
		}

		if (NodesNum != 2)
    		ui.SetDistanceText("Distance: -");

		ui.SetMeasureButtonEnabled(NodesNum == 2);

	}


    public override void _Forward3DForceDrawOverViewport(Control viewportControl)
    {

		Camera3D camera = EditorInterface.Singleton.GetEditorViewport3D(0).GetCamera3D();
		if(camera == null) return;

		if(hasMeasurement)
		{
			Vector2 screenA = camera.UnprojectPosition(measuredA);
			Vector2 screenB = camera.UnprojectPosition(measuredB);
			viewportControl.DrawLine(screenA,screenB,Colors.Yellow, 2.0f, true);
			
			Vector2 mid = screenA + (screenB - screenA) * 0.35f;
			viewportControl.DrawString(ThemeDB.FallbackFont, mid, $"{measuredDis:F2}", HorizontalAlignment.Left, -1, 16, Colors.Yellow);
		}	
	

		if(selectedIndex>=0 && selectedIndex < measurements.Count)
		{
			var m = measurements[selectedIndex];
			Vector2 sA = camera.UnprojectPosition(m.vecPointA);
			Vector2 sB = camera.UnprojectPosition(m.vecPointB);
			viewportControl.DrawLine(sA, sB, Colors.Cyan,2.0f,true);
			Vector2 midP = sA + (sB - sA) * 0.65f;
			viewportControl.DrawString(ThemeDB.FallbackFont, midP, $"{m.Name}  {m.Distance:F2}", HorizontalAlignment.Left, -1, 16, Colors.Cyan);
		}


		
    }

	public float MeasureCurrentPair()
	{
		if (pointA == null || pointB == null) return 0f;
    	if (!GodotObject.IsInstanceValid(pointA) || !GodotObject.IsInstanceValid(pointB)) return 0f;

		measuredA = pointA.GlobalPosition;
		measuredB = pointB.GlobalPosition;
		nameA = pointA.Name;
		nameB = pointB.Name;

		hasMeasurement = true;

		Vector3 line = measuredB - measuredA;
		measuredDis = line.Length();

		ui.SetDistanceText($"Distance:{measuredDis:F2}");

		ui.SetSaveButtonEnabled(true);
		return measuredDis;
	}

	public void SaveMeasurement(string raw)
	{
		string fileName = string.IsNullOrWhiteSpace(raw) ? $"Measurement{measurements.Count + 1}":raw; // A? B:C == if(A)->B, else ->C 
		
		string candidate = fileName;
		int i = 1;
		while (measurements.Exists(m => m.Name == candidate))
			candidate = $"{fileName}{i++}";
		
		measurements.Add(new Measurement
		{
			Name = candidate,
			NodenameA = nameA,
			NodenameB = nameB,
			vecPointA = measuredA,
			vecPointB = measuredB,
		});

		ui.SetSaveButtonEnabled(false);
		ui.RefreshMeasurementList(measurements);
	}

	public void RemoveMeasurement(int index)
	{
		if (index < 0 || index >= measurements.Count) return;
		measurements.RemoveAt(index);
		selectedIndex = -1;
		ui.RefreshMeasurementList(measurements);
		ui.ClearDetails();
	}

	public void ClearAllMeasurements()
	{
		measurements.Clear();
		selectedIndex = -1;
		ui.RefreshMeasurementList(measurements);
		ui.ClearDetails();
	}

	public void SelectMeasurementForDetails(int index)
	{
		selectedIndex = index;
		var m = measurements[selectedIndex];

		bool isSameLine =(m.vecPointA == measuredA && m.vecPointB == measuredB);
		hasMeasurement = !isSameLine;

		string equation =
		$"P(t) = ({m.vecPointA.X:F2}, {m.vecPointA.Y:F2}, {m.vecPointA.Z:F2}) + " +
		$"t * ({m.Direction.X:F2}, {m.Direction.Y:F2}, {m.Direction.Z:F2}), t in [0,1]";

		ui.ShowDetails(m.Name, m.NodenameA, m.vecPointA, m.NodenameB, m.vecPointB,
			m.Distance, m.Vector, m.Direction, equation);
	}

	public void ClearSelection()
	{
		selectedIndex = -1;
		hasMeasurement = true;    // 之前若因选中"同一条"让黄线让了位,取消点选后恢复显示
		ui.ClearDetails();
	}



}
#endif
