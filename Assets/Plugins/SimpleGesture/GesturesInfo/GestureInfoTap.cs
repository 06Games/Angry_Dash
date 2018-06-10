using UnityEngine;
using System.Collections;

public class GestureInfoTap : GestureInfoBase
{
	public Vector2 position {get; private set; }
	public float duration {get; private set; }

	public GestureInfoTap(Vector2 position, float duration) : base(TouchPhase.Ended)
	{
		this.position = position;
		this.duration = duration;
	}
}
