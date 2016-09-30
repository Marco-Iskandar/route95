using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class to manage the game camera.
/// </summary>
public class CameraControl : MonoBehaviour {

	#region CameraControl Enums

	/// <summary>
	/// Current state of the game camera.
	/// </summary>
	public enum State {
		Setup,
		Live,
		Free
	}

	/// <summary>
	/// Current type of camera control.
	/// </summary>
	public enum CameraControlMode {
		Manual, // Angles only change on user input
		Random  // Angles cycle randomly through all available angles
	}

	#endregion
	#region CameraControl Vars

	[Header("General camera settings")]

	public static CameraControl instance; // Quick reference to this instance

	[Tooltip("Current camera state.")]
	public State state = State.Setup;

	[Tooltip("Free camera rotate sensitivity.")]
	public float rotateSensitivity = 0.25f;

	[Tooltip("Free camera movement sensitivity.")]
	public float moveSensitivity = 0.4f;

	[Tooltip("Camera sway speed.")]
	public float swaySpeed = 1f;

	[Tooltip("Camera base sway amount.")]
	public float baseSway = 1f;

	public bool doSway = true;

	const float DEFAULT_SPEED = 1f;     // Default camera speed
	const float DEFAULT_FOV = 75f;

	public GameObject CameraBlocker;

	// Camera interp vars
	CameraView initialView;
	Transform startTransform;                      // Camera lerp start transform
	float startFOV;
	CameraView targetView;

	float speed;						  // Camera speed
	bool moving = false;                  // Is the camera currently lerping?

	[SerializeField]
	float progress = 0f;                  // Lerp progress

	// Live mode camera angle vars
	CameraView currentAngle;              // Current live mode angle
	List<CameraView> liveAngles;          // List of all live mode angles

	[SerializeField]
	float transitionTimer;                // Timer to next angle change
	bool paused = false;                  // Is live mode paused?
	float resetDistance;

	[Tooltip("Initial position for camera")]
	//public Transform initialView;

	public Transform ViewOutsideCar;
	public Transform ViewDriving;
	public Transform ViewRadio;
	public Transform ViewChase;

	public CameraView OutsideCar;
	public CameraView Driving;
	public CameraView Radio;

	public Transform HoodForward;
	public Transform HoodBackward;
	public Transform NearChase;
	public Transform FarChase;
	public Transform FrontLeftWheel;
	public Transform FrontRightWheel;
	public Transform RearLeftWheel;
	public Transform RearRightWheel;
	public Transform WideRear;
	public Transform WideFront;

	[Tooltip("Frequency with which to automatically change camera angle in live mode.")]
	[Range(1f, 600f)]
	public float liveModeTransitionFreq;

	[Tooltip("Initial control mode.")]
	public CameraControlMode controlMode = CameraControlMode.Random;

	int targetAngle = -1;

	// Mappings of keys to camera angle indices
	static Dictionary <KeyCode, int> keyToView = new Dictionary<KeyCode, int> () {
		{ KeyCode.F1, 0 },
		{ KeyCode.F2, 1 },
		{ KeyCode.F3, 2 },
		{ KeyCode.F4, 3 },
		{ KeyCode.F5, 4 },
		{ KeyCode.F6, 5 },
		{ KeyCode.F7, 6 },
		{ KeyCode.F8, 7 },
		{ KeyCode.F9, 8 },
		{ KeyCode.F10, 9 }
	};

	#endregion
	#region Unity Callbacks

