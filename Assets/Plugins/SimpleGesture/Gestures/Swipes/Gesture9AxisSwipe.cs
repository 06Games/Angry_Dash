using UnityEngine;
using System.Collections;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTEND SIMPLE GESTURE: ----------------------------------------------------------------------------------------------

public partial class SimpleGesture
{
	public static void OnSwipe(GestureDelegate method)                        { SimpleGesture.Swipe9Axis_AddMethod(E_DIRECTION.ANY, method); }
	public static void On9AxisSwipeLeft(GestureDelegate method)               { SimpleGesture.Swipe9Axis_AddMethod(E_DIRECTION.LFT, method); }
	public static void On9AxisSwipeRight(GestureDelegate method)              { SimpleGesture.Swipe9Axis_AddMethod(E_DIRECTION.RHT, method); }
	public static void On9AxisSwipeUp(GestureDelegate method)                 { SimpleGesture.Swipe9Axis_AddMethod(E_DIRECTION.UPR, method); }
	public static void On9AxisSwipeDown(GestureDelegate method)               { SimpleGesture.Swipe9Axis_AddMethod(E_DIRECTION.DWN, method); }
	public static void On9AxisSwipeDiagonalRightUp(GestureDelegate method)    { SimpleGesture.Swipe9Axis_AddMethod(E_DIRECTION.DUR, method); }
	public static void On9AxisSwipeDiagonalRightDown(GestureDelegate method)  { SimpleGesture.Swipe9Axis_AddMethod(E_DIRECTION.DDR, method); }
	public static void On9AxisSwipeDiagonalLeftUp(GestureDelegate method)     { SimpleGesture.Swipe9Axis_AddMethod(E_DIRECTION.DUL, method); }
	public static void On9AxisSwipeDiagonalLeftDown(GestureDelegate method)   { SimpleGesture.Swipe9Axis_AddMethod(E_DIRECTION.DDL, method); }

	public static void StopSwipe(GestureDelegate method)                        { SimpleGesture.SwipeQuick_SubsMethod(E_DIRECTION.ANY, method); }
	public static void Stop9AxisSwipeLeft(GestureDelegate method)               { SimpleGesture.SwipeQuick_SubsMethod(E_DIRECTION.LFT, method); }
	public static void Stop9AxisSwipeRight(GestureDelegate method)              { SimpleGesture.SwipeQuick_SubsMethod(E_DIRECTION.RHT, method); }
	public static void Stop9AxisSwipeUp(GestureDelegate method)                 { SimpleGesture.SwipeQuick_SubsMethod(E_DIRECTION.UPR, method); }
	public static void Stop9AxisSwipeDown(GestureDelegate method)               { SimpleGesture.SwipeQuick_SubsMethod(E_DIRECTION.DWN, method); }
	public static void Stop9AxisSwipeDiagonalRightUp(GestureDelegate method)    { SimpleGesture.SwipeQuick_SubsMethod(E_DIRECTION.DUR, method); }
	public static void Stop9AxisSwipeDiagonalRightDown(GestureDelegate method)  { SimpleGesture.SwipeQuick_SubsMethod(E_DIRECTION.DDR, method); }
	public static void Stop9AxisSwipeDiagonalLeftUp(GestureDelegate method)     { SimpleGesture.SwipeQuick_SubsMethod(E_DIRECTION.DUL, method); }
	public static void Stop9AxisSwipeDiagonalLeftDown(GestureDelegate method)   { SimpleGesture.SwipeQuick_SubsMethod(E_DIRECTION.DDL, method); }

