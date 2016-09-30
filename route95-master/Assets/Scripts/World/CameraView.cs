using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Structure to hold all camera view information.
/// </summary>
public class CameraView {

	#region CameraView Enums

	/// <summary>
	/// Setup mode camera views.
	/// </summary>
	public enum View {
		OutsideCar,
		Driving,
		Radio,
		Chase
	}

	/// <summary>
	/// Type of camera placement.
	/// </summary>
	public enum CameraPlacementMode {
		Fixed,
		RandomSky,
		RandomGround
	}

	/// <summary>
	/// Type of camera follow.
	/// </summary>
	public enum CameraFollowMode {
		Lead, // Points in front of target
		Static,
		Shaky
	}

	#endregion
	#region CameraView Vars

	public string name;                       // Name
	public Transform transform;               // Transform to use
	public float fov;                         // Field of view
	public CameraFollowMode followMode;       // Type of camera following
	public CameraPlacementMode placementMode; // Type of camera placement
	public Vector3 pos;                       // Current position of camera
	public Quaternion rot;                    // Current rotation of camera

	public Vector3 targetPos;                 // Target transform
	public Quaternion targetRot;              // Target rotation

	public float lag;                         // How tightly camera follows (lower = tighter)
	public float shake;                       // Amount of camera shake

	#endregion
}