	void Awake () {
		instance = this;
		transitionTimer = liveModeTransitionFreq;
		speed = DEFAULT_SPEED;

		// Outside car
		OutsideCar = new CameraView () {
			name = "OutsideCar",
			transform = ViewOutsideCar,
			targetPos = ViewOutsideCar.position,
			targetRot = ViewOutsideCar.rotation,
			fov = DEFAULT_FOV,
			followMode = CameraView.CameraFollowMode.Static
		};

		// Driving
		Driving = new CameraView () {
			name = "Driving",
			transform = ViewDriving,
			targetPos = ViewDriving.position,
			targetRot = ViewDriving.rotation,
			fov = DEFAULT_FOV,
			followMode = CameraView.CameraFollowMode.Static
		};

		// Radio
		Radio = new CameraView () {
			name = "Radio",
			transform = ViewRadio,
			targetPos = ViewRadio.position,
			targetRot = ViewRadio.rotation,
			fov = 20f,
			followMode = CameraView.CameraFollowMode.Static
		};

		initialView = OutsideCar;
		SnapToView (initialView);

		// Init live mode angles
		liveAngles  = new List<CameraView> () {

			// On the hood, forwards
			new CameraView () {
				name = "HoodForward",
				transform = HoodForward,
				targetPos = HoodForward.position,
				targetRot = HoodForward.rotation,
				fov = DEFAULT_FOV,
				followMode = CameraView.CameraFollowMode.Static
			},

			// Near chase
			new CameraView () {
				name = "NearChase",
				transform = NearChase,
				targetPos = NearChase.position,
				targetRot = NearChase.rotation,
				fov = DEFAULT_FOV,
				followMode = CameraView.CameraFollowMode.Lead,
				placementMode = CameraView.CameraPlacementMode.Fixed,
				lag = 0.04f
			},

			// Far chase
			new CameraView () {
				name = "FarChase",
				transform = FarChase,
				targetPos = FarChase.position,
				targetRot = FarChase.rotation,
				fov = DEFAULT_FOV,
				followMode = CameraView.CameraFollowMode.Lead,
				placementMode = CameraView.CameraPlacementMode.Fixed,
				lag = 0.2f
			},

			// Front right wheel
			new CameraView () {
				name = "FrontRightWheel",
				transform = FrontRightWheel,
				targetPos = FrontRightWheel.position,
				targetRot = FrontRightWheel.rotation,
				fov = DEFAULT_FOV,
				followMode = CameraView.CameraFollowMode.Static,
				placementMode = CameraView.CameraPlacementMode.Fixed
			},

			// Front left wheel
			new CameraView () {
				name = "FrontLeftWheel",
				transform = FrontLeftWheel,
				targetPos = FrontLeftWheel.position,
				targetRot = FrontLeftWheel.rotation,
				fov = DEFAULT_FOV,
				followMode = CameraView.CameraFollowMode.Static,
				placementMode = CameraView.CameraPlacementMode.Fixed
			},

			// Rear right wheel
			new CameraView () {
				name = "RearRightWheel",
				transform = RearRightWheel,
				targetPos = RearRightWheel.position,
				targetRot = RearRightWheel.rotation,
				fov = DEFAULT_FOV,
				followMode = CameraView.CameraFollowMode.Static,
				placementMode = CameraView.CameraPlacementMode.Fixed
			},

			// Rear left wheel
			new CameraView () {
				name = "RearLeftWheel",
				transform = RearLeftWheel,
				targetPos = RearLeftWheel.position,
				targetRot = RearLeftWheel.rotation,
				fov = DEFAULT_FOV,
				followMode = CameraView.CameraFollowMode.Static,
				placementMode = CameraView.CameraPlacementMode.Fixed
			},

			// Rear left wheel
			new CameraView () {
				name = "RearLeftWheel",
				transform = RearLeftWheel,
				targetPos = RearLeftWheel.position,
				targetRot = RearLeftWheel.rotation,
				fov = DEFAULT_FOV,
				followMode = CameraView.CameraFollowMode.Static,
				placementMode = CameraView.CameraPlacementMode.Fixed
			},

			// Far top
			new CameraView () {
				name = "FarTop",
				targetPos = PickRandomPosition (25f, 50f),
				fov = 60f,
				followMode = CameraView.CameraFollowMode.Shaky,
				placementMode = CameraView.CameraPlacementMode.RandomSky
			},

			// Distant
			new CameraView () {
				name = "Distant",
				targetPos = PickRandomPosition (10f, 20f),
				fov = 60f,
				followMode = CameraView.CameraFollowMode.Shaky,
				placementMode = CameraView.CameraPlacementMode.RandomGround
			}
		};

		foreach (CameraView angle in liveAngles) {
			angle.pos = angle.targetPos;
			angle.rot = angle.targetRot;
		}
	}

	void Start () {
		resetDistance = WorldManager.instance.chunkLoadRadius * 
			0.5f * WorldManager.instance.chunkSize;
		currentAngle = OutsideCar;
	}

