using UnityEngine;
using System.Collections;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTEND SIMPLE GESTURE: ----------------------------------------------------------------------------------------------

public partial class SimpleGesture
{
	public static void OnTap(GestureDelegate method) 
	{
		if (GestureTap._instance == null)
		{
			GestureTap gestureTap = new GestureTap();
			SimpleGesture.Instance.oneFingerGestures.Add(gestureTap);
		}

		GestureTap._instance.AddDelegate(method);
	}

	public static void OnTap(GestureDelegate<GestureInfoTap> method) 
	{
		if (GestureTap._instance == null)
		{
			GestureTap gestureTap = new GestureTap();
			SimpleGesture.Instance.oneFingerGestures.Add(gestureTap);
		}
		
		GestureTap._instance.AddDelegate(method);
	}
	
	public static void StopTap(GestureDelegate method)
	{
		GestureTap._instance.RemoveDelegate(method);
		
		if (!GestureTap._instance.HasDelegates())
		{
			SimpleGesture.Instance.oneFingerGestures.Remove(GestureTap._instance);
			GestureTap._instance = null;
		}
	}
	
	public static void StopTap(GestureDelegate<GestureInfoTap> method)
	{
		GestureTap._instance.RemoveDelegate(method);
		
		if (!GestureTap._instance.HasDelegates())
		{
			SimpleGesture.Instance.oneFingerGestures.Remove(GestureTap._instance);
			GestureTap._instance = null;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// GESTURE: ------------------------------------------------------------------------------------------------------------

public class GestureTap : BaseGesture 
{
	// VARIABLES: ------------------------------------------------------------------------------------------------------
	
	public static GestureTap _instance;

	public const float TAP_MAX_DISTANCE = 25.0f;

	protected SimpleGesture.GestureDelegate broadcastOnTap;
	protected SimpleGesture.GestureDelegate<GestureInfoTap> broadcastOnTapI;

	// CONSTRUCTOR: ----------------------------------------------------------------------------------------------------
	
	public GestureTap() : base()
	{
		GestureTap._instance = this;
	}

	public override void Delete()
	{
		GestureTap._instance = null;
	}
	
	// OVERRIDES: ------------------------------------------------------------------------------------------------------
	
	protected override void OnEnded(TouchInfo touchInfo)
	{
		if (this.IsTap(touchInfo))
		{
			if (this.broadcastOnTap  != null) this.broadcastOnTap();
			if (this.broadcastOnTapI != null) 
			{
				float duration = Time.time - touchInfo.GetStartTime();
				Vector2 position = touchInfo.GetFirstPosition();
				GestureInfoTap gesture = new GestureInfoTap(position, duration);
				this.broadcastOnTapI(gesture);
			}
		}
	}
	
	// ADD AND REMOVE METHODS: -----------------------------------------------------------------------------------------
	
	public override void AddDelegate(SimpleGesture.GestureDelegate method)
	{
		this.broadcastOnTap += method;
	}
	
	public override void RemoveDelegate(SimpleGesture.GestureDelegate method)
	{
		this.broadcastOnTap -= method;
	}

	public void AddDelegate(SimpleGesture.GestureDelegate<GestureInfoTap> method)
	{
		this.broadcastOnTapI += method;
	}

	public void RemoveDelegate(SimpleGesture.GestureDelegate<GestureInfoTap> method)
	{
		this.broadcastOnTapI -= method;
	}
	
	public override bool HasDelegates()
	{
		bool del1 = (this.broadcastOnTap  == null ? false : true);
		bool del2 = (this.broadcastOnTapI == null ? false : true);
		return (del1 || del2);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// OTHER METHODS: --------------------------------------------------------------------------------------------------

	private bool IsTap(TouchInfo touchInfo)
	{
		if (touchInfo.GetTotalDistance() < TAP_MAX_DISTANCE)
		{
			return true;
		}

		return false;
	}
}
