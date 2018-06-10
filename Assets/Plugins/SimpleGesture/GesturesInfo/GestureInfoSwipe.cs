using UnityEngine;
using System.Collections;

public class GestureInfoSwipe : GestureInfoBase
{
	public Vector2 direction {get; private set; }
	public float distance {get; private set; }

	public Vector2 firstPosition {get; private set; }
	public Vector2 endPosition {get; private set; }
	
	public GestureInfoSwipe(Vector2 direction, float distance, Vector2 firstPosition, Vector2 endPosition, TouchPhase touchPhase) 
		: base(touchPhase)
	{
		this.direction = direction;
		this.distance = distance;
		this.firstPosition = firstPosition;
		this.endPosition = endPosition;
	}
}
