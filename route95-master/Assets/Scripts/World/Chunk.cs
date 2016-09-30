using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ChunkComparer : IComparer<Chunk> {

	int IComparer<Chunk>.Compare(Chunk x, Chunk y) {
		return x.priority.CompareTo(y.priority);
	}

}

 /// <summary>
 /// A chunk is a square mesh that's part of the larger terrain object.
 /// Chunks dynamically load and unload as the player gets closer and farther.
 /// This un/loading will be taken care of by the DynamicTerrain class. 
 /// </summary>
public class Chunk: MonoBehaviour, IComparable<Chunk>, IPoolable {

	#region Chunk Vars

	public int x;                          // x position in chunk grid
	public int y;                          // y position in chunk grid

	int numVerts;                          // number of vertices used in the mesh
	Mesh mesh;                             // 3D mesh to use for the chunk                 
	Vector3[] verts;                       // vertices used in the mesh
	int[] triangles;                       // triangles used in the mesh
	Vector3[] normals;                     // normals used for each mesh vertex
	Vector2[] uvs;                         // UVs used for each mesh vertex
	Color[] colors;                        // colors at each mesh vertex

	IntVector2[] coords;                   // vertex map coordinates of each mesh vertex
	Vertex[] mapVerts;                     // references to representative vertex map vertices for each mesh vertex

	DynamicTerrain terrain;                // reference to parent terrain
	VertexMap vmap;                        // reference to vertex map to use
	public List<GameObject> decorations;   // list of decorations parented to this chunk

	static int chunkRes;                          // chunk resolution (copied from WM)
	static float chunkSize;                       // width of a chunk in world units (copied from WM)

	public float priority = 0f;            // this chunk's priority when considering chunks to update.

	public GameObject grassEmitter;        // this chunk's grass emitter

	public bool hasCheckedForRoad = false; // has this chunk checked for roads?
	public bool hasRoad = false;           // chunk has road on it
	public bool nearRoad = false;          // chunk is within one chunk distance of a road
	bool isUpdatingVerts = false;          // is the chunk currently updating its vertices?
	bool needsColliderUpdate = false;      // does the chunk need a collider update?
	bool needsColorUpdate = false;         // does the chunk need a color update?

	#endregion
	#region IComparable Implementations

	int IComparable<Chunk>.CompareTo(Chunk other) {
		if (other == null)
			throw new ArgumentException ("Other object not a chunk!");

		if (priority > other.priority) return 1;
		else if (priority == other.priority) return 0;
		else return -1;
	}

	#endregion
	#region IPoolable Implementations

	void IPoolable.OnPool() {
		gameObject.SetActive(false);
		priority = 0;
	}

	void IPoolable.OnDepool() {
		gameObject.SetActive(true);
	}

	#endregion
	#region Chunk Methods

	/// <summary>
	/// Initializes a brand new chunk at x and y.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public void Initialize (int x, int y) {

		terrain = WorldManager.instance.terrain;

		// Init vars
		this.x = x;
		this.y = y;

		vmap = terrain.vertexmap;
		chunkRes = WorldManager.instance.chunkResolution;
		chunkSize = WorldManager.instance.chunkSize;

		// Generate vertices
		verts = CreateUniformVertexArray (chunkRes);
		numVerts = verts.Length;

		// Generate triangles
		triangles = CreateSquareArrayTriangles (chunkRes);

		// Init normals
		normals = new Vector3[numVerts];

		// Generate UVs
		uvs = CreateUniformUVArray (chunkRes);

		// Init colors
		colors = new Color[numVerts];

		// Init coords and mapVerts
		coords = new IntVector2[numVerts];
		mapVerts = new Vertex[numVerts];

		// Build initial chunk mesh
		mesh = CreateChunkMesh();

		// Assign mesh
		GetComponent<MeshFilter>().mesh = mesh;

		// Move GameObject to appropriate position
		transform.position = new Vector3 (x * chunkSize - chunkSize/2f, 0f, y * chunkSize - chunkSize/2f);

		// Initialize name
		gameObject.name = "Chunk ("+x+","+y+") Position:"+transform.position.ToString();

		// Init decorations list
		decorations = new List<GameObject>();