	void Update() {

		switch (state) {
		case State.Setup:
			if (moving) {

				if (progress < 1f) {

					progress += speed * Time.deltaTime;

					// Lerp position
					transform.position = Vector3.Slerp(startTransform.position, targetView.transform.position, progress);

					// Lerp Rotation
					transform.rotation = Quaternion.Slerp(startTransform.rotation, targetView.transform.rotation, progress);

					Camera.main.fieldOfView = Mathf.Lerp (startFOV, targetView.fov, Mathf.Sqrt(progress));

				} else {
					moving = false;
					startTransform = targetView.transform;
					transform.position = startTransform.position;
					transform.rotation = startTransform.rotation;
					OnCompleteLerp();
				}
			}
			break;


		case State.Live:
			if (!paused) {

				// Check each mapped key for input
				foreach (KeyCode key in keyToView.Keys) {
					if (Input.GetKeyDown (key) && CameraBlocker.GetComponent<Fadeable>().NotFading) {
						if (controlMode != CameraControlMode.Manual)
							controlMode = CameraControlMode.Manual;
						StartFade();
						targetAngle = keyToView[key];
					}
				}
					
				UpdateAllAngles();

				// Move camera to current angle position
				transform.position = currentAngle.pos;
				transform.rotation = currentAngle.rot;

				// Update transition timer
				if (controlMode == CameraControlMode.Random) {
					if (transitionTimer <= 0f) {
						transitionTimer = liveModeTransitionFreq;
						targetAngle = -1;
						StartFade();
					}  else transitionTimer--;
				}

				if (CameraBlocker.GetComponent<Fadeable>().DoneUnfading) {
					GameManager.instance.Hide(CameraBlocker);
					if (targetAngle == -1) ChangeAngle();
					else ChangeAngle(targetAngle);
				}

				// Check if camera is out of range
				float distToPlayer = Vector3.Distance (transform.position, PlayerMovement.instance.transform.position);
				if (distToPlayer > resetDistance && !CameraBlocker.GetComponent<Fadeable>().busy)
					StartFade();
			}
			break;

		case State.Free:

			// Rotate camera
			Vector3 d = InputManager.instance.mouseDelta;
			Vector3 old = transform.rotation.eulerAngles;
			bool slow = Input.GetAxisRaw("Slow") != 0f;
			old.z = 0f;
			old.x += -d.y * rotateSensitivity * (slow ? 0.05f : 1f);
			old.y += d.x * rotateSensitivity * (slow ? 0.05f : 1f);
			transform.rotation = Quaternion.Euler(old);

			// Translate camera
			float forward = Input.GetAxisRaw("Forward") * moveSensitivity * (slow ? 0.05f : 1f);
			float up = Input.GetAxisRaw("Up") * moveSensitivity * (slow ? 0.05f : 1f);
			float right = Input.GetAxisRaw("Right") * moveSensitivity * (slow ? 0.05f : 1f);
			transform.Translate(new Vector3 (right, up, forward));

			break;

		}

		if (doSway) {

			// Calculate sway
			float bx = ((Mathf.PerlinNoise (0f, Time.time*swaySpeed)-0.5f)) * baseSway * Camera.main.fieldOfView / DEFAULT_FOV;
			float by = ((Mathf.PerlinNoise (0f, (Time.time*swaySpeed)+100f))-0.5f) * baseSway * Camera.main.fieldOfView / DEFAULT_FOV;

			// Do sway
			transform.Rotate (bx, by, 0f);
		}
	}

	#endregion
	#region CameraControl Live Mode Methods

	/// <summary>
	/// Start camera live mode.
	/// </summary>
	public void StartLiveMode () {
		if (state == State.Free) return;
		state = State.Live;
		ChangeAngle();
		paused = false;
	}

	/// <summary>
	/// Pause camera live mode.
	/// </summary>
	public void StopLiveMode () {
		state = State.Setup;
		paused = true;
	}

	/// <summary>
	/// Start camera free mode.
	/// </summary>
	public void StartFreeMode () {
		state = State.Free;
		transform.rotation = Quaternion.identity;
		GameManager.instance.MoveCasetteBack();
		GameManager.instance.HideAll();
		GameManager.instance.Hide(GameManager.instance.systemButtons);
		GameManager.instance.Hide(GameManager.instance.exitButton);
		Cursor.lockState = CursorLockMode.Confined;
		Cursor.visible = false;
	}

	/// <summary>
	/// Stop camera free mode.
	/// </summary>
	public void StopFreeMode () {
		state = State.Setup;
	}

	public void StartFade () {
		GameManager.instance.Show(CameraBlocker);
	}

	/// <summary>
	/// Pause camera movement.
	/// </summary>
	public void Pause () {
		paused = true;
	}