	public static void OnSwipe(GestureDelegate<GestureInfoSwipe> method)                        { SimpleGesture.Swipe9Axis_AddMethod(E_DIRECTION.ANY, method); }
	public static void On9AxisSwipeLeft(GestureDelegate<GestureInfoSwipe> method)               { SimpleGesture.Swipe9Axis_AddMethod(E_DIRECTION.LFT, method); }
	public static void On9AxisSwipeRight(GestureDelegate<GestureInfoSwipe> method)              { SimpleGesture.Swipe9Axis_AddMethod(E_DIRECTION.RHT, method); }
	public static void On9AxisSwipeUp(GestureDelegate<GestureInfoSwipe> method)                 { SimpleGesture.Swipe9Axis_AddMethod(E_DIRECTION.UPR, method); }
	public static void On9AxisSwipeDown(GestureDelegate<GestureInfoSwipe> method)               { SimpleGesture.Swipe9Axis_AddMethod(E_DIRECTION.DWN, method); }
	public static void On9AxisSwipeDiagonalRightUp(GestureDelegate<GestureInfoSwipe> method)    { SimpleGesture.Swipe9Axis_AddMethod(E_DIRECTION.DUR, method); }
	public static void On9AxisSwipeDiagonalRightDown(GestureDelegate<GestureInfoSwipe> method)  { SimpleGesture.Swipe9Axis_AddMethod(E_DIRECTION.DDR, method); }
	public static void On9AxisSwipeDiagonalLeftUp(GestureDelegate<GestureInfoSwipe> method)     { SimpleGesture.Swipe9Axis_AddMethod(E_DIRECTION.DUL, method); }
	public static void On9AxisSwipeDiagonalLeftDown(GestureDelegate<GestureInfoSwipe> method)   { SimpleGesture.Swipe9Axis_AddMethod(E_DIRECTION.DDL, method); }
	
	public static void StopSwipe(GestureDelegate<GestureInfoSwipe> method)                        { SimpleGesture.SwipeQuick_SubsMethod(E_DIRECTION.ANY, method); }
	public static void Stop9AxisSwipeLeft(GestureDelegate<GestureInfoSwipe> method)               { SimpleGesture.SwipeQuick_SubsMethod(E_DIRECTION.LFT, method); }
	public static void Stop9AxisSwipeRight(GestureDelegate<GestureInfoSwipe> method)              { SimpleGesture.SwipeQuick_SubsMethod(E_DIRECTION.RHT, method); }
	public static void Stop9AxisSwipeUp(GestureDelegate<GestureInfoSwipe> method)                 { SimpleGesture.SwipeQuick_SubsMethod(E_DIRECTION.UPR, method); }
	public static void Stop9AxisSwipeDown(GestureDelegate<GestureInfoSwipe> method)               { SimpleGesture.SwipeQuick_SubsMethod(E_DIRECTION.DWN, method); }
	public static void Stop9AxisSwipeDiagonalRightUp(GestureDelegate<GestureInfoSwipe> method)    { SimpleGesture.SwipeQuick_SubsMethod(E_DIRECTION.DUR, method); }
	public static void Stop9AxisSwipeDiagonalRightDown(GestureDelegate<GestureInfoSwipe> method)  { SimpleGesture.SwipeQuick_SubsMethod(E_DIRECTION.DDR, method); }
	public static void Stop9AxisSwipeDiagonalLeftUp(GestureDelegate<GestureInfoSwipe> method)     { SimpleGesture.SwipeQuick_SubsMethod(E_DIRECTION.DUL, method); }
	public static void Stop9AxisSwipeDiagonalLeftDown(GestureDelegate<GestureInfoSwipe> method)   { SimpleGesture.SwipeQuick_SubsMethod(E_DIRECTION.DDL, method); }

	private static void Swipe9Axis_AddMethod(E_DIRECTION type, GestureDelegate method)
	{
		if (Gesture9AxisSwipe._instance == null)
		{
			Gesture9AxisSwipe gestureSwipe = new Gesture9AxisSwipe();
			SimpleGesture.Instance.oneFingerGestures.Add(gestureSwipe);
		}

		Gesture9AxisSwipe._instance.SetType(type);
		Gesture9AxisSwipe._instance.AddDelegate(method);
	}

	private static void SwipeQuick_SubsMethod(E_DIRECTION type, GestureDelegate method)
	{
		Gesture9AxisSwipe._instance.SetType(type);
		Gesture9AxisSwipe._instance.RemoveDelegate(method);

		if (!Gesture9AxisSwipe._instance.HasDelegates())
		{
			SimpleGesture.Instance.oneFingerGestures.Remove(Gesture9AxisSwipe._instance);
			Gesture9AxisSwipe._instance = null;
		}
	}

