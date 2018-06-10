using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
	public static CameraController _instance;

	//private Vector3 originPos = new Vector3(0.0f, 5.0f, -40.0f);
	//private Vector3 originRot = new Vector3(8.0f, 0.0f, 0.0f);

	public float targetFOV = 60.0f;
	private float smooth = 0.03f;
	public float minZoom = 30.0f;
	public float maxZoom = 90.0f;
	public float zoomCoeff = 1.0f;

	public float targetZRotation = 0.0f;
	public float maxRotation =  35.0f;
	public float minRotation = -35.0f;
	public float rotCoeff = 1.0f;
	
	[HideInInspector]
	public float orbitSpeed = 0.0f;

	public void Awake()
	{
		CameraController._instance = this;
	}

	public void Start()
	{
		//this.transform.position = this.originPos;
		//this.transform.rotation = Quaternion.Euler(this.originRot);
	}

	public void Update()
	{
		float velocity = 0.0f;
		float fov = Mathf.SmoothDamp(Camera.main.fieldOfView, this.targetFOV, ref velocity, this.smooth);
		Camera.main.fieldOfView = fov;
		
		Camera.main.transform.parent.RotateAround (Vector3.zero, Vector3.up, this.orbitSpeed * Time.deltaTime);
		float bin = 0.0f;
		this.orbitSpeed = Mathf.SmoothDamp(this.orbitSpeed, 0.0f, ref bin, 0.2f);
		
		float rot = Mathf.SmoothDampAngle(Camera.main.transform.rotation.eulerAngles.z, this.targetZRotation, ref velocity, this.smooth);
		Quaternion rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, rot));
		
		Camera.main.transform.localRotation = rotation;
	}
}
