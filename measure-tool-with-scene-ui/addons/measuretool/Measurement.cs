#if TOOLS
using Godot;

public class Measurement
{
	public string Name;
	public string NodenameA;
	public string NodenameB;

	public Vector3 vecPointA;
	public Vector3 vecPointB;

	public Vector3 Vector => vecPointB - vecPointA;
	public float Distance => Vector.Length();
	public Vector3 Direction => Vector.Normalized();
	public Vector3 Midpoint => (vecPointA + vecPointB) / 2f;
}
#endif