	private static void Swipe9Axis_AddMethod(E_DIRECTION type, GestureDelegate<GestureInfoSwipe> method)
	{
		if (Gesture9AxisSwipe._instance == null)
		{
			Gesture9AxisSwipe gestureSwipe = new Gesture9AxisSwipe();
			SimpleGesture.Instance.oneFingerGestures.Add(gestureSwipe);
		}
		
		Gesture9AxisSwipe._instance.SetType(type);
		Gesture9AxisSwipe._instance.AddDelegate(method);
	}
	
	private static void SwipeQuick_SubsMethod(E_DIRECTION type, GestureDelegate<GestureInfoSwipe> method)
	{
		Gesture9AxisSwipe._instance.SetType(type);
		Gesture9AxisSwipe._instance.RemoveDelegate(method);
		
		if (!Gesture9AxisSwipe._instance.HasDelegates())
		{
			SimpleGesture.Instance.oneFingerGestures.Remove(Gesture9AxisSwipe._instance);
			Gesture9AxisSwipe._instance = null;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// GESTURE: ------------------------------------------------------------------------------------------------------------

public class Gesture9AxisSwipe : BaseGesture 
{
	// VARIABLES: ------------------------------------------------------------------------------------------------------

	public static Gesture9AxisSwipe _instance;

	protected const float SWIPE_MIN_DISTANCE = 30.0f; // minimum distance in pixels to detect a swipe gesture
	protected const float SWIPE_ERR_DISTANCE = 0.05f; // maximum error percent between the ideal distance and current

	private SimpleGesture.GestureDelegate broadcastOn9AxisSwipeANY;
	private SimpleGesture.GestureDelegate broadcastOn9AxisSwipeLFT;
	private SimpleGesture.GestureDelegate broadcastOn9AxisSwipeRHT;
	private SimpleGesture.GestureDelegate broadcastOn9AxisSwipeUPR;
	private SimpleGesture.GestureDelegate broadcastOn9AxisSwipeDWN;
	private SimpleGesture.GestureDelegate broadcastOn9AxisSwipeDUR;
	private SimpleGesture.GestureDelegate broadcastOn9AxisSwipeDDR;
	private SimpleGesture.GestureDelegate broadcastOn9AxisSwipeDUL;
	private SimpleGesture.GestureDelegate broadcastOn9AxisSwipeDDL;

	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn9AxisSwipeANYI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn9AxisSwipeLFTI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn9AxisSwipeRHTI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn9AxisSwipeUPRI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn9AxisSwipeDWNI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn9AxisSwipeDURI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn9AxisSwipeDDRI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn9AxisSwipeDULI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn9AxisSwipeDDLI;

	private SimpleGesture.E_DIRECTION tmp_type;

	// CONSTRUCTOR: ----------------------------------------------------------------------------------------------------

	public Gesture9AxisSwipe() : base()
	{
		Gesture9AxisSwipe._instance = this;
	}

	public override void Delete()
	{
		Gesture9AxisSwipe._instance = null;
	}

	// OVERRIDES: ------------------------------------------------------------------------------------------------------

	protected override void OnEnded(TouchInfo touchInfo)
	{
		if (this.IsSwipe(touchInfo))
		{
			if (this.broadcastOn9AxisSwipeANY != null)
			{
				this.broadcastOn9AxisSwipeANY();
			}

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
		case SimpleGesture.E_DIRECTION.ANY : this.broadcastOn9AxisSwipeANY += method; break;
		case SimpleGesture.E_DIRECTION.LFT : this.broadcastOn9AxisSwipeLFT += method; break;
		case SimpleGesture.E_DIRECTION.RHT : this.broadcastOn9AxisSwipeRHT += method; break;
		case SimpleGesture.E_DIRECTION.UPR : this.broadcastOn9AxisSwipeUPR += method; break;
		case SimpleGesture.E_DIRECTION.DWN : this.broadcastOn9AxisSwipeDWN += method; break;
		case SimpleGesture.E_DIRECTION.DDR : this.broadcastOn9AxisSwipeDDR += method; break;
		case SimpleGesture.E_DIRECTION.DUR : this.broadcastOn9AxisSwipeDUR += method; break;
		case SimpleGesture.E_DIRECTION.DDL : this.broadcastOn9AxisSwipeDDL += method; break;
		case SimpleGesture.E_DIRECTION.DUL : this.broadcastOn9AxisSwipeDUL += method; break;
		default : Debug.LogError(SimpleGesture.ERROR_ENUM_DOES_NOT_CORRESPOND); break;
		}
	}

	public override void RemoveDelegate(SimpleGesture.GestureDelegate method)
	{
		switch (this.tmp_type)
		{
		case SimpleGesture.E_DIRECTION.ANY : this.broadcastOn9AxisSwipeANY -= method; break;
		case SimpleGesture.E_DIRECTION.LFT : this.broadcastOn9AxisSwipeLFT -= method; break;
		case SimpleGesture.E_DIRECTION.RHT : this.broadcastOn9AxisSwipeRHT -= method; break;
		case SimpleGesture.E_DIRECTION.UPR : this.broadcastOn9AxisSwipeUPR -= method; break;
		case SimpleGesture.E_DIRECTION.DWN : this.broadcastOn9AxisSwipeDWN -= method; break;
		case SimpleGesture.E_DIRECTION.DDR : this.broadcastOn9AxisSwipeDDR -= method; break;
		case SimpleGesture.E_DIRECTION.DUR : this.broadcastOn9AxisSwipeDUR -= method; break;
		case SimpleGesture.E_DIRECTION.DDL : this.broadcastOn9AxisSwipeDDL -= method; break;
		case SimpleGesture.E_DIRECTION.DUL : this.broadcastOn9AxisSwipeDUL -= method; break;
		default : Debug.LogError(SimpleGesture.ERROR_ENUM_DOES_NOT_CORRESPOND); break;
		}
	}

	public void AddDelegate(SimpleGesture.GestureDelegate<GestureInfoSwipe> method)
	{
		switch (this.tmp_type)
		{
		case SimpleGesture.E_DIRECTION.ANY : this.broadcastOn9AxisSwipeANYI += method; break;
		case SimpleGesture.E_DIRECTION.LFT : this.broadcastOn9AxisSwipeLFTI += method; break;
		case SimpleGesture.E_DIRECTION.RHT : this.broadcastOn9AxisSwipeRHTI += method; break;
		case SimpleGesture.E_DIRECTION.UPR : this.broadcastOn9AxisSwipeUPRI += method; break;
		case SimpleGesture.E_DIRECTION.DWN : this.broadcastOn9AxisSwipeDWNI += method; break;
		case SimpleGesture.E_DIRECTION.DDR : this.broadcastOn9AxisSwipeDDRI += method; break;
		case SimpleGesture.E_DIRECTION.DUR : this.broadcastOn9AxisSwipeDURI += method; break;
		case SimpleGesture.E_DIRECTION.DDL : this.broadcastOn9AxisSwipeDDLI += method; break;
		case SimpleGesture.E_DIRECTION.DUL : this.broadcastOn9AxisSwipeDULI += method; break;
		default : Debug.LogError(SimpleGesture.ERROR_ENUM_DOES_NOT_CORRESPOND); break;
		}
	}
	
	public void RemoveDelegate(SimpleGesture.GestureDelegate<GestureInfoSwipe> method)
	{
		switch (this.tmp_type)
		{
		case SimpleGesture.E_DIRECTION.ANY : this.broadcastOn9AxisSwipeANYI -= method; break;
		case SimpleGesture.E_DIRECTION.LFT : this.broadcastOn9AxisSwipeLFTI -= method; break;
		case SimpleGesture.E_DIRECTION.RHT : this.broadcastOn9AxisSwipeRHTI -= method; break;
		case SimpleGesture.E_DIRECTION.UPR : this.broadcastOn9AxisSwipeUPRI -= method; break;
		case SimpleGesture.E_DIRECTION.DWN : this.broadcastOn9AxisSwipeDWNI -= method; break;
		case SimpleGesture.E_DIRECTION.DDR : this.broadcastOn9AxisSwipeDDRI -= method; break;
		case SimpleGesture.E_DIRECTION.DUR : this.broadcastOn9AxisSwipeDURI -= method; break;
		case SimpleGesture.E_DIRECTION.DDL : this.broadcastOn9AxisSwipeDDLI -= method; break;
		case SimpleGesture.E_DIRECTION.DUL : this.broadcastOn9AxisSwipeDULI -= method; break;
		default : Debug.LogError(SimpleGesture.ERROR_ENUM_DOES_NOT_CORRESPOND); break;
		}
	}

	public override bool HasDelegates()
	{
		if (this.broadcastOn9AxisSwipeANY == null &&
		    this.broadcastOn9AxisSwipeLFT == null &&
		    this.broadcastOn9AxisSwipeRHT == null &&
		    this.broadcastOn9AxisSwipeUPR == null &&
		    this.broadcastOn9AxisSwipeDWN == null &&
		    this.broadcastOn9AxisSwipeDDR == null &&
		    this.broadcastOn9AxisSwipeDUR == null &&
		    this.broadcastOn9AxisSwipeDDL == null &&
		    this.broadcastOn9AxisSwipeDUL == null &&
		    this.broadcastOn9AxisSwipeANYI == null &&
		    this.broadcastOn9AxisSwipeLFTI == null &&
		    this.broadcastOn9AxisSwipeRHTI == null &&
		    this.broadcastOn9AxisSwipeUPRI == null &&
		    this.broadcastOn9AxisSwipeDWNI == null &&
		    this.broadcastOn9AxisSwipeDDRI == null &&
		    this.broadcastOn9AxisSwipeDURI == null &&
		    this.broadcastOn9AxisSwipeDDLI == null &&
		    this.broadcastOn9AxisSwipeDULI == null)
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

		if (angle <= 20.0f)
		{
			return this.broadcastOn9AxisSwipeRHT;
		}
		else if (angle >= 160.0f)
		{
			return this.broadcastOn9AxisSwipeLFT;
		}
		else if (angle > 20.0f && angle <= 70.0f)
		{
			if (direction.y > 0.0f) return this.broadcastOn9AxisSwipeDUR;
			else return this.broadcastOn9AxisSwipeDDR;
		}
		else if (angle > 70.0f && angle <= 110.0f)
		{
			if (direction.y > 0.0f) return this.broadcastOn9AxisSwipeUPR;
			else return this.broadcastOn9AxisSwipeDWN;
		}
		else if (angle > 110.0f && angle < 160.0f)
		{
			if (direction.y > 0.0f) return this.broadcastOn9AxisSwipeDUL;
			else return this.broadcastOn9AxisSwipeDDL;
		}

		Debug.LogError(SimpleGesture.ERROR_ANGLE_VALUE_NOT_CAUGHT + " {"+angle+"}");
		return null;
	}

	private SimpleGesture.GestureDelegate<GestureInfoSwipe> GetDelegateI(TouchInfo touchInfo)
	{
		Vector2 direction = (touchInfo.GetLastPosition() - touchInfo.GetFirstPosition()).normalized;
		float angle = Vector2.Angle(direction, Vector2.right);
		
		if (angle <= 20.0f)
		{
			return this.broadcastOn9AxisSwipeRHTI;
		}
		else if (angle >= 160.0f)
		{
			return this.broadcastOn9AxisSwipeLFTI;
		}
		else if (angle > 20.0f && angle <= 70.0f)
		{
			if (direction.y > 0.0f) return this.broadcastOn9AxisSwipeDURI;
			else return this.broadcastOn9AxisSwipeDDRI;
		}
		else if (angle > 70.0f && angle <= 110.0f)
		{
			if (direction.y > 0.0f) return this.broadcastOn9AxisSwipeUPRI;
			else return this.broadcastOn9AxisSwipeDWNI;
		}
		else if (angle > 110.0f && angle < 160.0f)
		{
			if (direction.y > 0.0f) return this.broadcastOn9AxisSwipeDULI;
			else return this.broadcastOn9AxisSwipeDDLI;
		}
		
		Debug.LogError(SimpleGesture.ERROR_ANGLE_VALUE_NOT_CAUGHT + " {"+angle+"}");
		return null;
	}
}
