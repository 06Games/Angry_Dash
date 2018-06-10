using UnityEngine;
using System.Collections;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTEND SIMPLE GESTURE: ----------------------------------------------------------------------------------------------

public partial class SimpleGesture
{
	public static void On4AxisSwipeLeft(GestureDelegate method)    { SimpleGesture.Swipe4Axis_AddMethod(E_DIRECTION.LFT, method); }
	public static void On4AxisSwipeRight(GestureDelegate method)   { SimpleGesture.Swipe4Axis_AddMethod(E_DIRECTION.RHT, method); }
	public static void On4AxisSwipeUp(GestureDelegate method)      { SimpleGesture.Swipe4Axis_AddMethod(E_DIRECTION.UPR, method); }
	public static void On4AxisSwipeDown(GestureDelegate method)    { SimpleGesture.Swipe4Axis_AddMethod(E_DIRECTION.DWN, method); }
	
	public static void Stop4AxisSwipeLeft(GestureDelegate method)  { SimpleGesture.Swipe4Axis_SubsMethod(E_DIRECTION.LFT, method); }
	public static void Stop4AxisSwipeRight(GestureDelegate method) { SimpleGesture.Swipe4Axis_SubsMethod(E_DIRECTION.RHT, method); }
	public static void Stop4AxisSwipeUp(GestureDelegate method)    { SimpleGesture.Swipe4Axis_SubsMethod(E_DIRECTION.UPR, method); }
	public static void Stop4AxisSwipeDown(GestureDelegate method)  { SimpleGesture.Swipe4Axis_SubsMethod(E_DIRECTION.DWN, method); }

	public static void On4AxisSwipeLeft(GestureDelegate<GestureInfoSwipe> method)    { SimpleGesture.Swipe4Axis_AddMethod(E_DIRECTION.LFT, method); }
	public static void On4AxisSwipeRight(GestureDelegate<GestureInfoSwipe> method)   { SimpleGesture.Swipe4Axis_AddMethod(E_DIRECTION.RHT, method); }
	public static void On4AxisSwipeUp(GestureDelegate<GestureInfoSwipe> method)      { SimpleGesture.Swipe4Axis_AddMethod(E_DIRECTION.UPR, method); }
	public static void On4AxisSwipeDown(GestureDelegate<GestureInfoSwipe> method)    { SimpleGesture.Swipe4Axis_AddMethod(E_DIRECTION.DWN, method); }
	
	public static void Stop4AxisSwipeLeft(GestureDelegate<GestureInfoSwipe> method)  { SimpleGesture.Swipe4Axis_SubsMethod(E_DIRECTION.LFT, method); }
	public static void Stop4AxisSwipeRight(GestureDelegate<GestureInfoSwipe> method) { SimpleGesture.Swipe4Axis_SubsMethod(E_DIRECTION.RHT, method); }
	public static void Stop4AxisSwipeUp(GestureDelegate<GestureInfoSwipe> method)    { SimpleGesture.Swipe4Axis_SubsMethod(E_DIRECTION.UPR, method); }
	public static void Stop4AxisSwipeDown(GestureDelegate<GestureInfoSwipe> method)  { SimpleGesture.Swipe4Axis_SubsMethod(E_DIRECTION.DWN, method); }

	private static void Swipe4Axis_AddMethod(E_DIRECTION type, GestureDelegate method)
	{
		if (Gesture4AxisSwipe._instance == null)
		{
			Gesture4AxisSwipe gestureSwipe = new Gesture4AxisSwipe();
			SimpleGesture.Instance.oneFingerGestures.Add(gestureSwipe);
		}

		Gesture4AxisSwipe._instance.SetType(type);
		Gesture4AxisSwipe._instance.AddDelegate(method);
	}

	private static void Swipe4Axis_SubsMethod(E_DIRECTION type, GestureDelegate method)
	{
		Gesture4AxisSwipe._instance.SetType(type);
		Gesture4AxisSwipe._instance.RemoveDelegate(method);

		if (!Gesture4AxisSwipe._instance.HasDelegates())
		{
			SimpleGesture.Instance.oneFingerGestures.Remove(Gesture4AxisSwipe._instance);
			Gesture4AxisSwipe._instance = null;
		}
	}

