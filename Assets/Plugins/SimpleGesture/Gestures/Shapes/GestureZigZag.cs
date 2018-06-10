using UnityEngine;
using System.Collections;
using System.Collections.Generic;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTEND SIMPLE GESTURE: ----------------------------------------------------------------------------------------------

public partial class SimpleGesture
{
	public static void OnZigZag(GestureDelegate method) 
	{
		if (GestureZigZag._instance == null)
		{
			GestureZigZag gestureZigZag = new GestureZigZag();
			SimpleGesture.Instance.oneFingerGestures.Add(gestureZigZag);
		}

		GestureZigZag._instance.AddDelegate(method);
	}
	
	public static void StopZigZag(GestureDelegate method)
	{
		GestureZigZag._instance.RemoveDelegate(method);
		
		if (!GestureZigZag._instance.HasDelegates())
		{
			SimpleGesture.Instance.oneFingerGestures.Remove(GestureZigZag._instance);
			GestureZigZag._instance = null;
		}
	}

	public static void OnZigZag(GestureDelegate<GestureInfoZigZag> method) 
	{
		if (GestureZigZag._instance == null)
		{
			GestureZigZag gestureZigZag = new GestureZigZag();
			SimpleGesture.Instance.oneFingerGestures.Add(gestureZigZag);
		}
		
		GestureZigZag._instance.AddDelegate(method);
	}

	public static void StopZigZag(GestureDelegate<GestureInfoZigZag> method)
	{
		GestureZigZag._instance.RemoveDelegate(method);
		
		if (!GestureZigZag._instance.HasDelegates())
		{
			SimpleGesture.Instance.oneFingerGestures.Remove(GestureZigZag._instance);
			GestureZigZag._instance = null;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// GESTURE: ------------------------------------------------------------------------------------------------------------

public class GestureZigZag : BaseGesture 
{
	// VARIABLES: ------------------------------------------------------------------------------------------------------
	
	public static GestureZigZag _instance;

	private const float MAX_ANGLE_THRESHOLD     = 120.0f;
	private const int MIN_POINTS_TO_CHANGE_DIR  = 3;
	private const int MIN_ZIGZAGS_TO_TRIGGER    = 3;
	private const int MAX_ZIGZAGS_TO_TRIGGER    = 10;

	protected SimpleGesture.GestureDelegate broadcastOnZigZag;
	protected SimpleGesture.GestureDelegate<GestureInfoZigZag> broadcastOnZigZagI;

	// CONSTRUCTOR: ----------------------------------------------------------------------------------------------------
	
	public GestureZigZag() : base()
	{
		GestureZigZag._instance = this;
	}

	public override void Delete()
	{
		GestureZigZag._instance = null;
	}
	
	// OVERRIDES: ------------------------------------------------------------------------------------------------------

	protected override void OnEnded(TouchInfo touchInfo)
	{
		if (this.IsZigZag(touchInfo))
		{
			if (this.broadcastOnZigZag != null) this.broadcastOnZigZag();
			if (this.broadcastOnZigZagI != null)
			{
				Vector2 direction = touchInfo.GetLastPosition() - touchInfo.GetFirstPosition();
				float distance = touchInfo.GetTotalDistance();
				GestureInfoZigZag gesture = new GestureInfoZigZag(direction, distance);
				this.broadcastOnZigZagI(gesture);
			}
		}
	}
	
	// ADD AND REMOVE METHODS: -----------------------------------------------------------------------------------------
	
	public override void AddDelegate(SimpleGesture.GestureDelegate method)
	{
		this.broadcastOnZigZag += method;
	}
	
	public override void RemoveDelegate(SimpleGesture.GestureDelegate method)
	{
		this.broadcastOnZigZag -= method;
	}

	public void AddDelegate(SimpleGesture.GestureDelegate<GestureInfoZigZag> method)
	{
		this.broadcastOnZigZagI += method;
	}

	public void RemoveDelegate(SimpleGesture.GestureDelegate<GestureInfoZigZag> method)
	{
		this.broadcastOnZigZagI -= method;
	}
	
	public override bool HasDelegates()
	{
		bool del1 = (this.broadcastOnZigZag  == null ? false : true);
		bool del2 = (this.broadcastOnZigZagI == null ? false : true);
		return (del1 || del2);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// OTHER METHODS: --------------------------------------------------------------------------------------------------

	private bool IsZigZag(TouchInfo touchInfo)
	{
		int positionsSize = touchInfo.GetPositionsSize();
		if (positionsSize < 4) return false;

		int zigZags = 1;

		int originIndex = touchInfo.GetFirstIndex();
		int currentIndex = touchInfo.GetNextIndex(originIndex);
		int previousIndex = currentIndex;

		Vector2 candidateDirection = (touchInfo.GetPosition(currentIndex) - touchInfo.GetPosition(originIndex));
		int candidateDirectionSize = 1;

		for (int i = 2; i < positionsSize; ++i)
		{
			previousIndex = currentIndex;
			currentIndex = touchInfo.GetNextIndex(currentIndex);

			Vector2 currentPosition = touchInfo.GetPosition(currentIndex);
			Vector2 previousPosition = touchInfo.GetPosition(previousIndex);

			Vector2 direction = (currentPosition - previousPosition);
			if (direction == Vector2.zero) continue;

			Vector2 candidate = candidateDirection / candidateDirectionSize;
			if (Vector2.Angle(direction, candidate) < MAX_ANGLE_THRESHOLD)
			{
				candidateDirection += direction;
				candidateDirectionSize++;

				if (candidateDirectionSize == MIN_POINTS_TO_CHANGE_DIR)
				{
					++zigZags;
				}
			}
			else
			{
				candidateDirection = direction;
				candidateDirectionSize = 1;
			}
		}

		if (zigZags > MIN_ZIGZAGS_TO_TRIGGER && zigZags < MAX_ZIGZAGS_TO_TRIGGER) return true;
		return false;
	}
}
