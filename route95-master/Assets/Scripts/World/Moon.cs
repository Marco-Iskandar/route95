using UnityEngine;
using System.Collections;

/// <summary>
/// Class to handle moon movement and lighting.
/// </summary>
public class Moon : GlobalLightSource {

	#region Moon Vars

	public static Moon instance;  // Quick reference to this instance
	Light _light;                 // Reference to this light component

	public Light shadowCaster;

	public float radius;
	public float scale;

	private Vector3 target; // target for the sun to point at: the car or the origin

	#endregion
	#region Unity Callbacks

	void Awake () {
		instance = this;
		GetComponent<Light>().cullingMask = 
			(1 << 0 | 1 << 1 | 1 << 2 | 1 << 4 | 1 << 5 | 1 << 8 | 1 << 9);
	}

	void Start () {
		transform.SetParent (PlayerMovement.instance.transform);
		transform.localScale = new Vector3 (scale, scale, scale);
	}

	void Update() {
		UpdateTransform();
	}

	#endregion
	#region Moon Callbacks
		
	private void UpdateTransform(){
		target = PlayerMovement.instance.transform.position;

		float newX = -radius * Mathf.Cos(WorldManager.instance.timeOfDay);
		float newY = -radius * Mathf.Sin(WorldManager.instance.timeOfDay);
		float newZ = -radius * Mathf.Cos(WorldManager.instance.timeOfDay + Mathf.PI/5);
		this.transform.position = new Vector3(newX, newY, newZ);

		this.transform.LookAt (target);
	}

	#endregion

}
