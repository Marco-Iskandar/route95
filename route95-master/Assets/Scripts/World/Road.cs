	using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Road : Bezier {


	#region Road Vars

	public bool loading = false;
	public bool loaded = false;
	List<Vector3> toCheck;

	DynamicTerrain terrain;   // Reference to terrain

	float heightScale;        // World height scale (copied from WM)
	float generateRoadRadius; // distance from player to generate road (copied from WM)
	float cleanupRoadRadius;  // Distance from player to cleanup road (copied from WM)
	float variance;
	float placementDistance;  // marginal distance to add new road (copied from WM)
	float maxSlope;           // maximum slope per world unit (copied from WM)

	#endregion
	#region Road Mesh Vars

	int stepsPerCurve;        // number of steps per road curve (copied from WM)
	int steps = 0;            // current number of steps

	public float width;              // width of generated road (copied from WM)
	public float height;             // height of generated road (copied from WM)
	public float slope;              // ratio of top plane to bottom width (copied from WM)

	Mesh mesh;                // road mesh
	List<Vector3> verts;      // vertices in road mesh
	List<Vector2> uvs;        // road mesh UVs
	List<int> tris;           // road mesh triangles

	float UVProgress = 0f;    // current UV value to use when generating mesh

	#endregion
	#region Unity Callbacks

	void Awake () {

		toCheck = new List<Vector3>();

		verts = new List<Vector3> ();
		uvs = new List<Vector2> ();
		tris = new List<int> ();

		// Init mesh
		mesh = new Mesh();

		// Init road points
		points = new List<Vector3>();

	}

	public void Start () {

		// Copy vars from WM
		width = WorldManager.instance.roadWidth;
		height = WorldManager.instance.roadHeight;
		slope = WorldManager.instance.roadSlope;

		heightScale = WorldManager.instance.heightScale;
		generateRoadRadius = WorldManager.instance.roadExtendRadius;
		cleanupRoadRadius = WorldManager.instance.roadCleanupRadius;
		variance = WorldManager.instance.roadVariance;
		placementDistance = WorldManager.instance.roadPlacementDistance;
		maxSlope = WorldManager.instance.roadMaxSlope;

		stepsPerCurve = WorldManager.instance.roadStepsPerCurve;

		terrain = WorldManager.instance.terrain;
		
	}

	public void Update () {
		if (!loading) return;

		if (!loaded) {
			List<string> loadMessages = new List<string>() {
				"Blazing a trail...",
				"Rerouting...",
				"Following star maps..."
			};
			GameManager.instance.ChangeLoadingMessage(loadMessages.Random());
		}
			
		// These values may have changed, so get them from WM
		variance = WorldManager.instance.roadVariance;
		maxSlope = WorldManager.instance.roadMaxSlope;

		float progress = PlayerMovement.instance.progress;

		Vector3 playerPosition = PlayerMovement.instance.transform.position;

		bool changesMade = false;

		// Create new points in front of player
		if (Vector3.Distance (points.Tail(), PlayerMovement.instance.transform.position) < generateRoadRadius) {

			float numerator = progress * CurveCount;

			// Create curve
			AddCurve ();

			// Update player progress
			PlayerMovement.instance.progress = numerator / CurveCount;

			changesMade = true;
		} else

		// If road beginning is too close
		if (Vector3.Distance (points.Head(), playerPosition) < generateRoadRadius) {

			float numerator = progress * CurveCount * 2f;
			float denominatorOld = CurveCount * 2f;

			// Generate backwards
			Backtrack();

			// Update player progress
			PlayerMovement.instance.progress = (numerator + 2f) / (denominatorOld + 2f);

			changesMade = true;

		// If road beginning is too far away
		} else if (Vector3.Distance (points.Head(), playerPosition) > cleanupRoadRadius) {

			float numerator = progress * CurveCount * 2f;
			float denominator = CurveCount * 2f;

			// Remove first curve
			RemoveCurve();

			// Update player progress
			PlayerMovement.instance.progress = (numerator - 2f) / (denominator - 2f);

			changesMade = true;

		
		} else if (!loaded)  {
			loaded = true;
			PlayerMovement.instance.transform.position = GetPoint(0.6f) + new Vector3(0f, 2.27f + height, 0f);
			PlayerMovement.instance.transform.LookAt (GetPoint(0.6f) + GetVelocity(0.6f), Vector3.up);
			if (WorldManager.instance.doDecorate)
				WorldManager.instance.DoLoadDecorations();
			else WorldManager.instance.FinishLoading();
		}

		if (changesMade) Build();
		else if (toCheck.Count > 0) Check(toCheck.PopFront());

	}

	#endregion
	#region Road Methods

	/// <summary>
	/// Generates initial road points.
	/// </summary>
	public void Reset () {

		// Get initial point
		Vector3 point = new Vector3 (0f, heightScale, 0f);

		// Raycast down to terrain
		RaycastHit hit;
		if (Physics.Raycast (point, Vector3.down, out hit, Mathf.Infinity))
			point.y = hit.point.y;

		// Init points list
		points = new List<Vector3>() {
			point
		};

		// Init modes list
		modes = new List<BezierControlPointMode>() {
			BezierControlPointMode.Mirrored
		};

		AddCurve(true);
	}

	public float Width {
		get {
			return width;
		}
	}

	public void DoLoad () {
		// Build mesh
		Reset ();
		loading = true;
	}

	// Adds a new curve to the road bezier
	void AddCurve (bool ignoreMaxSlope=false) {
		float displacedDirection = placementDistance * variance; //placementRange;

		Vector3 point = points.Tail();
		Vector3 old = point;

		Vector3 direction;
		if (points.Count == 1) {
			direction = UnityEngine.Random.insideUnitSphere;
			direction.y = 0f;
			direction.Normalize();
			direction *= placementDistance;
		} else direction = GetDirection (1f) * placementDistance;

		RaycastHit hit;

		for (int i=3; i>0; i--) {
			float a = UnityEngine.Random.Range (0f, Mathf.PI * 2f);
			float d = UnityEngine.Random.Range (displacedDirection * 0.75f, displacedDirection);
			//float d = UnityEngine.Random.Range (0f, variance);

			//point += direction * (1f - d) * placementDistance + new Vector3 (Mathf.Cos(a), 0f, Mathf.Sin(a)) * d * placementDistance;
			point += direction + new Vector3 (Mathf.Cos(a), 0f, Mathf.Sin(a)) * d;
				
			Vector3 rayStart = point + new Vector3 (0f, heightScale, 0f);

			float dist = Vector2.Distance (new Vector2 (old.x, point.x), new Vector2 (old.z, point.z));
			if (Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity)) {
				if (ignoreMaxSlope) point.y = hit.point.y;
				else point.y += Mathf.Clamp(hit.point.y-point.y, -dist*maxSlope, dist*maxSlope);
			} else {
				throw new InvalidOperationException("Failed to place road point!");
			}
			points.Add(point);
		}

		terrain.OnExtendRoad ();

		modes.Add (modes.Tail());
		EnforceMode (points.Count - 4);
		steps += stepsPerCurve;
		DoBulldoze(PlayerMovement.instance.moving ? PlayerMovement.instance.progress : 0f);
	}

	public void Backtrack () {
		float displacedDirection = placementDistance * variance; //placementRange;

		Vector3 point = points.Head();
		Vector3 old = point;

		Vector3 direction = -GetDirection (0f) * placementDistance;

		RaycastHit hit;

		for (int i=3; i>0; i--) {
			float a = UnityEngine.Random.Range (0f, Mathf.PI * 2f);
			float d = UnityEngine.Random.Range (displacedDirection * 0.75f, displacedDirection);
			//float d = UnityEngine.Random.Range (0f, variance);

			//point += direction * (1f - d) * placementDistance + new Vector3 (Mathf.Cos(a), 0f, Mathf.Sin(a)) * d * placementDistance;
			point += direction + new Vector3 (Mathf.Cos(a), 0f, Mathf.Sin(a)) * d;

			Vector3 rayStart = point + new Vector3 (0f, heightScale, 0f);

			float dist = Vector2.Distance (new Vector2 (old.x, point.x), new Vector2 (old.z, point.z));

			if (Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity))
				point.y += Mathf.Clamp(hit.point.y-point.y, -dist*maxSlope, dist*maxSlope);

			points.Insert (0, point);
		}

		terrain.OnExtendRoad ();

		modes.Add (modes.Tail());
		EnforceMode (4);
		steps += stepsPerCurve;
		DoBulldoze(0f, 1f/(float)CurveCount);

	}

	/// <summary>
	/// Removes a curve behind the player.
	/// </summary>
	void RemoveCurve () {

		for (int i=0; i<3; i++) points.RemoveAt(0);
		modes.RemoveAt(0);

		steps -= stepsPerCurve;

	}

	// Marks all points between player and newly created points for leveling
	public void DoBulldoze (float startProgress, float endProgress=1f) {
		//StopCoroutine ("Bulldoze");
		StartCoroutine(Bulldoze( startProgress, endProgress));
	}

	IEnumerator Bulldoze (float startProgress, float endProgress) {
		float startTime = Time.realtimeSinceStartup;
		float progress = startProgress;
		float diff = endProgress - startProgress;
		if (diff < 0f) yield break;
		float resolution = WorldManager.instance.roadPathCheckResolution * diff;
		while (progress < endProgress) {
			Vector3 point = GetPoint(progress);
			toCheck.Add (point);
			progress += diff / resolution;

			if (Time.realtimeSinceStartup - startTime > GameManager.instance.targetDeltaTime) {
				yield return null;
				startTime = Time.realtimeSinceStartup;
			}
		}
		toCheck.Add (GetPoint(endProgress));
		yield return null;
	}

	void Check (Vector3 point) {
		terrain.vertexmap.DoCheckRoads(point);
	}

	// Sets the road mesh
	public void Build () {
		mesh.Clear();

		// Populate vertex, UV, and triangles lists
		BuildRoadMesh ();

		// Apply lists
		mesh.vertices = verts.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.triangles = tris.ToArray();

		// Recalculate properties
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.Optimize();

		// Set mesh
		GetComponent<MeshFilter> ().mesh = mesh;
		GetComponent<MeshFilter> ().sharedMesh = mesh;
	}

	// Calculates vertices, UVs, and tris for road mesh
	void BuildRoadMesh() {

		verts.Clear ();
		uvs.Clear ();
		tris.Clear ();

		float UVoffset = 0.2f;
		float UVslope = slope;

		float progressI = 0f;
		Vector3 pointI = GetPoint (progressI);
		Vector3 dirI = GetDirection(progressI);
		Vector3 rightI = BezRight (dirI);
		Vector3 downI = BezDown (dirI);

		// Left down
		verts.Add(pointI + width * -rightI);
		uvs.Add(new Vector2(-UVoffset, 0f));
		int leftDownI = 0;

		// Right down
		verts.Add(pointI + width *rightI);
		uvs.Add(new Vector2(1f + UVoffset, 0f));
		int rightDownI = 1;

		// Left up
		verts.Add(pointI + slope * width * -rightI + height * -downI);
		uvs.Add(new Vector2(-UVoffset + UVslope, 0f));
		int leftUpI = 2;

		// Right up
		verts.Add(pointI + slope * width * rightI + height * -downI);
		uvs.Add(new Vector2(1f + UVoffset - UVslope, 1f));
		int rightUpI = 3;

		bool flipUVs = true;

		for (int i = 1; i<steps; i++) {
			int num = i;

			float progressF = (float)(num) / (float)steps;
			Vector3 pointF = GetPoint (progressF);
			Vector3 dirF = GetDirection(progressF);
			Vector3 rightF = BezRight(dirF);
			Vector3 downF = BezDown(dirF);

			UVProgress += Vector3.Distance (pointF, pointI) / 20f;

			// Left down
			verts.Add(pointF + width * -rightF);
			uvs.Add(new Vector2(-UVoffset, UVProgress));
			int leftDownF = num * 4;

			// Right down
			verts.Add(pointF + width * rightF);
			uvs.Add(new Vector2(1f + UVoffset, UVProgress));
			int rightDownF = num * 4 + 1;

			// Left up
			verts.Add(pointF + slope * width * -rightF + height * -downF);
			uvs.Add(new Vector2(-UVoffset + UVslope, UVProgress));
			int leftUpF = num * 4 + 2;

			// Right up
			verts.Add(pointF + slope * width * rightF + height * -downF);
			uvs.Add(new Vector2(1f + UVoffset - UVslope, UVProgress));
			int rightUpF = num * 4 + 3;


			// Left slope
			tris.Add (leftDownI);
			tris.Add (leftUpI);
			tris.Add (leftDownF);

			tris.Add (leftDownF);
			tris.Add (leftUpI);
			tris.Add (leftUpF);


			// Right slope
			tris.Add (rightUpI);
			tris.Add (rightDownI);
			tris.Add (rightDownF);

			tris.Add (rightUpF);
			tris.Add (rightUpI);
			tris.Add (rightDownF);


			// Road surface plane
			tris.Add (leftUpF);
			tris.Add (rightUpI);
			tris.Add (rightUpF);

			tris.Add (leftUpI);
			tris.Add (rightUpI);
			tris.Add (leftUpF);


			progressI = progressF;
			pointI = pointF;
			leftDownI = leftDownF;
			rightDownI = rightDownF;
			leftUpI = leftUpF;
			rightUpI = rightUpF;

			flipUVs = !flipUVs;
		}
	}

	#endregion
}
