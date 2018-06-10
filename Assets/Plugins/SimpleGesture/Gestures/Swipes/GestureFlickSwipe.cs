using UnityEngine;
using System.Collections;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTEND SIMPLE GESTURE: ----------------------------------------------------------------------------------------------

public partial class SimpleGesture
{
	public static void OnFlickSwipe(GestureDelegate method)                        { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.ANY, method, false); }
	public static void On9AxisFlickSwipeLeft(GestureDelegate method)               { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.LFT, method, false); }
	public static void On9AxisFlickSwipeRight(GestureDelegate method)              { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.RHT, method, false); }
	public static void On9AxisFlickSwipeUp(GestureDelegate method)                 { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.UPR, method, false); }
	public static void On9AxisFlickSwipeDown(GestureDelegate method)               { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.DWN, method, false); }
	public static void On9AxisFlickSwipeDiagonalRightUp(GestureDelegate method)    { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.DUR, method, false); }
	public static void On9AxisFlickSwipeDiagonalRightDown(GestureDelegate method)  { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.DDR, method, false); }
	public static void On9AxisFlickSwipeDiagonalLeftUp(GestureDelegate method)     { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.DUL, method, false); }
	public static void On9AxisFlickSwipeDiagonalLeftDown(GestureDelegate method)   { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.DDL, method, false); }
	public static void On4AxisFlickSwipeLeft(GestureDelegate method)               { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.LFT, method, true ); }
	public static void On4AxisFlickSwipeRight(GestureDelegate method)              { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.RHT, method, true ); }
	public static void On4AxisFlickSwipeUp(GestureDelegate method)                 { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.UPR, method, true ); }
	public static void On4AxisFlickSwipeDown(GestureDelegate method)               { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.DWN, method, true ); }


	public static void StopFlickSwipe(GestureDelegate method)                        { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.ANY, method, false); }
	public static void Stop9AxisFlickSwipeLeft(GestureDelegate method)               { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.LFT, method, false); }
	public static void Stop9AxisFlickSwipeRight(GestureDelegate method)              { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.RHT, method, false); }
	public static void Stop9AxisFlickSwipeUp(GestureDelegate method)                 { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.UPR, method, false); }
	public static void Stop9AxisFlickSwipeDown(GestureDelegate method)               { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.DWN, method, false); }
	public static void Stop9AxisFlickSwipeDiagonalRightUp(GestureDelegate method)    { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.DUR, method, false); }
	public static void Stop9AxisFlickSwipeDiagonalRightDown(GestureDelegate method)  { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.DDR, method, false); }
	public static void Stop9AxisFlickSwipeDiagonalLeftUp(GestureDelegate method)     { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.DUL, method, false); }
	public static void Stop9AxisFlickSwipeDiagonalLeftDown(GestureDelegate method)   { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.DDL, method, false); }
	public static void Stop4AxisFlickSwipeLeft(GestureDelegate method)               { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.LFT, method, true ); }
	public static void Stop4AxisFlickSwipeRight(GestureDelegate method)              { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.RHT, method, true ); }
	public static void Stop4AxisFlickSwipeUp(GestureDelegate method)                 { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.UPR, method, true ); }
	public static void Stop4AxisFlickSwipeDown(GestureDelegate method)               { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.DWN, method, true ); }

	public static void OnFlickSwipe(GestureDelegate<GestureInfoSwipe> method)                        { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.ANY, method, false); }
	public static void On9AxisFlickSwipeLeft(GestureDelegate<GestureInfoSwipe> method)               { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.LFT, method, false); }
	public static void On9AxisFlickSwipeRight(GestureDelegate<GestureInfoSwipe> method)              { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.RHT, method, false); }
	public static void On9AxisFlickSwipeUp(GestureDelegate<GestureInfoSwipe> method)                 { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.UPR, method, false); }
	public static void On9AxisFlickSwipeDown(GestureDelegate<GestureInfoSwipe> method)               { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.DWN, method, false); }
	public static void On9AxisFlickSwipeDiagonalRightUp(GestureDelegate<GestureInfoSwipe> method)    { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.DUR, method, false); }
	public static void On9AxisFlickSwipeDiagonalRightDown(GestureDelegate<GestureInfoSwipe> method)  { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.DDR, method, false); }
	public static void On9AxisFlickSwipeDiagonalLeftUp(GestureDelegate<GestureInfoSwipe> method)     { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.DUL, method, false); }
	public static void On9AxisFlickSwipeDiagonalLeftDown(GestureDelegate<GestureInfoSwipe> method)   { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.DDL, method, false); }
	public static void On4AxisFlickSwipeLeft(GestureDelegate<GestureInfoSwipe> method)               { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.LFT, method, true ); }
	public static void On4AxisFlickSwipeRight(GestureDelegate<GestureInfoSwipe> method)              { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.RHT, method, true ); }
	public static void On4AxisFlickSwipeUp(GestureDelegate<GestureInfoSwipe> method)                 { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.UPR, method, true ); }
	public static void On4AxisFlickSwipeDown(GestureDelegate<GestureInfoSwipe> method)               { SimpleGesture.SwipeFlick_AddMethod(E_DIRECTION.DWN, method, true ); }
	
	
	public static void StopFlickSwipe(GestureDelegate<GestureInfoSwipe> method)                        { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.ANY, method, false); }
	public static void Stop9AxisFlickSwipeLeft(GestureDelegate<GestureInfoSwipe> method)               { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.LFT, method, false); }
	public static void Stop9AxisFlickSwipeRight(GestureDelegate<GestureInfoSwipe> method)              { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.RHT, method, false); }
	public static void Stop9AxisFlickSwipeUp(GestureDelegate<GestureInfoSwipe> method)                 { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.UPR, method, false); }
	public static void Stop9AxisFlickSwipeDown(GestureDelegate<GestureInfoSwipe> method)               { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.DWN, method, false); }
	public static void Stop9AxisFlickSwipeDiagonalRightUp(GestureDelegate<GestureInfoSwipe> method)    { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.DUR, method, false); }
	public static void Stop9AxisFlickSwipeDiagonalRightDown(GestureDelegate<GestureInfoSwipe> method)  { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.DDR, method, false); }
	public static void Stop9AxisFlickSwipeDiagonalLeftUp(GestureDelegate<GestureInfoSwipe> method)     { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.DUL, method, false); }
	public static void Stop9AxisFlickSwipeDiagonalLeftDown(GestureDelegate<GestureInfoSwipe> method)   { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.DDL, method, false); }
	public static void Stop4AxisFlickSwipeLeft(GestureDelegate<GestureInfoSwipe> method)               { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.LFT, method, true ); }
	public static void Stop4AxisFlickSwipeRight(GestureDelegate<GestureInfoSwipe> method)              { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.RHT, method, true ); }
	public static void Stop4AxisFlickSwipeUp(GestureDelegate<GestureInfoSwipe> method)                 { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.UPR, method, true ); }
	public static void Stop4AxisFlickSwipeDown(GestureDelegate<GestureInfoSwipe> method)               { SimpleGesture.SwipeFlick_SubsMethod(E_DIRECTION.DWN, method, true ); }

	private static void SwipeFlick_AddMethod(E_DIRECTION type, GestureDelegate method, bool is4Axis)
	{
		if (GestureFlickSwipe._instance == null)
		{
			GestureFlickSwipe gestureSwipe = new GestureFlickSwipe();
			SimpleGesture.Instance.oneFingerGestures.Add(gestureSwipe);
		}

		GestureFlickSwipe._instance.SetType(type, is4Axis);
		GestureFlickSwipe._instance.AddDelegate(method);
	}

	private static void SwipeFlick_SubsMethod(E_DIRECTION type, GestureDelegate method, bool is4Axis)
	{
		GestureFlickSwipe._instance.SetType(type, is4Axis);
		GestureFlickSwipe._instance.RemoveDelegate(method);

		if (!GestureFlickSwipe._instance.HasDelegates())
		{
			SimpleGesture.Instance.oneFingerGestures.Remove(GestureFlickSwipe._instance);
			GestureFlickSwipe._instance = null;
		}
	}

	private static void SwipeFlick_AddMethod(E_DIRECTION type, GestureDelegate<GestureInfoSwipe> method, bool is4Axis)
	{
		if (GestureFlickSwipe._instance == null)
		{
			GestureFlickSwipe gestureSwipe = new GestureFlickSwipe();
			SimpleGesture.Instance.oneFingerGestures.Add(gestureSwipe);
		}
		
		GestureFlickSwipe._instance.SetType(type, is4Axis);
		GestureFlickSwipe._instance.AddDelegate(method);
	}
	
	private static void SwipeFlick_SubsMethod(E_DIRECTION type, GestureDelegate<GestureInfoSwipe> method, bool is4Axis)
	{
		GestureFlickSwipe._instance.SetType(type, is4Axis);
		GestureFlickSwipe._instance.RemoveDelegate(method);
		
		if (!GestureFlickSwipe._instance.HasDelegates())
		{
			SimpleGesture.Instance.oneFingerGestures.Remove(GestureFlickSwipe._instance);
			GestureFlickSwipe._instance = null;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// GESTURE: ------------------------------------------------------------------------------------------------------------

public class GestureFlickSwipe : BaseGesture 
{
	// VARIABLES: ------------------------------------------------------------------------------------------------------

	public static GestureFlickSwipe _instance;

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
	private SimpleGesture.GestureDelegate broadcastOn4AxisSwipeLFT;
	private SimpleGesture.GestureDelegate broadcastOn4AxisSwipeRHT;
	private SimpleGesture.GestureDelegate broadcastOn4AxisSwipeUPR;
	private SimpleGesture.GestureDelegate broadcastOn4AxisSwipeDWN;

	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn9AxisSwipeANYI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn9AxisSwipeLFTI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn9AxisSwipeRHTI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn9AxisSwipeUPRI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn9AxisSwipeDWNI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn9AxisSwipeDURI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn9AxisSwipeDDRI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn9AxisSwipeDULI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn9AxisSwipeDDLI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn4AxisSwipeLFTI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn4AxisSwipeRHTI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn4AxisSwipeUPRI;
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> broadcastOn4AxisSwipeDWNI;

	private SimpleGesture.E_DIRECTION tmp_type;
	private bool tmp_orth;
	private bool hasFlickSwiped = false;

	// CONSTRUCTOR: ----------------------------------------------------------------------------------------------------

	public GestureFlickSwipe() : base()
	{
		GestureFlickSwipe._instance = this;
	}

	public override void Delete()
	{
		GestureFlickSwipe._instance = null;
	}

	// OVERRIDES: ------------------------------------------------------------------------------------------------------

	protected override void OnBegin(TouchInfo touchInfo)
	{
		this.hasFlickSwiped = false;
	}

	protected override void OnMoved(TouchInfo touchInfo)
	{
		if (!this.hasFlickSwiped && this.IsSwipe(touchInfo))
		{
			this.hasFlickSwiped = true;

			if (this.broadcastOn9AxisSwipeANY != null)
			{
				this.broadcastOn9AxisSwipeANY();
			}

			if (this.broadcastOn9AxisSwipeANYI != null)
			{
				Vector2 pos1 = touchInfo.GetFirstPosition();
				Vector2 pos2 = touchInfo.GetLastPosition();
				Vector2 direction = pos2 - pos1;
				float distance = direction.magnitude;
				GestureInfoSwipe gesture = new GestureInfoSwipe(direction, distance, pos1, pos2, TouchPhase.Ended);
				this.broadcastOn9AxisSwipeANYI(gesture);
			}
			
			SimpleGesture.GestureDelegate method9Axis = this.Get9AxisDelegate(touchInfo);
			if (method9Axis != null) method9Axis();

			SimpleGesture.GestureDelegate method4Axis = this.Get4AxisDelegate(touchInfo);
			if (method4Axis != null) method4Axis();

			SimpleGesture.GestureDelegate<GestureInfoSwipe> method9AxisI = this.Get9AxisDelegateI(touchInfo);
			if (method9AxisI != null) 
			{
				Vector2 pos1 = touchInfo.GetFirstPosition();
				Vector2 pos2 = touchInfo.GetLastPosition();
				Vector2 direction = pos2 - pos1;
				float distance = direction.magnitude;
				GestureInfoSwipe gesture = new GestureInfoSwipe(direction, distance, pos1, pos2, TouchPhase.Ended);
				method9AxisI(gesture);
			}
			
			SimpleGesture.GestureDelegate<GestureInfoSwipe> method4AxisI = this.Get4AxisDelegateI(touchInfo);
			if (method4AxisI != null) 
			{
				Vector2 pos1 = touchInfo.GetFirstPosition();
				Vector2 pos2 = touchInfo.GetLastPosition();
				Vector2 direction = pos2 - pos1;
				float distance = direction.magnitude;
				GestureInfoSwipe gesture = new GestureInfoSwipe(direction, distance, pos1, pos2, TouchPhase.Ended);
				method4AxisI(gesture);
			}
		}
	}

	protected override void OnEnded(TouchInfo touchInfo)
	{
		this.hasFlickSwiped = false;
	}

	// ADD AND REMOVE METHODS: -----------------------------------------------------------------------------------------

	public override void AddDelegate(SimpleGesture.GestureDelegate method)
	{
		if (!this.tmp_orth)
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
			default : Debug.LogError(SimpleGesture.ERROR_ENUM_DOES_NOT_CORRESPOND+" "+this.tmp_orth+" "+this.tmp_type); break;
			}
		}
		else
		{
			switch (this.tmp_type)
			{
			case SimpleGesture.E_DIRECTION.LFT : this.broadcastOn4AxisSwipeLFT += method; break;
			case SimpleGesture.E_DIRECTION.RHT : this.broadcastOn4AxisSwipeRHT += method; break;
			case SimpleGesture.E_DIRECTION.UPR : this.broadcastOn4AxisSwipeUPR += method; break;
			case SimpleGesture.E_DIRECTION.DWN : this.broadcastOn4AxisSwipeDWN += method; break;
			default : Debug.LogError(SimpleGesture.ERROR_ENUM_DOES_NOT_CORRESPOND+" "+this.tmp_orth+" "+this.tmp_type); break;
			}
		}
	}

	public override void RemoveDelegate(SimpleGesture.GestureDelegate method)
	{
		if (!this.tmp_orth)
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
			default : Debug.LogError(SimpleGesture.ERROR_ENUM_DOES_NOT_CORRESPOND+" "+this.tmp_orth+" "+this.tmp_type); break;
			}
		}
		else
		{
			switch (this.tmp_type)
			{
			case SimpleGesture.E_DIRECTION.LFT : this.broadcastOn4AxisSwipeLFT -= method; break;
			case SimpleGesture.E_DIRECTION.RHT : this.broadcastOn4AxisSwipeRHT -= method; break;
			case SimpleGesture.E_DIRECTION.UPR : this.broadcastOn4AxisSwipeUPR -= method; break;
			case SimpleGesture.E_DIRECTION.DWN : this.broadcastOn4AxisSwipeDWN -= method; break;
			default : Debug.LogError(SimpleGesture.ERROR_ENUM_DOES_NOT_CORRESPOND+" "+this.tmp_orth+" "+this.tmp_type); break;
			}
		}
	}

	public void AddDelegate(SimpleGesture.GestureDelegate<GestureInfoSwipe> method)
	{
		if (!this.tmp_orth)
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
			default : Debug.LogError(SimpleGesture.ERROR_ENUM_DOES_NOT_CORRESPOND+" "+this.tmp_orth+" "+this.tmp_type); break;
			}
		}
		else
		{
			switch (this.tmp_type)
			{
			case SimpleGesture.E_DIRECTION.LFT : this.broadcastOn4AxisSwipeLFTI += method; break;
			case SimpleGesture.E_DIRECTION.RHT : this.broadcastOn4AxisSwipeRHTI += method; break;
			case SimpleGesture.E_DIRECTION.UPR : this.broadcastOn4AxisSwipeUPRI += method; break;
			case SimpleGesture.E_DIRECTION.DWN : this.broadcastOn4AxisSwipeDWNI += method; break;
			default : Debug.LogError(SimpleGesture.ERROR_ENUM_DOES_NOT_CORRESPOND+" "+this.tmp_orth+" "+this.tmp_type); break;
			}
		}
	}
	
	public void RemoveDelegate(SimpleGesture.GestureDelegate<GestureInfoSwipe> method)
	{
		if (!this.tmp_orth)
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
			default : Debug.LogError(SimpleGesture.ERROR_ENUM_DOES_NOT_CORRESPOND+" "+this.tmp_orth+" "+this.tmp_type); break;
			}
		}
		else
		{
			switch (this.tmp_type)
			{
			case SimpleGesture.E_DIRECTION.LFT : this.broadcastOn4AxisSwipeLFTI -= method; break;
			case SimpleGesture.E_DIRECTION.RHT : this.broadcastOn4AxisSwipeRHTI -= method; break;
			case SimpleGesture.E_DIRECTION.UPR : this.broadcastOn4AxisSwipeUPRI -= method; break;
			case SimpleGesture.E_DIRECTION.DWN : this.broadcastOn4AxisSwipeDWNI -= method; break;
			default : Debug.LogError(SimpleGesture.ERROR_ENUM_DOES_NOT_CORRESPOND+" "+this.tmp_orth+" "+this.tmp_type); break;
			}
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
		    this.broadcastOn4AxisSwipeLFT == null &&
		    this.broadcastOn4AxisSwipeRHT == null &&
		    this.broadcastOn4AxisSwipeUPR == null &&
		    this.broadcastOn4AxisSwipeDWN == null &&
		    this.broadcastOn9AxisSwipeANYI == null &&
		    this.broadcastOn9AxisSwipeLFTI == null &&
		    this.broadcastOn9AxisSwipeRHTI == null &&
		    this.broadcastOn9AxisSwipeUPRI == null &&
		    this.broadcastOn9AxisSwipeDWNI == null &&
		    this.broadcastOn9AxisSwipeDDRI == null &&
		    this.broadcastOn9AxisSwipeDURI == null &&
		    this.broadcastOn9AxisSwipeDDLI == null &&
		    this.broadcastOn9AxisSwipeDULI == null &&
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

	public void SetType(SimpleGesture.E_DIRECTION type, bool is4Axis)
	{
		this.tmp_type = type;
		this.tmp_orth = is4Axis;
	}

	private bool IsSwipe(TouchInfo touchInfo)
	{
		float curDistance = touchInfo.GetTotalDistance();
		float minDistance = Vector2.Distance(touchInfo.GetFirstPosition(), touchInfo.GetLastPosition());

		if (curDistance < SWIPE_MIN_DISTANCE) return false;
		if (curDistance - (SWIPE_ERR_DISTANCE * minDistance) > minDistance) return false;
		return true;
	}

	private SimpleGesture.GestureDelegate Get9AxisDelegate(TouchInfo touchInfo)
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

	private SimpleGesture.GestureDelegate Get4AxisDelegate(TouchInfo touchInfo)
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

	private SimpleGesture.GestureDelegate<GestureInfoSwipe> Get9AxisDelegateI(TouchInfo touchInfo)
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
	
	private SimpleGesture.GestureDelegate<GestureInfoSwipe> Get4AxisDelegateI(TouchInfo touchInfo)
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