	/// <summary>
	/// Unpause camera movement.
	/// </summary>
	public void Unpause () {
		paused = false;
	}

	/// <summary>
	/// Pick a random live camera angle.
	/// </summary>
	public void ChangeAngle () {
		ChangeAngle (Random.Range(0, liveAngles.Count));
	}

	/// <summary>
	/// Pick a specific live camera angle.
	/// </summary>
	/// <param name="camView"></param>
	public void ChangeAngle (int camView) {
		currentAngle = liveAngles[camView];
		GetComponent<Camera>().fieldOfView = currentAngle.fov;
		//if (Debug.isDebugBuild) 
			//Debug.Log("CameraControl.ChangeAngle(): switch to view \"" + currentAngle.name +".\"");
		
		switch (currentAngle.placementMode) {

			case CameraView.CameraPlacementMode.Fixed:
				break;

			case CameraView.CameraPlacementMode.RandomGround:
				currentAngle.targetPos = PickRandomPosition (10f, 20f);
				break;
			case CameraView.CameraPlacementMode.RandomSky:
				currentAngle.targetPos = PickRandomPosition (25f, 50f);
				break;
		}
	}

	/// <summary>
	/// Warms all live mode camera angles.
	/// </summary>
	void UpdateAllAngles () {
		foreach (CameraView angle in liveAngles) {
			if (angle.transform != null) {
				angle.targetPos = angle.transform.position;
				angle.targetRot = angle.transform.rotation;
			}

			switch (angle.followMode) {

				case CameraView.CameraFollowMode.Lead:
					//angle.pos = Vector3.Lerp(angle.pos, angle.targetPos, angle.lag);
					//angle.pos = angle.pos + (angle.targetPos - angle.pos) * angle.lag;
					angle.pos = angle.targetPos;
					angle.rot = Quaternion.LookRotation (PlayerMovement.instance.transform.position + PlayerMovement.instance.transform.forward *20f - angle.pos, Vector3.up);
					break;

				case CameraView.CameraFollowMode.Static:
					angle.pos = angle.targetPos;
					angle.rot = angle.targetRot;
					break;

				case CameraView.CameraFollowMode.Shaky:
					angle.pos = angle.targetPos;
				transform.LookAt (PlayerMovement.instance.transform.position, Vector3.up);
					angle.rot = transform.rotation;
					break;
			}

		}
	}

	/// <summary>
	/// Picks a random position.
	/// </summary>
	/// <param name="minHeight"></param>
	/// <param name="maxHeight"></param>
	/// <returns></returns>
	Vector3 PickRandomPosition (float minHeight, float maxHeight) {
		float chunkSize = WorldManager.instance.chunkSize;

		Vector3 point = new Vector3 (
			PlayerMovement.instance.transform.position.x + Random.Range (-chunkSize / 2f, chunkSize / 2f),
			0f,
			PlayerMovement.instance.transform.position.z + Random.Range (-chunkSize / 2f, chunkSize / 2f)
		);

		Vector3 rayOrigin = new Vector3 (point.x, WorldManager.instance.heightScale, point.z);

		RaycastHit hit;
		if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity))
			point.y = hit.point.y;

		point.y += Random.Range (minHeight, maxHeight);

		return point;
	}

	#endregion
	#region CameraControl Non-Live Mode Methods

	/// <summary>
	/// Sets the camera lerp speed.
	/// </summary>
	/// <param name="newSpeed"></param>
	public void SetSpeed (float newSpeed) {
		speed = newSpeed;
	}

	/// <summary>
	/// Teleports the camera to a position.
	/// </summary>
	/// <param name="newPosition"></param>
	public void SnapToView (CameraView newView) {
		targetView = newView;
		startTransform = newView.transform;
		transform.position = newView.transform.position;
		transform.rotation = newView.transform.rotation;
		Camera.main.fieldOfView = newView.fov;
	}

	public void LerpToView (CameraView newView, float newSpeed= DEFAULT_SPEED) {
		if (targetView == newView) {
			OnCompleteLerp();
			return;
		}

		CameraView oldView = targetView;
		startFOV = oldView.fov;
		startTransform = oldView.transform;

		targetView = newView;
		moving = true;
		speed = newSpeed;
		progress = 0f;
	}

	void OnCompleteLerp () {
		GameManager.instance.AttemptMoveCasette();
	}
		
	#endregion
		
}
