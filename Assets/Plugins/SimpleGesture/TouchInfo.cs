using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TouchInfo
{
	private const int MAX_POSITIONS = 200;

	// VARIABLES: ------------------------------------------------------------------------------------------------------

	private bool positionsOverflow;
	private int positionsIndex;
	private Vector2[] positions;

	private Vector2 avgPosition;
	private int numPositions;

	private float totalAngle;
	private float totalDistance;
	private float startTime;

	private Vector2 lastDeltaPosition;

	// CONSTRUCTOR: ----------------------------------------------------------------------------------------------------

	public TouchInfo(Vector2 position)
	{
		this.positionsOverflow = false;
		this.positionsIndex    = 0;
		this.positions         = new Vector2[TouchInfo.MAX_POSITIONS];
		this.positions[0]      = position;

		this.avgPosition       = Vector2.zero;
		this.numPositions      = 0;

		this.totalDistance     = 0.0f;
		this.startTime         = Time.time;
	}

	// PUBLIC METHODS: -------------------------------------------------------------------------------------------------

	public void AddPosition(Touch touch)
	{
		if (touch.phase == TouchPhase.Stationary) return;

		Vector2 curPosition = touch.position;

		this.positions[this.positionsIndex] = curPosition;
		this.avgPosition += curPosition;
		this.numPositions++;

		this.lastDeltaPosition = touch.deltaPosition;

		this.totalDistance += this.CalculateDistance();

		this.IncrementPositions();
	}

	public List<Vector2> GetPositions()
	{
		List<Vector2> positions = new List<Vector2>();
		if (this.positionsOverflow)
		{
			for (int i = this.GetFirstIndex(); i < TouchInfo.MAX_POSITIONS; ++i)
			{
				positions.Add(this.positions[i]);
			}

			for (int i = 0; i <= this.positionsIndex; ++i)
			{
				positions.Add(this.positions[i]);
			}
		}
		else
		{
			for (int i = 0; i <= this.positionsIndex; ++i)
			{
				positions.Add(this.positions[i]);
			}
		}

		return positions;
	}

	public Vector2 GetPosition(int index)
	{
		return this.positions[index];
	}

	public Vector2 GetFirstPosition()
	{
		return this.positions[this.GetFirstIndex()];
	}

	public Vector2 GetLastPosition()
	{
		return this.positions[this.GetLastIndex()];
	}

	public Vector2 GetRandomPosition()
	{
		int size = (this.positionsOverflow ? MAX_POSITIONS : this.positionsIndex);
		return this.positions[Random.Range(0, size)];
	}

	public Vector2 GetDecimatedPosition(int index, int nChunks)
	{
		int size = (this.positionsOverflow ? MAX_POSITIONS : this.positionsIndex);
		//Debug.Log(index + ": " + (index * size/nChunks) + " of " + size);
		if (index == 0) return this.positions[this.GetFirstIndex()];
		if (index == nChunks - 1) return this.positions[this.GetLastIndex()];
		return this.positions[ index * (size/nChunks) ];
	}
	
	public int GetPreviousIndex(int index)
	{
		return (--index >= 0 ? index : TouchInfo.MAX_POSITIONS - 1);
	}
	
	public int GetNextIndex(int index)
	{
		return (index++ < TouchInfo.MAX_POSITIONS ? index : 0);
	}
	
	public int GetFirstIndex()
	{
		return (this.positionsOverflow ? this.GetNextIndex(this.positionsIndex) : 0);
	}

	public int GetLastIndex()
	{
		if (this.positionsIndex == 0)
		{
			if (this.positionsOverflow) return MAX_POSITIONS - 1;
			else return 0;
		}

		return this.positionsIndex - 1;
	}

	public int GetPositionsSize()
	{
		return (this.positionsOverflow ? MAX_POSITIONS : this.positionsIndex + 1);
	}

	public float GetTotalDistance()
	{
		return this.totalDistance;
	}

	public float GetStartTime()
	{
		return this.startTime;
	}

	public Vector2 GetAveragePosition()
	{
		return this.avgPosition/(float)this.numPositions;
	}

	public Vector2 GetDeltaPosition()
	{
		return this.lastDeltaPosition;
	}

	// PRIVATE METHODS: ------------------------------------------------------------------------------------------------

	private void IncrementPositions()
	{
		this.positionsIndex++;
		if (this.positionsIndex >= TouchInfo.MAX_POSITIONS)
		{
			this.positionsIndex = 0;
			this.positionsOverflow = true;
		}
	}

	/*
	if (direction0 != Vector2.zero && direction1 != Vector2.zero)
	{
		ANGLE WITH SIGN. **NOT** USEFULL FOR THE MOMENT:
		angle = Vector2.Angle(direction0, direction1);
		Vector3 cross = Vector3.Cross(direction0, direction1);
		if (cross.z > 0.0f) angle = -angle;

	}
	*/

	private float CalculateDistance()
	{
		if (!this.positionsOverflow && this.positionsIndex <= 0) return 0.0f;

		return Vector2.Distance(
			this.positions[this.GetPreviousIndex(this.positionsIndex)],
			this.positions[this.positionsIndex]
		);
	}
}
