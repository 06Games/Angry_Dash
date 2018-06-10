using UnityEngine;
using System.Collections;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTEND SIMPLE GESTURE: ----------------------------------------------------------------------------------------------

public partial class SimpleGesture
{
	public static void OnShortTap(GestureDelegate method) 
	{
		if (GestureShortTap._instance == null)
		{
			GestureShortTap gestureShortTap = new GestureShortTap();
			SimpleGesture.Instance.oneFingerGestures.Add(gestureShortTap);
		}
		
		GestureShortTap._instance.AddDelegate(method);
	}
	
	public static void StopShortTap(GestureDelegate method)
	{
		GestureShortTap._instance.RemoveDelegate(method);
		
		if (!GestureShortTap._instance.HasDelegates())
		{
			SimpleGesture.Instance.oneFingerGestures.Remove(GestureShortTap._instance);
			GestureShortTap._instance = null;
		}
	}

	public static void OnShortTap(GestureDelegate<GestureInfoTap> method) 
	{
		if (GestureShortTap._instance == null)
		{
			GestureShortTap gestureShortTap = new GestureShortTap();
			SimpleGesture.Instance.oneFingerGestures.Add(gestureShortTap);
		}
		
		GestureShortTap._instance.AddDelegate(method);
	}
	
	public static void StopShortTap(GestureDelegate<GestureInfoTap> method)
	{
		GestureShortTap._instance.RemoveDelegate(method);
		
		if (!GestureShortTap._instance.HasDelegates())
		{
			SimpleGesture.Instance.oneFingerGestures.Remove(GestureShortTap._instance);
			GestureShortTap._instance = null;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// GESTURE: ------------------------------------------------------------------------------------------------------------

public class GestureShortTap : BaseGesture 
{
	// VARIABLES: ------------------------------------------------------------------------------------------------------
	
	public static GestureShortTap _instance;
	
	protected const float SHORT_TAP_MAX_TIME = 0.5f;
	
	protected SimpleGesture.GestureDelegate broadcastOnTap;
	protected SimpleGesture.GestureDelegate<GestureInfoTap> broadcastOnTapI;
	
	// CONSTRUCTOR: ----------------------------------------------------------------------------------------------------
	
	public GestureShortTap() : base()
	{
		GestureShortTap._instance = this;
	}

	public override void Delete()
	{
		GestureShortTap._instance = null;
	}
	
	// OVERRIDES: ------------------------------------------------------------------------------------------------------
	
	protected override void OnEnded(TouchInfo touchInfo)
	{
		if (this.IsTap(touchInfo))
		{
			if (this.broadcastOnTap != null) this.broadcastOnTap();
			if (this.broadcastOnTapI != null)
			{
				float duration = Time.time - touchInfo.GetStartTime();
				Vector2 position = touchInfo.GetLastPosition();
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
		if (touchInfo.GetTotalDistance() < GestureTap.TAP_MAX_DISTANCE &&
		    Time.time - touchInfo.GetStartTime() < SHORT_TAP_MAX_TIME)
		{
			return true;
		}
		
		return false;
	}
}
