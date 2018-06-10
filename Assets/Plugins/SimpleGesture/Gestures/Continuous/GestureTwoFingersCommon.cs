using UnityEngine;
using System.Collections;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTEND SIMPLE GESTURE: ----------------------------------------------------------------------------------------------

public partial class SimpleGesture
{
	public enum E_2FINGERSCOMMON
	{
		PANNING,
		PINCHING,
		STRETCHING,
		TWISTING
	};

	public static void WhilePanning(GestureDelegate method)    { SimpleGesture.TwoFingerGesture_AddMethod(E_2FINGERSCOMMON.PANNING,    method); }
	public static void WhilePinching(GestureDelegate method)   { SimpleGesture.TwoFingerGesture_AddMethod(E_2FINGERSCOMMON.PINCHING,   method); }
	public static void WhileStretching(GestureDelegate method) { SimpleGesture.TwoFingerGesture_AddMethod(E_2FINGERSCOMMON.STRETCHING, method); }
	public static void WhileTwisting(GestureDelegate method)   { SimpleGesture.TwoFingerGesture_AddMethod(E_2FINGERSCOMMON.TWISTING,   method); }
	
	public static void StopPanning(GestureDelegate method)    { SimpleGesture.TwoFingerGesture_SubsMethod(E_2FINGERSCOMMON.PANNING,    method); }
	public static void StopPinching(GestureDelegate method)   { SimpleGesture.TwoFingerGesture_SubsMethod(E_2FINGERSCOMMON.PINCHING,   method); }
	public static void StopStretching(GestureDelegate method) { SimpleGesture.TwoFingerGesture_SubsMethod(E_2FINGERSCOMMON.STRETCHING, method); }
	public static void StopTwisting(GestureDelegate method)   { SimpleGesture.TwoFingerGesture_SubsMethod(E_2FINGERSCOMMON.TWISTING,   method); }

	public static void WhilePanning(GestureDelegate<GestureInfoPan> method)     { SimpleGesture.TwoFingerGesture_AddMethod(E_2FINGERSCOMMON.PANNING,    method); }
	public static void WhilePinching(GestureDelegate<GestureInfoZoom> method)   { SimpleGesture.TwoFingerGesture_AddMethod(E_2FINGERSCOMMON.PINCHING,   method); }
	public static void WhileStretching(GestureDelegate<GestureInfoZoom> method) { SimpleGesture.TwoFingerGesture_AddMethod(E_2FINGERSCOMMON.STRETCHING, method); }
	public static void WhileTwisting(GestureDelegate<GestureInfoTwist> method)  { SimpleGesture.TwoFingerGesture_AddMethod(E_2FINGERSCOMMON.TWISTING,   method); }

	public static void StopPanning(GestureDelegate<GestureInfoPan> method)      { SimpleGesture.TwoFingerGesture_SubsMethod(E_2FINGERSCOMMON.PANNING,    method); }
	public static void StopPinching(GestureDelegate<GestureInfoZoom> method)    { SimpleGesture.TwoFingerGesture_SubsMethod(E_2FINGERSCOMMON.PINCHING,   method); }
	public static void StopStretching(GestureDelegate<GestureInfoZoom> method)  { SimpleGesture.TwoFingerGesture_SubsMethod(E_2FINGERSCOMMON.STRETCHING, method); }
	public static void StopTwisting(GestureDelegate<GestureInfoTwist> method)   { SimpleGesture.TwoFingerGesture_SubsMethod(E_2FINGERSCOMMON.TWISTING,   method); }

	private static void TwoFingerGesture_AddMethod(E_2FINGERSCOMMON type, GestureDelegate method)
	{
		if (GestureTwoFingersCommon._instance == null)
		{
			GestureTwoFingersCommon gesture2Fingers = new GestureTwoFingersCommon();
			SimpleGesture.Instance.twoFingerGestures.Add(gesture2Fingers);
		}

		GestureTwoFingersCommon._instance.SetType(type);
		GestureTwoFingersCommon._instance.AddDelegate(method);
	}
	
	private static void TwoFingerGesture_SubsMethod(E_2FINGERSCOMMON type, GestureDelegate method)
	{
		GestureTwoFingersCommon._instance.SetType(type);
		GestureTwoFingersCommon._instance.RemoveDelegate(method);
		
		if (!GestureTwoFingersCommon._instance.HasDelegates())
		{
			SimpleGesture.Instance.twoFingerGestures.Remove(GestureTwoFingersCommon._instance);
			GestureTwoFingersCommon._instance = null;
		}
	}

