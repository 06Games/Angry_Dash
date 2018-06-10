using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// +-------------------------------------------------------------------------------------------------------------------+
// | SIMPLE GESTURE RECOGNIZER v.2.2:                                                                                  |
// | 2015 - Martí-Joan Nogué Coll                                                                                      |
// |                                                                                                                   |
// | Please direct any bugs/comments/suggestions to hello@catsoft-studios.com                                          |
// | Documentation and questions, head to http://simplegesture.catsoft-studios.com                                     |
// +-------------------------------------------------------------------------------------------------------------------+

public partial class SimpleGesture : MonoBehaviour
{
	// ENUMS: ----------------------------------------------------------------------------------------------------------

	public enum E_DIRECTION
	{
		ANY, // any direction
		LFT, // left
		RHT, // right
		UPR, // up
		DWN, // down
		DUR, // diagonal up-right
		DDR, // diagonal down-right
		DUL, // diagonal up-left
		DDL  // diagonal down-left
	};

	// INSTANCE: -------------------------------------------------------------------------------------------------------

	private static SimpleGesture _instance = null;
	
	public static SimpleGesture Instance
	{
		get
		{
			if (SimpleGesture._instance == null)
			{
				GameObject sg = new GameObject("SimpleGesture");
				SimpleGesture._instance = sg.AddComponent<SimpleGesture>();
			}

			return SimpleGesture._instance; 
		}
	}

	public void OnDestroy()
	{
		foreach(BaseGesture gesture in this.oneFingerGestures) 
		{
			gesture.Delete();
		}

		foreach(BaseGesture gesture in this.twoFingerGestures) 
		{
			gesture.Delete();
		}

		this.oneFingerGestures.Clear();
		this.twoFingerGestures.Clear();
	}

	// ERROR MESSAGES: -------------------------------------------------------------------------------------------------

	public const string ERROR_ENUM_DOES_NOT_CORRESPOND = "ENUM DOES NOT MATCH WITH ANY OTHER";
	public const string ERROR_ANGLE_VALUE_NOT_CAUGHT   = "ANGLE VALUE DID NOT MATCH ANY CASE";

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// VARIABLES: ------------------------------------------------------------------------------------------------------

	public delegate void GestureDelegate();
	public delegate void GestureDelegate<T> (T bonus);

	private Dictionary<int, TouchInfo> touches = new Dictionary<int, TouchInfo>();
	public List<BaseGesture> oneFingerGestures = new List<BaseGesture>();
	public List<BaseGesture> twoFingerGestures = new List<BaseGesture>();

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// MAIN LOOP: ------------------------------------------------------------------------------------------------------
	
	private void Update()
	{
		for (int i = 0; i < Input.touchCount; ++i)
		{
			Touch touch = Input.GetTouch(i);
			int tID = touch.fingerId;

			// update touch positions: ---------------------------------------------------------------------------------

			if (touch.phase == TouchPhase.Began)
			{
				if (this.touches.ContainsKey(tID)) this.touches.Remove(tID);
				this.touches.Add(tID, new TouchInfo(touch.position));
			}

			if (!this.touches.ContainsKey(tID)) continue;

			this.touches[tID].AddPosition(touch);

			// update single finger gestures: --------------------------------------------------------------------------

			foreach(BaseGesture gesture in this.oneFingerGestures) 
			{
				gesture.OnPhase(touch.phase, this.touches[tID]);
			}
		}

		// update multi finger gestures: -------------------------------------------------------------------------------

		if (Input.touchCount == 2)
		{
			int tID1 = Input.GetTouch(0).fingerId;
			int tID2 = Input.GetTouch(1).fingerId;
			TouchPhase phase1 = Input.GetTouch(0).phase;
			TouchPhase phase2 = Input.GetTouch(1).phase;

			if (!this.touches.ContainsKey(tID1)) return;
			if (!this.touches.ContainsKey(tID2)) return;

			TouchPhase phase = TouchPhase.Stationary;
			if (phase1 == TouchPhase.Moved    || phase2 == TouchPhase.Moved)    phase = TouchPhase.Moved;
			if (phase1 == TouchPhase.Began    || phase2 == TouchPhase.Began)    phase = TouchPhase.Began;
			if (phase1 == TouchPhase.Ended    || phase2 == TouchPhase.Ended)    phase = TouchPhase.Ended;
			if (phase1 == TouchPhase.Canceled || phase2 == TouchPhase.Canceled) phase = TouchPhase.Canceled;

			foreach (BaseGesture gesture in this.twoFingerGestures)
			{
				gesture.OnPhase(phase, this.touches[tID1], this.touches[tID2]);
			}
		}
	}
}