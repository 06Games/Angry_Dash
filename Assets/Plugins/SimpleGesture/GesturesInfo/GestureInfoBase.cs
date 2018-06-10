using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GestureInfoBase
{
	public TouchPhase touchPhase;

	public GestureInfoBase(TouchPhase touchPhase)
	{
		this.touchPhase = touchPhase;
	}
}
