using UnityEngine;
using System.Collections;

public class GestureInfoPan : GestureInfoBase
{
	public Vector2 deltaDirection {get; private set;}

	public GestureInfoPan(Vector2 deltaDirection, TouchPhase touchPhase) : base(touchPhase)
	{
		this.deltaDirection = deltaDirection;
	}
}
