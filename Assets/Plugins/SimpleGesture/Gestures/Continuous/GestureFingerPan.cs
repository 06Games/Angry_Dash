using UnityEngine;
using System.Collections;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTEND SIMPLE GESTURE: ----------------------------------------------------------------------------------------------

public partial class SimpleGesture
{
	public static void While1FingerPanning(GestureDelegate method)
	{
		if (GestureFingerPan._instance == null)
		{
			GestureFingerPan gesture1FingerPan = new GestureFingerPan();
			SimpleGesture.Instance.oneFingerGestures.Add(gesture1FingerPan);
		}

		GestureFingerPan._instance.AddDelegate(method);
	}

	public static void Stop1FingerPanning(GestureDelegate method)
	{
		GestureFingerPan._instance.RemoveDelegate(method);
		
		if (!GestureFingerPan._instance.HasDelegates())
		{
			SimpleGesture.Instance.oneFingerGestures.Remove(GestureFingerPan._instance);
			GestureFingerPan._instance = null;
		}
	}

	public static void While1FingerPanning(GestureDelegate<GestureInfoPan> method)
	{
		if (GestureFingerPan._instance == null)
		{
			GestureFingerPan gesture1FingerPan = new GestureFingerPan();
			SimpleGesture.Instance.oneFingerGestures.Add(gesture1FingerPan);
		}
		
		GestureFingerPan._instance.AddDelegate(method);
	}
	
	public static void Stop1FingerPanning(GestureDelegate<GestureInfoPan> method)
	{
		GestureFingerPan._instance.RemoveDelegate(method);
		
		if (!GestureFingerPan._instance.HasDelegates())
		{
			SimpleGesture.Instance.oneFingerGestures.Remove(GestureFingerPan._instance);
			GestureFingerPan._instance = null;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// GESTURE: ------------------------------------------------------------------------------------------------------------

public class GestureFingerPan : BaseGesture 
{
	// VARIABLES: ------------------------------------------------------------------------------------------------------
	
	public static GestureFingerPan _instance;
	
	protected SimpleGesture.GestureDelegate<GestureInfoPan> broadcastWhilePanningI;
	protected SimpleGesture.GestureDelegate broadcastWhilePanning;
	
	// CONSTRUCTOR: ----------------------------------------------------------------------------------------------------
	
	public GestureFingerPan() : base()
	{
		GestureFingerPan._instance = this;
	}

	public override void Delete()
	{
		GestureFingerPan._instance = null;
	}
	
	// OVERRIDES: ------------------------------------------------------------------------------------------------------

	protected override void OnBegin (TouchInfo touchInfo)
	{
		GestureInfoPan gesture = new GestureInfoPan(touchInfo.GetDeltaPosition(), TouchPhase.Began);

		if (this.broadcastWhilePanningI != null) this.broadcastWhilePanningI(gesture);
		if (this.broadcastWhilePanning  != null) this.broadcastWhilePanning();
	}

	protected override void OnMoved(TouchInfo touchInfo)
	{
		GestureInfoPan gesture = new GestureInfoPan(touchInfo.GetDeltaPosition(), TouchPhase.Moved);

		if (this.broadcastWhilePanningI != null) this.broadcastWhilePanningI(gesture);
		if (this.broadcastWhilePanning  != null) this.broadcastWhilePanning();
	}

	protected override void OnEnded (TouchInfo touchInfo)
	{
		GestureInfoPan gesture = new GestureInfoPan(touchInfo.GetDeltaPosition(), TouchPhase.Ended);

		if (this.broadcastWhilePanningI != null) this.broadcastWhilePanningI(gesture);
		if (this.broadcastWhilePanning  != null) this.broadcastWhilePanning();
	}
	
	// ADD AND REMOVE METHODS: -----------------------------------------------------------------------------------------

	public override void AddDelegate(SimpleGesture.GestureDelegate method)
	{
		this.broadcastWhilePanning += method;
	}
	
	public override void RemoveDelegate(SimpleGesture.GestureDelegate method)
	{
		this.broadcastWhilePanning -= method;
	}

	public void AddDelegate(SimpleGesture.GestureDelegate<GestureInfoPan> method)
	{
		this.broadcastWhilePanningI += method;
	}
	
	public void RemoveDelegate(SimpleGesture.GestureDelegate<GestureInfoPan> method)
	{
		this.broadcastWhilePanningI -= method;
	}
	
	public override bool HasDelegates()
	{
		bool del1 = (this.broadcastWhilePanning  == null ? false : true);
		bool del2 = (this.broadcastWhilePanningI == null ? false : true);
		return (del1 || del2);
	}
}