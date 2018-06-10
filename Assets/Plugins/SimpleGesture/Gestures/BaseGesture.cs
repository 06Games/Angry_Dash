using UnityEngine;
using System.Collections;

public abstract class BaseGesture 
{
	// VARIABLES: ------------------------------------------------------------------------------------------------------

	private bool hasBegun = false;

	// PUBLIC METHODS: -------------------------------------------------------------------------------------------------
	
	public void OnPhase(TouchPhase phase, TouchInfo touchInfo)
	{
		switch (phase)
		{
		case TouchPhase.Began:
			this.hasBegun = true;
			this.OnBegin(touchInfo);
			break;
		
		case TouchPhase.Moved:
			if (this.hasBegun)
			{
				this.OnMoved(touchInfo);
			}
			break;

		case TouchPhase.Stationary:
			if (this.hasBegun)
			{
				this.OnStationary(touchInfo);
			}
			break;

		case TouchPhase.Ended:
			if (this.hasBegun)
			{
				this.OnEnded(touchInfo);
				this.hasBegun = false;
			}
			break;

		case TouchPhase.Canceled:
			if (this.hasBegun)
			{
				this.OnCanceled(touchInfo);
				this.hasBegun = false;
			}
			break;
		}
	}

	public void OnPhase(TouchPhase phase, TouchInfo touchInfo1, TouchInfo touchInfo2)
	{
		switch (phase)
		{
		case TouchPhase.Began:
			this.hasBegun = true;
			this.OnBegin(touchInfo1, touchInfo2);
			break;
			
		case TouchPhase.Moved:
			if (this.hasBegun)
			{
				this.OnMoved(touchInfo1, touchInfo2);
			}
			break;
			
		case TouchPhase.Stationary:
			if (this.hasBegun)
			{
				this.OnStationary(touchInfo1, touchInfo2);
			}
			break;
			
		case TouchPhase.Ended:
			if (this.hasBegun)
			{
				this.OnEnded(touchInfo1, touchInfo2);
				this.hasBegun = false;
			}
			break;
			
		case TouchPhase.Canceled:
			if (this.hasBegun)
			{
				this.OnCanceled(touchInfo1, touchInfo2);
				this.hasBegun = false;
			}
			break;
		}
	}

	// ABSTRACT METHODS: -----------------------------------------------------------------------------------------------
	
	public abstract void AddDelegate(SimpleGesture.GestureDelegate method);
	public abstract void RemoveDelegate(SimpleGesture.GestureDelegate method);
	public abstract bool HasDelegates();
	public abstract void Delete();

	protected virtual void OnBegin(TouchInfo touchInfo)      { return; }
	protected virtual void OnMoved(TouchInfo touchInfo)      { return; }
	protected virtual void OnStationary(TouchInfo touchInfo) { return; }
	protected virtual void OnEnded(TouchInfo touchInfo)      { return; }
	protected virtual void OnCanceled(TouchInfo touchInfo)   { return; }

	protected virtual void OnBegin(TouchInfo touchInfo1, TouchInfo touchInfo2)      { return; }
	protected virtual void OnMoved(TouchInfo touchInfo1, TouchInfo touchInfo2)      { return; }
	protected virtual void OnStationary(TouchInfo touchInfo1, TouchInfo touchInfo2) { return; }
	protected virtual void OnEnded(TouchInfo touchInfo1, TouchInfo touchInfo2)      { return; }
	protected virtual void OnCanceled(TouchInfo touchInfo1, TouchInfo touchInfo2)   { return; }
}
