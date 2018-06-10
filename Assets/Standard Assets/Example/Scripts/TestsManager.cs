using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TestsManager : MonoBehaviour 
{
	// VARIABLES: ------------------------------------------------------------------------------------------------------

	public static TestsManager _instance;

	public Text console;

	// INITIALIZE: -----------------------------------------------------------------------------------------------------

	public void Awake()
	{
		TestsManager._instance = this;
		Application.targetFrameRate = 60;

		this.AddMessage("Console");
		SimpleGesture.OnTap(this.CallbackTap);
		SimpleGesture.On4AxisFlickSwipeUp(this.CallbackSwipeUp);
		SimpleGesture.On4AxisFlickSwipeDown(this.CallbackSwipeDown);
		SimpleGesture.On4AxisFlickSwipeLeft(this.CallbackSwipeLeft);
		SimpleGesture.OnCircle(this.CallbackCircle);
		SimpleGesture.OnZigZag(this.CallbackZigZag);

		SimpleGesture.WhilePanning(this.CallbackPanning);
		SimpleGesture.WhilePinching(this.CallbackPinching);
		SimpleGesture.WhileStretching(this.CallbackStretching);
		SimpleGesture.WhileTwisting(this.CallbackTwisting);
	}

	// PRIVATE METHODS: ------------------------------------------------------------------------------------------------

	private void AddMessage(string message)
	{
		this.console.text = message;
	}

	// CALLBACK METHODS: -----------------------------------------------------------------------------------------------

	public void CallbackTap()
	{
		this.AddMessage("Tap!");
	}

	public void CallbackSwipeUp()
	{
		this.AddMessage("Swipe Up!");
	}

	public void CallbackSwipeLeft()
	{
		this.AddMessage("Swipe Left!");
	}

	public void CallbackSwipeDown()
	{
		this.AddMessage("Swipe Down!");
	}

	public void CallbackCircle()
	{
		this.AddMessage("Circle!");
	}

	public void CallbackZigZag()
	{
		this.AddMessage("Zig-Zag!");
	}

	public void CallbackPanning(GestureInfoPan pan)
	{
		CameraController._instance.orbitSpeed += pan.deltaDirection.x;
		if (Mathf.Sign(CameraController._instance.orbitSpeed) != Mathf.Sign(pan.deltaDirection.x))
		{
			CameraController._instance.orbitSpeed = pan.deltaDirection.x;
		}

		this.AddMessage("Panning");

	}

	public void CallbackPinching(GestureInfoZoom zoom)
	{
		float fov = CameraController._instance.targetFOV + (zoom.deltaDistance * CameraController._instance.zoomCoeff);
		fov = Mathf.Min(CameraController._instance.maxZoom, fov);
		CameraController._instance.targetFOV = fov;

		this.AddMessage("Pinching");

	}

	public void CallbackStretching(GestureInfoZoom zoom)
	{
		float fov = CameraController._instance.targetFOV - (zoom.deltaDistance * CameraController._instance.zoomCoeff);
		fov = Mathf.Max(CameraController._instance.minZoom, fov);
		CameraController._instance.targetFOV = fov;

		this.AddMessage("Stretching");
	}

	public void CallbackTwisting(GestureInfoTwist twist)
	{
		if (twist.clockwise)
		{
			float rotation = CameraController._instance.targetZRotation + (twist.deltaDistance * CameraController._instance.rotCoeff);
			rotation = Mathf.Min(CameraController._instance.maxRotation, rotation);

			CameraController._instance.targetZRotation = rotation;
			this.AddMessage("Twisting Right");
		}
		else
		{
			float rotation = CameraController._instance.targetZRotation - (twist.deltaDistance * CameraController._instance.rotCoeff);
			rotation = Mathf.Max(CameraController._instance.minRotation, rotation);

			CameraController._instance.targetZRotation = rotation;
			this.AddMessage("Twisting Left");
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void RestartButton()
	{
		SceneManager.LoadScene(0);
	}
}