		// Register all vertices with vertex map
		// Move vertices, generate normals/colors
		for (int i=0; i<numVerts; i++) {

			// Init normal/color
			normals [i] = Vector3.up;
			colors[i] = Color.white;

			// Get VMap coords
			IntVector2 coord = IntToV2 (i);
			coords[i] = coord;

			// Get corresponding vertex
			mapVerts[i] = vmap.VertexAt (coord, true);

			// If vertex exists, get height
			UpdateVertex (i, mapVerts[i].height);
			UpdateColor (i, mapVerts[i].color);
		}

		// Assign material
		MeshRenderer renderer = GetComponent<MeshRenderer> ();
		renderer.sharedMaterial = WorldManager.instance.terrainMaterial;
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
		renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

		// Assign collision mesh
		MeshCollider collider = GetComponent<MeshCollider>();
		collider.sharedMesh = mesh;
		collider.convex = false;

		// Init rigidbody
		Rigidbody rigidbody = GetComponent<Rigidbody>();
		rigidbody.freezeRotation = true;
		rigidbody.isKinematic = true;
		rigidbody.useGravity = false;
		rigidbody.constraints = RigidbodyConstraints.FreezeAll;

		// Add grass system
		grassEmitter = GameObject.Instantiate (WorldManager.instance.grassEmitterTemplate);
		grassEmitter.transform.parent = transform;
		grassEmitter.transform.localPosition = Vector3.zero;

		// Randomize grass density
		ParticleSystem sys = grassEmitter.GetComponent<ParticleSystem>();
		sys.maxParticles = UnityEngine.Random.Range(0,WorldManager.instance.grassPerChunk);
		sys.playOnAwake = true;

		// Assign particle system emission shape
		ParticleSystem.ShapeModule shape = sys.shape;
		shape.mesh = mesh;

		// Assign particle system emission rate
		ParticleSystem.EmissionModule emit = sys.emission;
		emit.rate = new ParticleSystem.MinMaxCurve(WorldManager.instance.decorationsPerStep);

