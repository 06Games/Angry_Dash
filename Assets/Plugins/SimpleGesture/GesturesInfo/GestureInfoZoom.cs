using UnityEngine;
using System.Collections;

public class GestureInfoZoom : GestureInfoBase
{
	public float deltaDistance {get; private set; }
	public Vector2 center {get; private set; }

	public Vector2 position1 {get; private set; }
	public Vector2 position2 {get; private set; }

	public GestureInfoZoom(float deltaDistance, Vector2 center, Vector2 position1, Vector2 position2, TouchPhase touchPhase)
		: base(touchPhase)
	{
		this.deltaDistance = deltaDistance;
		this.center = center;
		this.position1 = position1;
		this.position2 = position2;
	}
}
