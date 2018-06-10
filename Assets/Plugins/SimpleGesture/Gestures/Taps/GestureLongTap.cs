using UnityEngine;
using System.Collections;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTEND SIMPLE GESTURE: ----------------------------------------------------------------------------------------------

public partial class SimpleGesture
{
	public static void OnLongTap(GestureDelegate method) 
	{
		if (GestureLongTap._instance == null)
		{
			GestureLongTap gestureLongTap = new GestureLongTap();
			SimpleGesture.Instance.oneFingerGestures.Add(gestureLongTap);
		}
		
		GestureLongTap._instance.AddDelegate(method);
	}
	
	public static void StopLongTap(GestureDelegate method)
	{
		GestureLongTap._instance.RemoveDelegate(method);

		if (!GestureLongTap._instance.HasDelegates())
		{
			SimpleGesture.Instance.oneFingerGestures.Remove(GestureLongTap._instance);
			GestureLongTap._instance = null;
		}
	}

	public static void OnLongTap(GestureDelegate<GestureInfoTap> method) 
	{
		if (GestureLongTap._instance == null)
		{
			GestureLongTap gestureLongTap = new GestureLongTap();
			SimpleGesture.Instance.oneFingerGestures.Add(gestureLongTap);
		}
		
		GestureLongTap._instance.AddDelegate(method);
	}
	
	public static void StopLongTap(GestureDelegate<GestureInfoTap> method)
	{
		GestureLongTap._instance.RemoveDelegate(method);
		
		if (!GestureLongTap._instance.HasDelegates())
		{
			SimpleGesture.Instance.oneFingerGestures.Remove(GestureLongTap._instance);
			GestureLongTap._instance = null;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// GESTURE: ------------------------------------------------------------------------------------------------------------

public class GestureLongTap : BaseGesture 
{
	// VARIABLES: ------------------------------------------------------------------------------------------------------
	
	public static GestureLongTap _instance;
	
	protected const float LONG_TAP_MIN_TIME = 0.5f;
	
	protected SimpleGesture.GestureDelegate broadcastOnTap;
	protected SimpleGesture.GestureDelegate<GestureInfoTap> broadcastOnTapI;
	
	// CONSTRUCTOR: ----------------------------------------------------------------------------------------------------
	
	public GestureLongTap() : base()
	{
		GestureLongTap._instance = this;
	}

	public override void Delete()
	{
		GestureLongTap._instance = null;
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
		if (touchInfo.GetTotalDistance() < GestureTap.TAP_MAX_DISTANCE &&
		    Time.time - touchInfo.GetStartTime() > LONG_TAP_MIN_TIME)
		{
			return true;
		}
		
		return false;
	}
}