	private static void TwoFingerGesture_AddMethod(E_2FINGERSCOMMON type, GestureDelegate<GestureInfoPan> method)
	{
		if (GestureTwoFingersCommon._instance == null)
		{
			GestureTwoFingersCommon gesture2Fingers = new GestureTwoFingersCommon();
			SimpleGesture.Instance.twoFingerGestures.Add(gesture2Fingers);
		}
		
		GestureTwoFingersCommon._instance.SetType(type);
		GestureTwoFingersCommon._instance.AddDelegate(method);
	}

	private static void TwoFingerGesture_AddMethod(E_2FINGERSCOMMON type, GestureDelegate<GestureInfoZoom> method)
	{
		if (GestureTwoFingersCommon._instance == null)
		{
			GestureTwoFingersCommon gesture2Fingers = new GestureTwoFingersCommon();
			SimpleGesture.Instance.twoFingerGestures.Add(gesture2Fingers);
		}
		
		GestureTwoFingersCommon._instance.SetType(type);
		GestureTwoFingersCommon._instance.AddDelegate(method);
	}

	private static void TwoFingerGesture_AddMethod(E_2FINGERSCOMMON type, GestureDelegate<GestureInfoTwist> method)
	{
		if (GestureTwoFingersCommon._instance == null)
		{
			GestureTwoFingersCommon gesture2Fingers = new GestureTwoFingersCommon();
			SimpleGesture.Instance.twoFingerGestures.Add(gesture2Fingers);
		}
		
		GestureTwoFingersCommon._instance.SetType(type);
		GestureTwoFingersCommon._instance.AddDelegate(method);
	}

	private static void TwoFingerGesture_SubsMethod(E_2FINGERSCOMMON type, GestureDelegate<GestureInfoPan> method)
	{
		GestureTwoFingersCommon._instance.SetType(type);
		GestureTwoFingersCommon._instance.RemoveDelegate(method);
		
		if (!GestureTwoFingersCommon._instance.HasDelegates())
		{
			SimpleGesture.Instance.twoFingerGestures.Remove(GestureTwoFingersCommon._instance);
			GestureTwoFingersCommon._instance = null;
		}
	}

	private static void TwoFingerGesture_SubsMethod(E_2FINGERSCOMMON type, GestureDelegate<GestureInfoZoom> method)
	{
		GestureTwoFingersCommon._instance.SetType(type);
		GestureTwoFingersCommon._instance.RemoveDelegate(method);
		
		if (!GestureTwoFingersCommon._instance.HasDelegates())
		{
			SimpleGesture.Instance.twoFingerGestures.Remove(GestureTwoFingersCommon._instance);
			GestureTwoFingersCommon._instance = null;
		}
	}