		UpdateCollider();
	}

	/// <summary>
	/// Resets necessary variables after de-pooling a chunk.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public void Reuse (int x, int y) {
		terrain = WorldManager.instance.terrain;

		// Update vars
		this.x = x;
		this.y = y;

		// Move chunk to appropriate position
		transform.position = new Vector3 (x * chunkSize - chunkSize/2f, 0f, y * chunkSize - chunkSize/2f);

		// Update chunk name
		gameObject.name = "Chunk ("+x+","+y+") Position:"+transform.position.ToString();

		priority = 0f;

		// Clear decoration list
		decorations.Clear();

		// Register all vertices with vertex map
		// Move vertices, generate normals/colors
		for (int i=0; i<numVerts; i++) {

			// Get VMap coords
			IntVector2 coord = IntToV2 (i);
			coords[i] = coord;

			// Get corresponding vertex
			mapVerts[i] = vmap.VertexAt(coord, true);

			// Get height from vertex
			UpdateVertex (i, mapVerts[i].height);
	
		}

		hasCheckedForRoad = false;

		UpdateCollider();
	}

	/// <summary>
	/// Creates a uniform vertex array.
	/// </summary>
	/// <returns>The uniform vertex array.</returns>
	/// <param name="vertexSize">Vertex size.</param>
	Vector3[] CreateUniformVertexArray (int vertexSize) { 
		float chunkSize = WorldManager.instance.chunkSize;
		int chunkRes = WorldManager.instance.chunkResolution;
		float scale = chunkSize / (chunkRes-1);
		int numVertices = vertexSize * vertexSize;
		Vector3[] uniformArray = new Vector3 [numVertices];
		for (int vert = 0; vert < numVertices; vert++) {
			uniformArray [vert] = new Vector3 (vert % vertexSize, 0, vert / vertexSize); //create vertex
			uniformArray [vert] = uniformArray[vert] * scale; //scale vector appropriately
		}
		return uniformArray;
	}

	/// <summary>
	/// Takes the number of vertices per side, returns a uniform array of UV coords for a square vertex array
	/// </summary>
	/// <param name="vertexSize"></param>
	/// <returns></returns>
	Vector2[] CreateUniformUVArray(int vertexSize) {
		int numVertices = vertexSize * vertexSize;
		Vector2[] uniformUVArray = new Vector2[numVertices];
		for (int vert = 0; vert < numVertices; vert++) {
			int x = vert % vertexSize; //get x position of vert
			int y = vert / vertexSize; //get y position of vert
			float u = x / (float)(vertexSize - 1); // normalize x into u between 0 and 1
			float v = ((vertexSize - 1) - y) / (float)(vertexSize - 1); //normalize y into v between 0 and 1 and flip direction
			uniformUVArray[vert] = new Vector2(u , v);
		}
		return uniformUVArray;
	}

	/// <summary>
	/// Takes the number of vertices per side, returns indices (each group of three defines a triangle) into a square vertex array to form mesh 
	/// </summary>
	/// <param name="vertexSize"></param>
	/// <returns></returns>
	int[] CreateSquareArrayTriangles (int vertexSize){ 
		int numTriangles = 2 * vertexSize * (vertexSize - 1);//a mesh with n^2 vertices has 2n(n-1) triangles
		int[] triangleArray = new int[numTriangles * 3]; //three points per triangle 
		int numVertices = vertexSize * vertexSize;
		int i = 0; //index into triangleArray (next two are the sibling vertices for its triangle, add 3 to jump to next triangle)
		for (int vert = 0; vert < numVertices - vertexSize; vert++) {
			/* Make these types of triangles
			 * 3---2
			 * *\**|
			 * **\*|
			 * ***\|
			 * ****1
			 */
			if (((vert + 1) % vertexSize) != 0) { //if vertex is not on the right edge
				triangleArray [i] = vert + vertexSize; //vertex 1
				triangleArray [i + 1] = vert + 1; //vertex 2
				triangleArray [i + 2] = vert; //vertex 3
				i = i + 3; //jump to next triangle
			}

			/* Make these types of triangles
			 * ****3 
			 * ***7|
			 * **7*|
			 * *7**|
			 * 1---2
			 */
			if ((vert % vertexSize) != 0) { //if vertex is not on the left edge
				triangleArray [i] = vert + vertexSize - 1; //vertex 1
				triangleArray [i + 1] = vert + vertexSize; //vertex 2
				triangleArray [i + 2] = vert; //vertex 3
				i = i + 3; //jump to next triangle
			}
		}
		return triangleArray;
	}

	/// <summary>
	/// Creates the chunk GameObject.
	/// </summary>
	/// <returns>The chunk.</returns>
	/// <param name="vertices">Vertices.</param>
	/// <param name="normals">Normals.</param>
	/// <param name="UVcoords">U vcoords.</param>
	/// <param name="triangles">Triangles.</param>
	Mesh CreateChunkMesh() {

		// Create mesh
		Mesh chunkMesh = new Mesh();

		// Optimize mesh for frequent updates
		chunkMesh.MarkDynamic ();

		// Assign vertices/normals/UVs/tris/colors
		chunkMesh.vertices = verts;
		chunkMesh.normals = normals;
		chunkMesh.uv = uvs;
		chunkMesh.triangles = triangles;
		chunkMesh.colors = colors;

		return chunkMesh;
	}

	/// <summary>
	/// Updates the physics collider.
	/// </summary>
	public void UpdateCollider () {
		needsColliderUpdate = false;

		ParticleSystem grass = grassEmitter.GetComponent<ParticleSystem>();

		// Clear current grass
		grass.Clear();

		// Reassign mesh vertices/normals
		mesh.vertices = verts;
		mesh.normals = normals; // NEEDED FOR PROPER LIGHTING

		// Recalculate bounding box
		mesh.RecalculateBounds();

		// Reassign collider mesh
		GetComponent<MeshCollider> ().sharedMesh = mesh;

		// Assign particle system emission shape
		ParticleSystem.ShapeModule shape = grass.shape;
		shape.mesh = mesh;

		// Replace decorations
		ReplaceDecorations();

		// Replace grass
		grassEmitter.GetComponent<ParticleSystem>().Play();

	}

	/// <summary>
	/// Updates the verrex colors.
	/// </summary>
	public void UpdateColors () {
		mesh.colors = colors;
		needsColorUpdate = false;
	}

	/// <summary>
	/// Updates a vertex.
	/// </summary>
	/// <param name="index">Index.</param>
	/// <param name="height">Height.</param>
	/// <param name="normal">Normal.</param>
	public void UpdateVertex (int index, float height, bool forceUpdate=false) {
		try {
			
			// Check if height update is needed
			if (verts[index].y != height) {
				priority++;
				verts[index].y = height;
				if (forceUpdate) mesh.vertices = verts;
				needsColliderUpdate = true;
			}

		} catch (IndexOutOfRangeException e) {
			Debug.LogError ("Chunk.UpdateVertex(): invalid index "+index+"! " + e.Message);
			return;
		}
	}

	/// <summary>
	/// Updates the color.
	/// </summary>
	/// <param name="index">Index.</param>
	/// <param name="blendValue">Blend value.</param>
	public void UpdateColor (int index, Color color) {

		// Check if color update is needed
		if (colors[index] != color) {
			colors[index] = color;
			needsColorUpdate = true;
		}
	}

	/// <summary>
	/// Checks if a vertex is within range to be updated.
	/// </summary>
	/// <returns><c>true</c>, if dist was checked, <c>false</c> otherwise.</returns>
	/// <param name="dist">Dist.</param>
	/// <param name="updateDist">Update dist.</param>
	/// <param name="margin">Margin.</param>
	private bool CheckDist (float dist, float updateDist, float margin) {
		return ((dist < (updateDist + margin)) && (dist > (updateDist - margin)));
	}

	/// <summary>
	/// Coroutine to update the vertices of a chunk.
	/// </summary>
	/// <returns>The verts.</returns>
	private IEnumerator UpdateVerts() {

		if (!GameManager.instance.loaded) yield break;
		
		isUpdatingVerts = true;
		float margin = WorldManager.instance.chunkSize / 2;
		float startTime = Time.realtimeSinceStartup;
		Vector3 playerPos = PlayerMovement.instance.transform.position;
		Vector3 chunkPos = transform.position;

		int v = 0;
		for (; v < numVerts; v++) {
	
			// Get VMap coordinates
			IntVector2 coord = coords[v];

			// Get coresponding vertex
			Vertex vert = mapVerts[v];

			// Update vertex height
			UpdateVertex (v, vert.height);

			if (terrain.freqData == null) yield break;

			// If vertex is not locked and there is frequency data to use
			if (!vert.locked) { 

				// Distance between player and vertex
				Vector3 vertPos = chunkPos + verts [v];
				float distance = Vector3.Distance (vertPos, playerPos);

				// If vertex is close enough
				if (CheckDist (distance, WorldManager.instance.vertexUpdateDistance, margin)) {

					// Calculate new height
					Vector3 angleVector = vertPos - playerPos;
					float angle = Vector3.Angle (Vector3.right, angleVector);
					float linIntInput = angle / 360f;
					float newY = terrain.freqData.GetDataPoint (linIntInput) *
					              WorldManager.instance.heightScale;

					// If new height, set it
					//if (newY != vmap.VertexAt(coord, false).height) vmap.SetHeight (coord, newY);
					if (newY != 0f) vmap.AddHeight (coord, newY);
				}
			}

			if (v == numVerts-1) {
				isUpdatingVerts = false;
				yield break;
			} else if (Time.realtimeSinceStartup - startTime > GameManager.instance.targetDeltaTime) {
				yield return null;
				startTime = Time.realtimeSinceStartup;
			}
		}
	}

	/// <summary>
	/// Stops the vertex update coroutine.
	/// </summary>
	public void StopUpdatingVerts () {
		StopCoroutine("UpdateVerts");
	}

	/// <summary>
	/// Updates all necessary properties of a chunk.
	/// </summary>
	public void ChunkUpdate () {
		
		// Update collider if necessary
		if (needsColliderUpdate) UpdateCollider();

		// Update vertex colors if necessary
		if (needsColorUpdate) UpdateColors();

		// Check for road if necessary
		if (!hasCheckedForRoad && WorldManager.instance.road.loaded)
			CheckForRoad(PlayerMovement.instance.moving ? PlayerMovement.instance.progress : 0f);

		// Update verts if possible
		if (!isUpdatingVerts) StartCoroutine("UpdateVerts");

		priority = 0f;
	}

	/// <summary>
	/// Checks if a chunk is nearby to or has a road.
	/// </summary>
	/// <param name="startProgress">Start progress.</param>
	public void CheckForRoad (float startProgress) {
		hasCheckedForRoad = true;
		Road road = WorldManager.instance.road;
		Vector3 chunkPos = transform.position;
		float checkResolution = (1f - startProgress) * WorldManager.instance.roadPathCheckResolution;

		// Set boundaries for "near road" consideration
		Vector2 nearMin = new Vector2 (chunkPos.x - chunkSize, chunkPos.z - chunkSize);
		Vector2 nearMax = new Vector2 (chunkPos.x + chunkSize * 2f, chunkPos.z + chunkSize * 2f);

		// Set boundaries for "has road" consideration
		Vector2 hasMin = new Vector2 (chunkPos.x, chunkPos.z);
		Vector2 hasMax = new Vector2 (chunkPos.x + chunkSize, chunkPos.z + chunkSize);

		float progress = startProgress;
		while (progress <= 1f) {

			// Sample road and check distance to chunk
			Vector3 sample = road.GetPoint(progress);
			if (sample.x >= nearMin.x && sample.x <= nearMax.x &&
				sample.z >= nearMin.y && sample.z <= nearMax.y) {

				if (!nearRoad) {
					gameObject.name += "|nearRoad";
					terrain.AddCloseToRoadChunk(this);
				}
				nearRoad = true;

				// If near road, check if has road
				if (sample.x >= hasMin.x && sample.x <= hasMax.x &&
					sample.z >= hasMin.y && sample.z <= hasMax.y) {

					terrain.AddRoadChunk(this);
					gameObject.name += "|hasRoad";
					grassEmitter.SetActive(false);
					hasRoad = true;
					return;
				}
			}
			progress += 1f / checkResolution;
		}
	}

	/// <summary>
	/// Converts a world position to the nearest vertex map coords.
	/// </summary>
	/// <returns>The nearest V map coords.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public static IntVector2 ToNearestVMapCoords (float x, float y) {
		//if (x < 0) Debug.Log(x);
		IntVector2 result = new IntVector2 (
			Mathf.FloorToInt((x+chunkSize/2f)* (chunkRes-1) / chunkSize),
			Mathf.FloorToInt((y+chunkSize/2f)* (chunkRes-1) / chunkSize)
		);
		//if (x < 0) Debug.Log(result.x);
		//Debug.Log (x+","+y +" mapped to "+result.ToString());
		return result;
	}

	/// <summary>
	/// Converts a vertex index to a vertex map coordinate.
	/// </summary>
	/// <returns>The to v2.</returns>
	/// <param name="i">The index.</param>
	IntVector2 IntToV2 (int i) {
		int chunkRes = WorldManager.instance.chunkResolution;

		int xi = x * (chunkRes-1) + i % chunkRes;
		int yi = y * (chunkRes-1) + i / chunkRes;
		return new IntVector2 (xi, yi);
	}

	/// <summary>
	/// Resets height of all decorations on a chunk
	/// </summary>
	public void ReplaceDecorations () {
		foreach (Transform tr in GetComponentsInChildren<Transform>()) {

			// Skip chunk itself
			if (tr == transform || tr == grassEmitter.transform) continue;

			// Raycast down
			RaycastHit hit;
			Vector3 rayOrigin = new Vector3 (tr.position.x, WorldManager.instance.heightScale, tr.position.z);
			if (Physics.Raycast(rayOrigin, Vector3.down,out hit, Mathf.Infinity))
				tr.position = new Vector3 (tr.position.x, hit.point.y + 
					tr.gameObject.GetComponent<Decoration>().positionOffset.y, tr.position.z);
		}
	}

	/// <summary>
	/// Removes and pools all decorations on the chunk.
	/// </summary>
	public void RemoveDecorations () {
		foreach (GameObject decoration in decorations)
			WorldManager.instance.RemoveDecoration(decoration);
	}

	/// <summary>
	/// Sets terrain debug colors.
	/// </summary>
	/// <param name="color"></param>
	public void SetDebugColors (DynamicTerrain.DebugColors color) {
		switch (color) {
		case DynamicTerrain.DebugColors.Constrained:
			for (int v=0; v<numVerts; v++) {
				colors[v] = mapVerts[v].noDecorations ? Color.black : Color.white;
			}
			mesh.colors = colors;
			break;
		}
	}

	#endregion

}