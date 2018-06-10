using UnityEngine;
using System.Collections;

public class GestureInfoCircle : GestureInfoBase
{
	public float radius {get; private set;}
	public Vector2 center {get; private set;}

	public GestureInfoCircle(float radius, Vector2 center) : base(TouchPhase.Ended)
	{
		this.radius = radius;
		this.center = center;
	}
}
