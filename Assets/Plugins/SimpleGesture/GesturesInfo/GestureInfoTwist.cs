using UnityEngine;
using System.Collections;

public class GestureInfoTwist : GestureInfoBase
{
	public Vector2 center {get; private set;}
	public float deltaDistance {get; private set;}

	public Vector2 position1 {get; private set;}
	public Vector2 position2 {get; private set;}

	public bool clockwise {get; private set;}

	public GestureInfoTwist(Vector2 center, float deltaDistance, Vector2 position1, Vector2 position2, bool clockwise, TouchPhase touchPhase)
		: base(touchPhase)
	{
		this.center = center;
		this.deltaDistance = deltaDistance;
		this.position1 = position1;
		this.position2 = position2;
		this.clockwise = clockwise;
	}
}
