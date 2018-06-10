using UnityEngine;
using System.Collections;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTEND SIMPLE GESTURE: ----------------------------------------------------------------------------------------------

public partial class SimpleGesture
{
	public static void OnCircle(GestureDelegate method) 
	{
		if (GestureCircle._instance == null)
		{
			GestureCircle gestureCircle = new GestureCircle();
			SimpleGesture.Instance.oneFingerGestures.Add(gestureCircle);
		}

		GestureCircle._instance.AddDelegate(method);
	}

	public static void OnCircle(GestureDelegate<GestureInfoCircle> method)
	{
		if (GestureCircle._instance == null)
		{
			GestureCircle gestureCircle = new GestureCircle();
			SimpleGesture.Instance.oneFingerGestures.Add(gestureCircle);
		}
		
		GestureCircle._instance.AddDelegate(method);
	}
	
	public static void StopCircle(GestureDelegate method)
	{
		GestureCircle._instance.RemoveDelegate(method);
		
		if (!GestureCircle._instance.HasDelegates())
		{
			SimpleGesture.Instance.oneFingerGestures.Remove(GestureCircle._instance);
			GestureCircle._instance = null;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// GESTURE: ------------------------------------------------------------------------------------------------------------

public class GestureCircle : BaseGesture 
{
	// VARIABLES: ------------------------------------------------------------------------------------------------------
	
	public static GestureCircle _instance;

	private const float CIRCLE_MIN_ANGLE  = 300.0f;
	private const float CIRCLE_MAX_ANGLE  = 440.0f;
	private const float CIRCLE_MIN_RADIUS = 10.0f;
	private const int CIRCLE_NUMPOS_ITER1 = 10;
	private const int CIRCLE_NUMPOS_ITER2 = 20;

	protected SimpleGesture.GestureDelegate broadcastOnCircle;
	protected SimpleGesture.GestureDelegate<GestureInfoCircle> broadcastOnCircleI;

	private float t_radius;
	private Vector2 t_center;
	private bool t_clockwise;

	// CONSTRUCTOR: ----------------------------------------------------------------------------------------------------
	
	public GestureCircle() : base()
	{
		GestureCircle._instance = this;
	}

	public override void Delete()
	{
		GestureCircle._instance = null;
	}
	
	// OVERRIDES: ------------------------------------------------------------------------------------------------------
	
	protected override void OnEnded(TouchInfo touchInfo)
	{
		if (this.IsCircle(touchInfo))
		{
			if (this.broadcastOnCircle != null) this.broadcastOnCircle();
			if (this.broadcastOnCircleI != null)
			{
				GestureInfoCircle info = new GestureInfoCircle(this.t_radius, this.t_center);
				this.broadcastOnCircleI(info);
			}
		}
	}
	
	// ADD AND REMOVE METHODS: -----------------------------------------------------------------------------------------
	
	public override void AddDelegate(SimpleGesture.GestureDelegate method)
	{
		this.broadcastOnCircle += method;
	}
	
	public override void RemoveDelegate(SimpleGesture.GestureDelegate method)
	{
		this.broadcastOnCircle -= method;
	}

	public void AddDelegate(SimpleGesture.GestureDelegate<GestureInfoCircle> method)
	{
		this.broadcastOnCircleI += method;
	}

	public void RemoveDelegate(SimpleGesture.GestureDelegate<GestureInfoCircle> method)
	{
		this.broadcastOnCircleI -= method;
	}
	
	public override bool HasDelegates()
	{
		bool del1 = (this.broadcastOnCircle  == null ? false : true);
		bool del2 = (this.broadcastOnCircleI == null ? false : true);
		return (del1 || del2);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// OTHER METHODS: --------------------------------------------------------------------------------------------------

	private bool IsCircle(TouchInfo touchInfo)
	{
		Vector2 center = touchInfo.GetAveragePosition();
		float avgDistance = 0.0f;

		Vector2 prevDirection = Vector2.zero;
		float totalAngle = 0.0f;

		// get the averaged center of the circle and calculate arbirary angles: ----------------------------------------
		for (int i = 0; i < CIRCLE_NUMPOS_ITER1; ++i)
		{
			Vector2 position = touchInfo.GetDecimatedPosition(i, CIRCLE_NUMPOS_ITER1);
			avgDistance += Vector2.Distance(position, center);

			Vector2 currDirection = (center - position);

			if (i != 0) totalAngle += Vector2.Angle(prevDirection, currDirection);
			prevDirection = currDirection;
		}

		// check if angle is within a logical range: -------------------------------------------------------------------
		totalAngle += Vector2.Angle(prevDirection, (center - touchInfo.GetLastPosition()));
		if (totalAngle < CIRCLE_MIN_ANGLE || totalAngle > CIRCLE_MAX_ANGLE) return false;

		// make sure the average distance is big enough: ---------------------------------------------------------------
		avgDistance = avgDistance / (float)CIRCLE_NUMPOS_ITER1;
		if (avgDistance < CIRCLE_MIN_RADIUS) return false;

		float maxDistance = avgDistance * 1.5f;
		float minDistance = avgDistance * 0.5f;

		// check random positions if the radius is within a certain range: ---------------------------------------------
		for (int i = 0; i < CIRCLE_NUMPOS_ITER2; ++i)
		{
			Vector2 randomPosition = touchInfo.GetRandomPosition();
			float distance = Vector2.Distance(randomPosition, center);
			if (distance > maxDistance || distance < minDistance) 
			{
				return false;
			}
		}

		this.t_center = center;
		this.t_radius = avgDistance;

		return true;
	}
}