	private static void Swipe4Axis_AddMethod(E_DIRECTION type, GestureDelegate<GestureInfoSwipe> method)
	{
		if (Gesture4AxisSwipe._instance == null)
		{
			Gesture4AxisSwipe gestureSwipe = new Gesture4AxisSwipe();
			SimpleGesture.Instance.oneFingerGestures.Add(gestureSwipe);
		}
		
		Gesture4AxisSwipe._instance.SetType(type);
		Gesture4AxisSwipe._instance.AddDelegate(method);
	}
	
	private static void Swipe4Axis_SubsMethod(E_DIRECTION type, GestureDelegate<GestureInfoSwipe> method)
	{
		Gesture4AxisSwipe._instance.SetType(type);
		Gesture4AxisSwipe._instance.RemoveDelegate(method);
		
		if (!Gesture4AxisSwipe._instance.HasDelegates())
		{
			SimpleGesture.Instance.oneFingerGestures.Remove(Gesture4AxisSwipe._instance);
			Gesture4AxisSwipe._instance = null;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// GESTURE: ------------------------------------------------------------------------------------------------------------

public class Gesture4AxisSwipe : BaseGesture 
{
	// VARIABLES: ------------------------------------------------------------------------------------------------------

	public static Gesture4AxisSwipe _instance;

	protected const float SWIPE_MIN_DISTANCE = 30.0f; // minimum distance in pixels to detect a swipe gesture
	protected const float SWIPE_ERR_DISTANCE = 0.05f; // maximum error percent between the ideal distance and current
	
	private SimpleGesture.GestureDelegate broadcastOn4AxisSwipeLFT;
	private SimpleGesture.GestureDelegate broadcastOn4AxisSwipeRHT;
	private SimpleGesture.GestureDelegate broadcastOn4AxisSwipeUPR;
	private SimpleGesture.GestureDelegate broadcastOn4AxisSwipeDWN;

	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn4AxisSwipeLFTI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn4AxisSwipeRHTI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn4AxisSwipeUPRI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn4AxisSwipeDWNI;

	private SimpleGesture.E_DIRECTION tmp_type;

	// CONSTRUCTOR: ----------------------------------------------------------------------------------------------------

	public Gesture4AxisSwipe() : base()
	{
		Gesture4AxisSwipe._instance = this;
	}

	public override void Delete()
	{
		Gesture4AxisSwipe._instance = null;
	}

	// OVERRIDES: ------------------------------------------------------------------------------------------------------

	protected override void OnEnded(TouchInfo touchInfo)
	{
		if (this.IsSwipe(touchInfo))
		{
			SimpleGesture.GestureDelegate method = this.GetDelegate(touchInfo);
			if (method != null) method();

			SimpleGesture.GestureDelegate<GestureInfoSwipe> methodI = this.GetDelegateI(touchInfo);
			if (methodI != null)
			{
				Vector2 pos1 = touchInfo.GetFirstPosition();
				Vector2 pos2 = touchInfo.GetLastPosition();
				Vector2 direction = pos2 - pos1;
				float distance = direction.magnitude;
				GestureInfoSwipe gesture = new GestureInfoSwipe(direction, distance, pos1, pos2, TouchPhase.Ended);
				methodI(gesture);
			}
		}
	}

	// ADD AND REMOVE METHODS: -----------------------------------------------------------------------------------------

	public override void AddDelegate(SimpleGesture.GestureDelegate method)
	{
		switch (this.tmp_type)
		{
		case SimpleGesture.E_DIRECTION.LFT : this.broadcastOn4AxisSwipeLFT += method; break;
		case SimpleGesture.E_DIRECTION.RHT : this.broadcastOn4AxisSwipeRHT += method; break;
		case SimpleGesture.E_DIRECTION.UPR : this.broadcastOn4AxisSwipeUPR += method; break;
		case SimpleGesture.E_DIRECTION.DWN : this.broadcastOn4AxisSwipeDWN += method; break;
		default : Debug.LogError(SimpleGesture.ERROR_ENUM_DOES_NOT_CORRESPOND); break;
		}
	}

	public override void RemoveDelegate(SimpleGesture.GestureDelegate method)
	{
		switch (this.tmp_type)
		{
		case SimpleGesture.E_DIRECTION.LFT : this.broadcastOn4AxisSwipeLFT -= method; break;
		case SimpleGesture.E_DIRECTION.RHT : this.broadcastOn4AxisSwipeRHT -= method; break;
		case SimpleGesture.E_DIRECTION.UPR : this.broadcastOn4AxisSwipeUPR -= method; break;
		case SimpleGesture.E_DIRECTION.DWN : this.broadcastOn4AxisSwipeDWN -= method; break;
		default : Debug.LogError(SimpleGesture.ERROR_ENUM_DOES_NOT_CORRESPOND); break;
		}
	}

	public void AddDelegate(SimpleGesture.GestureDelegate<GestureInfoSwipe> method)
	{
		switch (this.tmp_type)
		{
		case SimpleGesture.E_DIRECTION.LFT : this.broadcastOn4AxisSwipeLFTI += method; break;
		case SimpleGesture.E_DIRECTION.RHT : this.broadcastOn4AxisSwipeRHTI += method; break;
		case SimpleGesture.E_DIRECTION.UPR : this.broadcastOn4AxisSwipeUPRI += method; break;
		case SimpleGesture.E_DIRECTION.DWN : this.broadcastOn4AxisSwipeDWNI += method; break;
		default : Debug.LogError(SimpleGesture.ERROR_ENUM_DOES_NOT_CORRESPOND); break;
		}
	}
	
	public void RemoveDelegate(SimpleGesture.GestureDelegate<GestureInfoSwipe> method)
	{
		switch (this.tmp_type)
		{
		case SimpleGesture.E_DIRECTION.LFT : this.broadcastOn4AxisSwipeLFTI -= method; break;
		case SimpleGesture.E_DIRECTION.RHT : this.broadcastOn4AxisSwipeRHTI -= method; break;
		case SimpleGesture.E_DIRECTION.UPR : this.broadcastOn4AxisSwipeUPRI -= method; break;
		case SimpleGesture.E_DIRECTION.DWN : this.broadcastOn4AxisSwipeDWNI -= method; break;
		default : Debug.LogError(SimpleGesture.ERROR_ENUM_DOES_NOT_CORRESPOND); break;
		}
	}

	public override bool HasDelegates()
	{
		if (this.broadcastOn4AxisSwipeLFT  == null &&
		    this.broadcastOn4AxisSwipeRHT  == null &&
		    this.broadcastOn4AxisSwipeUPR  == null &&
		    this.broadcastOn4AxisSwipeDWN  == null &&
		    this.broadcastOn4AxisSwipeLFTI == null &&
		    this.broadcastOn4AxisSwipeRHTI == null &&
		    this.broadcastOn4AxisSwipeUPRI == null &&
		    this.broadcastOn4AxisSwipeDWNI == null)
		{
			return false;
		}

		return true;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// OTHER METHODS: --------------------------------------------------------------------------------------------------

	public void SetType(SimpleGesture.E_DIRECTION type)
	{
		this.tmp_type = type;
	}

	private bool IsSwipe(TouchInfo touchInfo)
	{
		float curDistance = touchInfo.GetTotalDistance();
		float minDistance = Vector2.Distance(touchInfo.GetFirstPosition(), touchInfo.GetLastPosition());

		if (curDistance < SWIPE_MIN_DISTANCE) return false;
		if (curDistance - (SWIPE_ERR_DISTANCE * minDistance) > minDistance) return false;
		return true;
	}

	private SimpleGesture.GestureDelegate GetDelegate(TouchInfo touchInfo)
	{
		Vector2 direction = (touchInfo.GetLastPosition() - touchInfo.GetFirstPosition()).normalized;
		float angle = Vector2.Angle(direction, Vector2.right);

		if (angle <= 45.0f)
		{
			return this.broadcastOn4AxisSwipeRHT;
		}
		else if (angle >= 135.0f)
		{
			return this.broadcastOn4AxisSwipeLFT;
		}
		else if (direction.y > 0.0f)
		{
			return this.broadcastOn4AxisSwipeUPR;
		}
		else 
		{
			return this.broadcastOn4AxisSwipeDWN;
		}
	}

	private SimpleGesture.GestureDelegate<GestureInfoSwipe> GetDelegateI(TouchInfo touchInfo)
	{
		Vector2 direction = (touchInfo.GetLastPosition() - touchInfo.GetFirstPosition()).normalized;
		float angle = Vector2.Angle(direction, Vector2.right);
		
		if (angle <= 45.0f)
		{
			return this.broadcastOn4AxisSwipeRHTI;
		}
		else if (angle >= 135.0f)
		{
			return this.broadcastOn4AxisSwipeLFTI;
		}
		else if (direction.y > 0.0f)
		{
			return this.broadcastOn4AxisSwipeUPRI;
		}
		else 
		{
			return this.broadcastOn4AxisSwipeDWNI;
		}
	}
}