	private static void TwoFingerGesture_SubsMethod(E_2FINGERSCOMMON type, GestureDelegate<GestureInfoTwist> method)
	{
		GestureTwoFingersCommon._instance.SetType(type);
		GestureTwoFingersCommon._instance.RemoveDelegate(method);
		
		if (!GestureTwoFingersCommon._instance.HasDelegates())
		{
			SimpleGesture.Instance.twoFingerGestures.Remove(GestureTwoFingersCommon._instance);
			GestureTwoFingersCommon._instance = null;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// GESTURE: ------------------------------------------------------------------------------------------------------------

public class GestureTwoFingersCommon : BaseGesture 
{
	// VARIABLES: ------------------------------------------------------------------------------------------------------
	
	public static GestureTwoFingersCommon _instance;
	
	protected SimpleGesture.GestureDelegate broadcastWhilePanning;
	protected SimpleGesture.GestureDelegate broadcastWhilePinching;
	protected SimpleGesture.GestureDelegate broadcastWhileStretching;
	protected SimpleGesture.GestureDelegate broadcastWhileTwisting;

	protected SimpleGesture.GestureDelegate<GestureInfoPan>   broadcastWhilePanningI;
	protected SimpleGesture.GestureDelegate<GestureInfoZoom>  broadcastWhilePinchingI;
	protected SimpleGesture.GestureDelegate<GestureInfoZoom>  broadcastWhileStretchingI;
	protected SimpleGesture.GestureDelegate<GestureInfoTwist> broadcastWhileTwistingI;

	SimpleGesture.E_2FINGERSCOMMON tmp_type;
	
	// CONSTRUCTOR: ----------------------------------------------------------------------------------------------------
	
	public GestureTwoFingersCommon() : base()
	{
		GestureTwoFingersCommon._instance = this;
	}

	public override void Delete()
	{
		GestureTwoFingersCommon._instance = null;
	}

	// OVERRIDES: ------------------------------------------------------------------------------------------------------
	
	protected override void OnMoved(TouchInfo touchInfo1, TouchInfo touchInfo2)
	{
		TouchPhase touchPhase = ((touchInfo1.GetPositionsSize() == 1 || touchInfo2.GetPositionsSize() == 1)
			? TouchPhase.Began : TouchPhase.Ended
		);

		if (touchInfo1.GetDeltaPosition() == Vector2.zero) return;
		if (touchInfo2.GetDeltaPosition() == Vector2.zero) return;
		
		Vector2 direction1 = touchInfo1.GetLastPosition() - (touchInfo1.GetLastPosition() - touchInfo1.GetDeltaPosition());
		Vector2 direction2 = touchInfo2.GetLastPosition() - (touchInfo2.GetLastPosition() - touchInfo2.GetDeltaPosition());
		
		float angle = Vector2.Angle(direction1, direction2);
		
		Vector2 center = Vector2.zero;
		center += touchInfo1.GetLastPosition() - touchInfo1.GetDeltaPosition();
		center += touchInfo2.GetLastPosition() - touchInfo2.GetDeltaPosition();
		center /= 2.0f;
		
		Vector2 dirCenter1 = (touchInfo1.GetLastPosition() - touchInfo1.GetDeltaPosition()) - center;
		Vector2 dirCenter2 = (touchInfo2.GetLastPosition() - touchInfo2.GetDeltaPosition()) - center;
		
		float dot1 = Vector3.Dot(dirCenter1.normalized, direction1.normalized);
		float dot2 = Vector3.Dot(dirCenter2.normalized, direction2.normalized);
		
		float angle1 = Mathf.Acos(dot1) * Mathf.Rad2Deg;
		float angle2 = Mathf.Acos(dot2) * Mathf.Rad2Deg;

		if (this.IsPanning(angle))
		{
			GestureInfoPan gesture = new GestureInfoPan(touchInfo1.GetDeltaPosition(), touchPhase);
			if (this.broadcastWhilePanning  != null) this.broadcastWhilePanning();
			if (this.broadcastWhilePanningI != null) this.broadcastWhilePanningI(gesture);
		}

		if (this.IsPinching(angle1, angle2)) 
		{
			float t_delta = direction1.magnitude + direction2.magnitude;
			Vector2 t_position1 = touchInfo1.GetLastPosition();
			Vector2 t_position2 = touchInfo2.GetLastPosition();
			Vector2 t_center = (t_position1 + t_position2) / 2.0f;
			GestureInfoZoom gesture = new GestureInfoZoom(t_delta, t_center, t_position1, t_position2, touchPhase);

			if (this.broadcastWhilePinching  != null) this.broadcastWhilePinching();
			if (this.broadcastWhilePinchingI != null) this.broadcastWhilePinchingI(gesture);
		}

		if (this.IsStretching(angle1, angle2))
		{
			float t_delta = direction1.magnitude + direction2.magnitude;
			Vector2 t_position1 = touchInfo1.GetLastPosition();
			Vector2 t_position2 = touchInfo2.GetLastPosition();
			Vector2 t_center = (t_position1 + t_position2) / 2.0f;
			GestureInfoZoom gesture = new GestureInfoZoom(t_delta, t_center, t_position1, t_position2, touchPhase);

			if (this.broadcastWhileStretching  != null) this.broadcastWhileStretching();
			if (this.broadcastWhileStretchingI != null) this.broadcastWhileStretchingI(gesture);
		}

		if (this.IsTwisting(angle1, angle2))
		{
			Vector2 t_pos1 = touchInfo1.GetLastPosition();
			Vector2 t_pos2 = touchInfo2.GetLastPosition();
			Vector2 t_center = (t_pos1 + t_pos2) / 2.0f;
			float t_delta = direction1.magnitude + direction2.magnitude;

			bool t_clockwise;
			if (t_pos1.y > t_pos2.y) t_clockwise = (direction1.x < 0.0f ? false : true);
			else t_clockwise = (direction2.x < 0.0f ? false : true);
			
			GestureInfoTwist gesture = new GestureInfoTwist(t_center, t_delta, t_pos1, t_pos2, t_clockwise, touchPhase);

			if (this.broadcastWhileTwisting  != null) this.broadcastWhileTwisting();
			if (this.broadcastWhileTwistingI != null) this.broadcastWhileTwistingI(gesture);
		}
	}

	protected override void OnEnded (TouchInfo touchInfo1, TouchInfo touchInfo2)
	{
		Vector2 position1 = touchInfo1.GetLastPosition();
		Vector2 position2 = touchInfo2.GetLastPosition();
		Vector2 center = (position1 + position2)/2f;

		GestureInfoPan gesurePan = new GestureInfoPan(Vector2.zero, TouchPhase.Ended);
		if (this.broadcastWhilePanning  != null) this.broadcastWhilePanning();
		if (this.broadcastWhilePanningI != null) this.broadcastWhilePanningI(gesurePan);

		GestureInfoZoom gestureZoom = new GestureInfoZoom(0f, center, position1, position2, TouchPhase.Ended);
		if (this.broadcastWhilePinching  != null) this.broadcastWhilePinching();
		if (this.broadcastWhilePinchingI != null) this.broadcastWhilePinchingI(gestureZoom);

		if (this.broadcastWhileStretching  != null) this.broadcastWhileStretching();
		if (this.broadcastWhileStretchingI != null) this.broadcastWhileStretchingI(gestureZoom);

		GestureInfoTwist gesture = new GestureInfoTwist(center, 0f, position1, position2, true, TouchPhase.Ended);
		if (this.broadcastWhileTwisting  != null) this.broadcastWhileTwisting();
		if (this.broadcastWhileTwistingI != null) this.broadcastWhileTwistingI(gesture);
	}
	
	// ADD AND REMOVE METHODS: -----------------------------------------------------------------------------------------
	
	public override void AddDelegate(SimpleGesture.GestureDelegate method)
	{
		switch (this.tmp_type)
		{
		case SimpleGesture.E_2FINGERSCOMMON.PANNING    : this.broadcastWhilePanning    += method; break;
		case SimpleGesture.E_2FINGERSCOMMON.PINCHING   : this.broadcastWhilePinching   += method; break;
		case SimpleGesture.E_2FINGERSCOMMON.STRETCHING : this.broadcastWhileStretching += method; break;
		case SimpleGesture.E_2FINGERSCOMMON.TWISTING   : this.broadcastWhileTwisting   += method; break;
		}
	}

	public void AddDelegate(SimpleGesture.GestureDelegate<GestureInfoPan> method)
	{
		this.broadcastWhilePanningI += method;
	}

	public void AddDelegate(SimpleGesture.GestureDelegate<GestureInfoZoom> method)
	{
		switch (this.tmp_type)
		{
		case SimpleGesture.E_2FINGERSCOMMON.PINCHING : this.broadcastWhilePinchingI += method; break;
		case SimpleGesture.E_2FINGERSCOMMON.STRETCHING : this.broadcastWhileStretchingI += method; break;
		}
	}

	public void AddDelegate(SimpleGesture.GestureDelegate<GestureInfoTwist> method)
	{
		this.broadcastWhileTwistingI += method;
	}

	public override void RemoveDelegate(SimpleGesture.GestureDelegate method)
	{
		switch (this.tmp_type)
		{
		case SimpleGesture.E_2FINGERSCOMMON.PANNING    : this.broadcastWhilePanning    -= method; break;
		case SimpleGesture.E_2FINGERSCOMMON.PINCHING   : this.broadcastWhilePinching   -= method; break;
		case SimpleGesture.E_2FINGERSCOMMON.STRETCHING : this.broadcastWhileStretching -= method; break;
		case SimpleGesture.E_2FINGERSCOMMON.TWISTING   : this.broadcastWhileTwisting   -= method; break;
		}
	}

	public void RemoveDelegate(SimpleGesture.GestureDelegate<GestureInfoPan> method)
	{
		this.broadcastWhilePanningI -= method;
	}
	
	public void RemoveDelegate(SimpleGesture.GestureDelegate<GestureInfoZoom> method)
	{
		switch (this.tmp_type)
		{
		case SimpleGesture.E_2FINGERSCOMMON.PINCHING : this.broadcastWhilePinchingI -= method; break;
		case SimpleGesture.E_2FINGERSCOMMON.STRETCHING : this.broadcastWhileStretchingI -= method; break;
		}
	}
	
	public void RemoveDelegate(SimpleGesture.GestureDelegate<GestureInfoTwist> method)
	{
		this.broadcastWhileTwistingI -= method;
	}

	
	public override bool HasDelegates()
	{
		if (this.broadcastWhilePanning == null     && this.broadcastWhilePinching == null  &&
		    this.broadcastWhileStretching == null  && this.broadcastWhileTwisting == null  &&
		    this.broadcastWhilePanningI == null    && this.broadcastWhilePinchingI == null &&
		    this.broadcastWhileStretchingI == null && this.broadcastWhileTwistingI == null)
		{
			return false;
		}

		return true;
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// OTHER METHODS: --------------------------------------------------------------------------------------------------
	
	private bool IsPanning(float angle)
	{
		if (angle < 25.0f) return true;
		return false;
	}

	private bool IsPinching(float angle1, float angle2)
	{
		if (angle1 > 160.0f && angle2 > 160.0f) return true;
		return false;
	}

	private bool IsStretching(float angle1, float angle2)
	{
		if (angle1 < 20.0f && angle2 < 20.0f) return true;
		return false;
	}

	private bool IsTwisting(float angle1, float angle2)
	{
		if (angle1 > 60.0f && angle1 < 120.0f && angle2 > 60.0f && angle2 < 120.0f) return true;
		return false;
	}

	public void SetType(SimpleGesture.E_2FINGERSCOMMON type)
	{
		this.tmp_type = type;
	}
}
