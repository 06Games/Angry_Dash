using UnityEngine;
using System.Collections;

public class GestureInfoZigZag : GestureInfoBase
{
	public Vector2 direction {get; private set; }
	public float distance {get; private set; }
	
	public GestureInfoZigZag(Vector2 direction, float distance) : base(TouchPhase.Ended)
	{
		this.direction = direction;
		this.distance = distance;
	}
}
