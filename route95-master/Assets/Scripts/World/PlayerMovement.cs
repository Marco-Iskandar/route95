using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerMovement : MonoBehaviour {
	public static PlayerMovement instance;
	Road road;

	public GameObject lightRight;
	public GameObject lightLeft;
	//public float velocity;
	const float distPerBeat = 0.0012f; // Tempo -> velocity
	const float particlesPerUnit = 100f; // Distance -> particle emission
	const float lookAhead = 0.01f;
	public bool moving;
	public bool lights;
	public float progress;
	float roadHeight;
	public List<ParticleSystem> particles;

	public GameObject frontLeftWheel;
	public GameObject frontRightWheel;
	public GameObject backLeftWheel;
	public GameObject backRightWheel;

	float minVelocity;
	float maxVelocity;
	float velocity;
	const float velocityToRotation = 10000f;
	//float acceleration;
	Vector3 target;
	float offsetH = 0f;
	float velocityOffset = 0f;

	public AudioClip engineClip;

	List<ReflectionProbe> reflectionProbes;
	//Vector3 prevPosition;

	bool initialized = false;
	float dOffset;

	// Use this for initialization
	void Awake () {
		instance = this;
		lights = false;
		moving = false;
		velocity = 0f;

		//prevPosition = Vector3.zero;
		reflectionProbes = GetComponentsInChildren<ReflectionProbe> ().ToList<ReflectionProbe>();

		target = new Vector3 (0f, 0f, 0f);

		progress = 0f;
		StopMoving();

		minVelocity = MusicManager.tempoToFloat [Tempo.Slowest] * distPerBeat;
		maxVelocity = MusicManager.tempoToFloat [Tempo.Fastest] * distPerBeat;

		GetComponent<AudioSource>().volume = 0.0f;

		dOffset = 0f;



	}

	void Start () {
		road = WorldManager.instance.road;
	}

	public void StartMoving() {
		roadHeight = WorldManager.instance.roadHeight;
		moving = true;
		foreach (ParticleSystem ps in particles) ps.Play();
		GetComponent<AudioSource>().clip = engineClip;
		GetComponent<AudioSource>().loop = true;
		GetComponent<AudioSource>().Play();
		GetComponent<AudioSource>().volume = 0f;
		//EnableReflections();
	}

	public void StopMoving() {
		moving = false;
		foreach (ParticleSystem ps in particles) ps.Pause();
		GetComponent<AudioSource>().Stop();
		//DisableReflections();
	}

	public void DisableReflections () {
		foreach (ReflectionProbe probe in reflectionProbes)
			probe.enabled = false;
	}

	public void EnableReflections () {
		foreach (ReflectionProbe probe in reflectionProbes)
			probe.enabled = true;
	}

	void Initialize() {
		//DisableReflections ();
		initialized = true;
		//frontLeftWheel.transform.Rotate (new Vector3 (0f, 180f,0f));
		//frontLeftWheel.transform.Rotate (new Vector3 (0f, 180f,0f));
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!initialized)
			Initialize ();

		if (Sun.instance != null) {
			if (moving && !GameManager.instance.paused) {
				
				dOffset += (Mathf.PerlinNoise (Random.Range (0f, 1f), 0f) - 0.5f);
				velocityOffset = Mathf.Clamp (velocityOffset + dOffset, minVelocity, maxVelocity);

				velocity = MusicManager.tempoToFloat [MusicManager.instance.tempo] * distPerBeat + velocityOffset;

				progress += velocity * Time.fixedDeltaTime / road.CurveCount;
				if (progress >= 1f)
					progress = 1f;
				
				offsetH += (Mathf.PerlinNoise (Random.Range (0f, 1f), 0f) - Random.Range (0f, offsetH)) * Time.deltaTime;
				Vector3 offset = new Vector3 (offsetH, 2.27f + roadHeight, 0f);
				Vector3 point = road.GetPoint(progress);
				Vector3 ahead = point + road.GetVelocity (progress);
				transform.position = point + offset - 
					road.BezRight (point) * road.Width / 3f;
				
				transform.LookAt (ahead);
				float rotation = velocity * velocityToRotation * Time.deltaTime;

				//Vector2 point2 = new Vector2 (point.x, point.z);
				//Vector2 ahead2 = new Vector2 (ahead.x, ahead.z);

				frontLeftWheel.transform.Rotate (new Vector3 (rotation, 0f, 0f), Space.Self);
				frontRightWheel.transform.Rotate (new Vector3 (rotation, 0f, 0f),Space.Self);
				backLeftWheel.transform.Rotate (new Vector3 (rotation, 0f, 0f), Space.Self);
				backRightWheel.transform.Rotate (new Vector3 (rotation, 0f, 0f), Space.Self);

				//frontLeftWheel.transform.Rotate (new Vector3 (0f, Vector2.Angle (ahead2-point2, point2)-frontLeftWheel.transform.rotation.eulerAngles.y, 0f));
				//frontRightWheel.transform.Rotate (new Vector3 (0f, Vector2.Angle (ahead2-point2, point2)-frontRightWheel.transform.rotation.eulerAngles.y, 0f));

				foreach (ParticleSystem particle in particles) {
					var emission = particle.emission;
					var rate = emission.rate;
					rate.constantMax = velocity * particlesPerUnit;
					emission.rate = rate;
				}

				GetComponent<AudioSource>().pitch = Mathf.Clamp (0.75f + (velocity - minVelocity) / (maxVelocity - minVelocity), 0.75f, 1.75f);

			}

			lights = (WorldManager.instance.timeOfDay  > (Mathf.PI * (7f / 8f))
				|| WorldManager.instance.timeOfDay <= Mathf.PI * (1f / 8f));
			lightRight.GetComponent<Light> ().enabled = lights;
			lightLeft.GetComponent<Light> ().enabled = lights;
		}
	}

	void OnDrawGizmosSelected () {
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere (target, 1f);
	}
}
