using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Class to manage chunks. 
/// </summary>
public class DynamicTerrain : MonoBehaviour {

	#region DynamicTerrain Enums

	/// <summary>
	/// Types of debug colors for terrain to show.
	/// </summary>
	public enum DebugColors {
		Constrained // Shows where decoration placement is constrained
	}

	#endregion
	#region DynamicTerrain Vars

	bool loaded = false;                 // is the terrain loaded?
	bool randomized = false;             // Has the terrain been randomized?
	public int loadsToDo;

	ObjectPool<Chunk> chunkPool;         // pool of chunk GameObjects
	List<Chunk> chunksToUpdate;          // list of chunks to be updated
	int chunkUpdatesPerCycle;            // number of chunks to update each cycle (copied from WM)

	List<Chunk> activeChunks;            // list of active chunks
	List<Chunk> activeCloseToRoadChunks; // list of active chunks that are within one chunk width of the road
	List<Chunk> activeRoadChunks;        // list of active chunks with road on them
	List<Chunk> deletions;               // list of chunks that will be deleted

	public VertexMap vertexmap;          // map of vertices
	Map<Chunk> chunkmap;                 // map of chunks

	float chunkSize;                     // size of chunks in world units (copied from WM)
	int chunkRes;                        // resolution of chunks (copied from WM)
	int chunkLoadRadius;                 // radius around player to load chunks, in number of chunks (copied from WM)

	Vector3 playerPos;                   // world position of player avatar (copied from PlayerMovement)
	IntVector2 playerChunkPos;           // 2D position of player avatar in terms of chunk

	#endregion
	#region Frequency Sampling Vars

	public LinInt freqData;              // pointer to stored music frequency data
	int freqSampleSize;                  // sample size to use when reading frequency data (copied from WM)
	float[] data;                        // raw frequency data
	FFTWindow fftWindow;                 // FFT window to use when reading frequency data (copied from WM)
	AudioSource[] sources;
	int numSources;

	#endregion
	#region Unity Callbacks

	void Awake () {

		// Copy vars from WM
		chunkUpdatesPerCycle = WorldManager.instance.chunkUpdatesPerCycle;
		chunkSize = WorldManager.instance.chunkSize;
		chunkRes = WorldManager.instance.chunkResolution;
		chunkLoadRadius = WorldManager.instance.chunkLoadRadius;

		int verts = 2 * chunkLoadRadius * (chunkRes - 1);
		loadsToDo = verts * verts;

		// Init chunk pool
		chunkPool = new ObjectPool<Chunk>();

		// Init chunk lists
		chunksToUpdate = new List<Chunk>();
		activeChunks = new List<Chunk>();
		activeRoadChunks = new List<Chunk>();
		activeCloseToRoadChunks = new List<Chunk>();
		deletions = new List<Chunk>();

		// Init vertex map
		vertexmap = new VertexMap();
		vertexmap.terrain = this;

		// Init chunk map
		chunkmap = new Map<Chunk>(chunkLoadRadius*2);

		// Init player chunk position
		playerChunkPos = new IntVector2(0, 0);

		// Init frequency data vars
		freqSampleSize = WorldManager.instance.freqArraySize;
		data = new float[freqSampleSize];
		fftWindow = WorldManager.instance.freqFFTWindow;
	}

	#endregion
	#region DynamicTerrain Load Callbacks

	/// <summary>
	/// Starts the chunk update coroutine.
	/// </summary>
	public void DoLoadChunks () {
		WorldManager.instance.StartCoroutine(UpdateChunks());
	}


	#endregion
	#region DynamicTerrain Callbacks

	/// <summary>
	/// Adds a new chunk at the specified coordinates,
	/// drawing from the pool if possible.
	/// </summary>
	/// <returns>The chunk.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	GameObject CreateChunk (int x, int y) {

		GameObject chunk;
		Chunk c;

		// If no chunks available to reuse
		if (chunkPool.Empty) {

			// Create new chunk
			chunk = new GameObject ("", 
				typeof(MeshFilter), 
				typeof(MeshRenderer),
				typeof(MeshCollider),
				typeof(Rigidbody),
				typeof(Chunk)
			);

			c = chunk.GetComponent<Chunk>();

			// Initialize chunk
			c.Initialize(x, y);

		// If a chunk is available to reuse
		} else {

			// Take a chunk from the pool
			chunk = chunkPool.Get().gameObject;

			c = chunk.GetComponent<Chunk>();

			// Reuse chunk
			c.Reuse(x, y);
		}

		// Parent chunk to terrain
		chunk.transform.parent = transform;

		// Register chunk as active
		activeChunks.Add(c);

		return chunk;
	}
		
	/// <summary>
	/// Updates the chunks.
	/// </summary>
	/// <returns>The chunks.</returns>
	IEnumerator UpdateChunks () {
		
		// Init loading vars
		float startTime = Time.realtimeSinceStartup;

		List<string> loadMessages = new List<string>() {
			"Building your new playground...",
			"Desertifying the desert..."
		};

		// Change loading screen message
		if (!loaded) GameManager.instance.ChangeLoadingMessage(loadMessages.Random());

		// Main loop
		while (true) {

			// If updating terrain
			if (loaded) {
				DeleteChunks();
				UpdateFreqData ();
			}

			// Update player world and chunk positions
			playerPos = PlayerMovement.instance.transform.position;
			playerChunkPos = new IntVector2 (
				(int)Mathf.RoundToInt((playerPos.x - chunkSize/2f) / chunkSize),
				(int)Mathf.RoundToInt((playerPos.z -chunkSize/2f) / chunkSize)
			);

			// For each space where there should be a chunk
			for (int x=playerChunkPos.x - chunkLoadRadius; x<=playerChunkPos.x + chunkLoadRadius; x++) {
				for (int y=playerChunkPos.y - chunkLoadRadius; y<=playerChunkPos.y + chunkLoadRadius; y++) {
					
					// Skip if chunk exists
					if (chunkmap.At(x,y) != null) continue;

					// Skip if chunk too far (circular generation)
					if (WorldManager.instance.chunkGenerationMode == WorldManager.ChunkGenerationMode.Circular)
						if (IntVector2.Distance (new IntVector2(x,y), playerChunkPos) > (float)chunkLoadRadius)
							continue;

					// Create chunk
					chunkmap.Set(x,y, CreateChunk (x, y).GetComponent<Chunk>());

					if (!loaded) {
						GameManager.instance.ReportLoaded(1);
						startTime = Time.realtimeSinceStartup;
					}
					yield return null;
						
				}
			}

			// Take a break if target frame rate is missed
			if (Time.realtimeSinceStartup - startTime > GameManager.instance.targetDeltaTime) {
				yield return null;
				startTime = Time.realtimeSinceStartup;
			}

			// If finished loading terrain
			if (!loaded) {
				loaded = true;

				// Update all colliders
				foreach (Chunk chunk in activeChunks) chunk.UpdateCollider();

				// Deform initial terrain
				int res = vertexmap.vertices.Width;
				StartCoroutine(CreateMountain (0, 0, res, res, 5f, 20f, -0.03f, 0.03f));

			// If updating terrain
			} else if (randomized)  {

				// Reset list of chunks to update
				chunksToUpdate.Clear ();
				int listCount = 0;

				// For each active chunk
				foreach (Chunk chunk in activeChunks) {

					// Increase priority
					chunk.priority++;

					// Insert chunk into list based on priority
					if (listCount == 0) {
						chunksToUpdate.Add (chunk);
						listCount++;
					}
					else for (int i=0; i<listCount; i++) 
						if (chunk.priority > chunksToUpdate[i].priority) {
							chunksToUpdate.Insert (i, chunk);
							listCount++;
							break;
						}
				}
					
				// Update highest priority chunks
				for (int i=0; i < chunkUpdatesPerCycle && i < activeChunks.Count && i < listCount; i++) {
					chunksToUpdate [i].ChunkUpdate ();
					chunksToUpdate[i].priority = 0;
				}
			

				// Take a break if target frame rate missed
				if (Time.realtimeSinceStartup - startTime > GameManager.instance.targetDeltaTime) {
					yield return null;
					startTime = Time.realtimeSinceStartup;
				}

			}
			yield return null;
		}
	}

	/// <summary>
	/// Determines which chunks to delete and deletes them.
	/// </summary>
	void DeleteChunks () {

		// Init list of chunks to delete
		deletions.Clear();

		// Check if each active chunk is within chunk load radius
		foreach (Chunk chunk in activeChunks) {
			switch (WorldManager.instance.chunkGenerationMode) {
				case WorldManager.ChunkGenerationMode.Circular:
					if (IntVector2.Distance (new IntVector2 (chunk.x, chunk.y), playerChunkPos) > chunkLoadRadius)
						deletions.Add(chunk);
					break;

				case WorldManager.ChunkGenerationMode.Square:
					if (chunk.x < playerChunkPos.x - chunkLoadRadius || chunk.x > playerChunkPos.x + chunkLoadRadius ||
						chunk.y < playerChunkPos.y - chunkLoadRadius || chunk.y > playerChunkPos.y + chunkLoadRadius)
						deletions.Add(chunk);
					break;
			}
		}

		// Delete all marked chunks
		foreach (Chunk chunk in deletions) DeleteChunk (chunk);
	}

	/// <summary>
	/// Deletes (pools) a chunk.
	/// </summary>
	/// <param name="chunk">Chunk.</param>
	void DeleteChunk(Chunk chunk){
		StopCoroutine ("RoadCheck");

		// Stop chunk updating verts.
		chunk.StopUpdatingVerts();

		// Remove all decorations on chunk
		chunk.RemoveDecorations();

		// Deregister from lists/map
		DeregisterChunk(chunk);

		chunksToUpdate.Remove(chunk);

		// Pool chunk
		chunkPool.Add(chunk);

		
	}

	/// <summary>
	/// Chunks at x and y.
	/// </summary>
	/// <returns>The <see cref="Chunk"/>.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public Chunk ChunkAt (int x, int y) {
		return chunkmap.At (x, y);
	}

	/// <summary>
	/// Removes a chunk from the appropriate lists.
	/// </summary>
	/// <param name="chunk">Chunk.</param>
	public void DeregisterChunk (Chunk chunk) {

		// Remove chunk from active list
		activeChunks.Remove(chunk);

		// Remove chunk from road and close to road lists
		if (chunk.nearRoad) {
			activeCloseToRoadChunks.Remove(chunk);
			if (chunk.hasRoad) activeRoadChunks.Remove(chunk);
		}

		// Deparent chunk
		chunk.transform.parent = null;

		// Remove entry from chunkmap
		chunkmap.Set(chunk.x, chunk.y, null);
	}

	/// <summary>
	/// Called when the road is extended.
	/// </summary>
	public void OnExtendRoad () {

		// Mark any chunk that is not near a road to be checked for road
		foreach (Chunk chunk in activeChunks) {
			if (!chunk.nearRoad) chunk.hasCheckedForRoad = false;
		}
	}
		
	/// <summary>
	/// Reads the frequency data put out from instruments.
	/// </summary>
	void UpdateFreqData () {
		if (sources == null) {
			sources = MusicManager.instance.instrumentAudioSources.Values.ToArray<AudioSource>();
			numSources = sources.Length;
		}

		// For each instrument audio source
		for (int s = 0; s < numSources; s++) {

			// Skip if disabled
			if (!sources[s].enabled) continue;

			// Sample audio source
			float[] sample = new float[freqSampleSize];
			sources[s].GetSpectrumData (sample, 0, fftWindow);

			// Add audio source data into final array
			for (int i = 0; i < freqSampleSize; i++) {
				if (sample [i] != float.NaN && sample [i] != 0f) {
					if (s == 0)  data [i] = sample [i];
					else data [i] += sample [i];
				}
			}
		}

		// Convert audio data into LinInt
		if (freqData == null && GameManager.instance.loaded) freqData = new LinInt();
		if (GameManager.instance.loaded) freqData.Update(data);
	}
		
	/// <summary>
	/// Returns a random active chunk.
	/// </summary>
	/// <returns>A random active chunk.</returns>
	public Chunk RandomChunk () {

		// Check if no active chunks
		if (activeChunks.Count == 0)
			return null;

		// Pick a random active chunk
		Chunk chunk = activeChunks[UnityEngine.Random.Range(0, activeChunks.Count)];

		// Check if chunk is null
		if (chunk == null) {
			Debug.LogError("DynamicTerrain.RandomChunk(): tried to return null chunk!");
			return null;
		}

		return chunk;
	}

	public int NumActiveChunks {
		get {
			return activeChunks.Count;
		}
	}

	/// <summary>
	/// Registers a chunk as having a road on it.
	/// </summary>
	/// <param name="chunk">Chunk.</param>
	public void AddRoadChunk (Chunk chunk) {
		activeRoadChunks.Add(chunk);
	}

	/// <summary>
	/// Returns a random chunk that has a road on it.
	/// </summary>
	/// <returns>The road chunk.</returns>
	public Chunk RandomRoadChunk() {

		// Check if no active chunks with road
		if (activeRoadChunks.Count == 0)
			return null;

		// Pick a random road chunk
		Chunk chunk = activeRoadChunks[UnityEngine.Random.Range(0, activeRoadChunks.Count)];

		// Check if chunk is null
		if (chunk == null) {
			Debug.LogError("DynamicTerrain.RandomRoadChunk(): tried to return null chunk!");
			return null;
		}
			
		return chunk;
	}

	/// <summary>
	/// Registers a chunk as being close to a road.
	/// </summary>
	/// <param name="chunk">Chunk.</param>
	public void AddCloseToRoadChunk (Chunk chunk) {
		activeCloseToRoadChunks.Add (chunk);
	}

	/// <summary>
	/// Returns a random chunk within one chunk width of a road.
	/// </summary>
	/// <returns>The close to road chunk.</returns>
	public Chunk RandomCloseToRoadChunk() {

		// Check if no active chunks near a road
		if (activeCloseToRoadChunks.Count == 0)
			return null;

		// Pick a random chunk near the road.
		Chunk chunk = activeCloseToRoadChunks[UnityEngine.Random.Range(0, activeCloseToRoadChunks.Count)];

		// Check if chunk is null
		if (chunk == null) {
			Debug.LogError("DynamicTerrain.RandomCloseToRoadChunk(): tried to return null chunk!");
			return null;
		}

		return chunk;
	}

	/// <summary>
	/// Creates a randomized mountain.
	/// </summary>
	/// <param name="x">x coordinate of the center vertex.</param>
	/// <param name="y">y coordinate of the center vertex.</param>
	/// <param name="width">Width.</param>
	/// <param name="depth">Depth.</param>
	/// <param name="height">Height.</param>
	/// <param name="rough">Rough.</param>
	/// <param name="rangeMin">Range minimum.</param>
	/// <param name="rangeMax">Range max.</param>
	public IEnumerator CreateMountain (int x, int y, int width, int depth, float height, float rough, 
		float rangeMin = -0.1f, float rangeMax = 1f) {

		float startTime = Time.realtimeSinceStartup;
		bool randomizing = false;
		float[,] heightmap = null;
		

		List<string> loadMessages = new List<string>() {
			"Literally moving mountains..."
		};

		GameManager.instance.ChangeLoadingMessage(loadMessages.Random());

		while (true) {

			if (!randomized && !randomizing) {

				//ensure width and depth are odd
				if (width % 2 == 0)
					width++;
				if (depth % 2 == 0)
					depth++;
				int size = Math.Max (width, depth);
				size--;
				size = MakePowerTwo (size); //size of the Diamond Square Alg array
				//Debug.Log("Size is: " + size);
				if (size < 2) yield break;
				heightmap = new float[size + 1, size + 1];
				float[] corners = InitializeCorners (vertexmap, x, y, width, depth);
				FillDiamondSquare (heightmap, corners, height, rough, rangeMin, rangeMax);
				randomizing = true;

			} else if (randomized) {

				//set vertices
				int minX = x - width/2;
				int maxX = x + width / 2;
				int minY = y - depth / 2;
				int maxY = y + depth / 2;
				int mapMax = (int)heightmap.GetLongLength (0) - 1;
				for (int i = minX; i <= maxX; i++) {
					float normalizedX = (float)(i - minX) / (float)(maxX - minX);
					int xFloor = Mathf.FloorToInt(normalizedX * (float)mapMax);
					int xCeil = Mathf.FloorToInt (normalizedX * (float)mapMax);
					float xT = normalizedX % 1f;
					for (int j = minY; j <= maxY; j++) {
						if (vertexmap.ContainsVertex(i, j)) {
							float normalizedY = (float)(j - minY) / (float)(maxY - minY);
							int yFloor = Mathf.FloorToInt (normalizedY * (float)mapMax);
							int yCeil = Mathf.FloorToInt (normalizedY * (float)mapMax);
							float yT = normalizedY % 1f;
							float p00 = GetFromHMap(heightmap, xFloor, yFloor);
							float p10 = GetFromHMap(heightmap, xCeil, yFloor);
							float p01 = GetFromHMap(heightmap, xFloor, yCeil);
							float p11 = GetFromHMap(heightmap, xCeil, yCeil);
							float interpH = ((1 - xT)*(1-yT))*p00 + ((xT)*(1-yT))*p10 + ((1-xT)*(yT))*p01 + ((xT)*(yT))*p11;
							if (!vertexmap.IsConstrained (i, j) && !vertexmap.IsLocked (i,j)) {
								vertexmap.SetHeight (i, j, interpH);
								GameManager.instance.ReportLoaded(1);
							}
						} else continue;

						if (Time.realtimeSinceStartup - startTime > GameManager.instance.targetDeltaTime) {
							yield return null;
							startTime = Time.realtimeSinceStartup;
						}

					}
				}

				// Begin loading road
				WorldManager.instance.DoLoadRoad();
				yield break;
			}

			yield return null;
		}
	}

	/// <summary>
	/// Raises n to the next power of two.
	/// </summary>
	/// <returns>The power two.</returns>
	/// <param name="n">N.</param>
	public int MakePowerTwo (int n) {
		if (n < 2) return -1; // if n is less than 2, return error value -1
		if ((n != 0) && ((n & (n-1)) == 0)) return n; //if n is already a power of 2, return n
		int r = 0; //counter of highest power of 2 in n
		//bit shift n to get place of leading bit, r, which is the log base 2 of n
		while ((n >>= 1) != 0) {
			r++;
			//Debug.Log ("r is: " + r);
		}
		r++; //raise power of two to next highest
		//Debug.Log("Final r is: " + r);
		return (int)Math.Pow (2, r);
	}

	/// <summary>
	/// Returns the initial corners for the Diamond Square Algorithm.
	/// </summary>
	/// <param name="vmap"></param>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="width"></param>
	/// <param name="depth"></param>
	/// <returns></returns>
	public float[] InitializeCorners(VertexMap vmap, int x, int y, int width, int depth) {
		float[] corners = new float[4];
		//corner lower left
		int vx = x - (width - 1) / 2; //vertex x
		int vy = y - (width - 1) / 2; //vertex y
		if (!float.IsNaN(corners[1] = vmap.GetHeight(vx, vy))) corners[0] = 0f;
		//corner lower right
		vx = x + (width - 1) / 2; //vertex x
		vy = y - (width - 1) / 2; //vertex y
		if (!float.IsNaN(corners[1] = vmap.GetHeight(vx, vy))) corners[1] = 0f;
		//corner upper right
		vx = x + (width - 1) / 2; //vertex x
		vy = y + (width - 1) / 2; //vertex y
		if (!float.IsNaN(corners[1] = vmap.GetHeight(vx, vy))) corners[2] = 0f;
		//corner upper left
		vx = x - (width - 1) / 2; //vertex x
		vy = y + (width - 1) / 2; //vertex y
		if (!float.IsNaN(corners[1] = vmap.GetHeight(vx, vy))) corners[3] = 0f;
		return corners;
	}

	/// <summary>
	/// Fills heightmap with DiamondSquare generated heights, 
	/// using corners as the seeds, height as the initial center 
	/// value, and rough as the height offset value
	/// </summary>
	/// <param name="heightmap"></param>
	/// <param name="corners"></param>
	/// <param name="height"></param>
	/// <param name="rough"></param>
	/// <param name="rangeMin"></param>
	/// <param name="rangeMax"></param>
	void FillDiamondSquare (float[,] heightmap, float[] corners, float height, float rough, float rangeMin, float rangeMax){
		//set middle of hmap
		int max = (int)heightmap.GetLongLength(0) - 1;
		heightmap [max/2 + 1, max/2 + 1] = height; //set middle height
		StartCoroutine(Divide(heightmap, max, rough, rangeMin, rangeMax));
	}

	/// <summary>
	/// Performs the square phase of the diamond-square algorithm.
	/// </summary>
	/// <param name="heightmap"></param>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="size"></param>
	/// <param name="offset"></param>
	void Square(float[,] heightmap, int x, int y, int size, float offset) {
		//Debug.Log("square");
		float ave = Average (new float[] {
			GetFromHMap(heightmap, x - size, y - size), //lower left
		 	GetFromHMap(heightmap, x + size, y - size), //lower right
			GetFromHMap(heightmap, x + size, y + size), //upper right
			GetFromHMap(heightmap, x - size, y + size) //upper left
		});
		heightmap [x, y] = ave + offset;
	}

	/// <summary>
	/// Performs the diamond phase of the diamond-square algorithm.
	/// </summary>
	/// <param name="heightmap"></param>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="size"></param>
	/// <param name="offset"></param>
	void Diamond(float[,] heightmap, int x, int y, int size, float offset) {
		//Debug.Log("diamond "+x+" "+y+" "+size+" "+offset);
		float ave = Average (new float[] {
			GetFromHMap(heightmap, x, y - size), //bottom
			GetFromHMap(heightmap, x + size, y), //right
			GetFromHMap(heightmap, x, y + size), //top
			GetFromHMap(heightmap, x - size, y) //left
		});
		heightmap [x, y] = ave + offset;
	}

	/// <summary>
	/// Calls diamond and square.
	/// </summary>
	/// <param name="heightmap"></param>
	/// <param name="size"></param>
	/// <param name="rough"></param>
	/// <param name="rangeMin"></param>
	/// <param name="rangeMax"></param>
	IEnumerator Divide(float[,] heightmap, int size, float rough, float rangeMin, float rangeMax) {

		//Debug.Log("divide");

		float startTime = Time.realtimeSinceStartup;

		while (true) {

			int x, y, half = size / 2;
			float scale = size * rough;
			if (half < 1) {//past the minimum size
				//Debug.Log("break");
				randomized = true;
				yield break;
			}

			//Debug.Log(heightmap.GetLongLength(0)-1);

			//do squares
			for (y = half; y < heightmap.GetLongLength(1) - 1; y += size) {
				for (x = half; x < heightmap.GetLongLength(0) - 1; x += size) {
					if (size == heightmap.GetLongLength(0) - 1) { //ignore setting the very center of the mountain
						//Debug.Log("skipping "+x+","+ y);
						continue;
					}else {
						Square (heightmap, x, y, half, UnityEngine.Random.Range (rangeMin, rangeMax) * scale);
					}

					if (Time.realtimeSinceStartup - startTime > GameManager.instance.targetDeltaTime) {
						yield return null;
						startTime = Time.realtimeSinceStartup;
					}
				}
			}

			//do diamonds
			for (y = 0; y <= heightmap.GetLongLength (1) - 1; y += half) {
				for (x = (y + half) % size; x <= heightmap.GetLength (0) - 1; x += size) {
					Diamond (heightmap, x, y, half, UnityEngine.Random.Range (rangeMin, rangeMax) * scale);
				
					if (Time.realtimeSinceStartup - startTime > GameManager.instance.targetDeltaTime) {
						yield return null;
						startTime = Time.realtimeSinceStartup;
					}
				}
			}

			size = half;

			yield return null;

			//Debug.Log("reached");
			//recursive call
			//Divide (heightmap, half, rough, rangeMin, rangeMax);
		}
	}

	/// <summary>
	/// Accesses heightmap and returns -INF for out of bounds vertices
	/// </summary>
	/// <param name="heightmap"></param>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	float GetFromHMap (float [,] heightmap, int x, int y) {
		if (x < 0 || x >= heightmap.GetLength (0) || y < 0 || y >= heightmap.GetLength (1)) {
			return float.NegativeInfinity;
		}
		return heightmap [x, y];
	}

	/// <summary>
	/// Returns the average of 4 corners, excluding non-legal values
	/// </summary>
	/// <param name="corners"></param>
	/// <returns></returns>
	float Average(float[] corners) {
		float count = 4f;
		float sum = 0f;
		foreach (float corner in corners) {
			if (float.IsNegativeInfinity (corner)) {
				count -= 1f;
				continue;
			}
			sum += corner;
		}
		return (sum/count);
	}

	/// <summary>
	/// Sets all chunks to use debug colors.
	/// </summary>
	/// <param name="colors"></param>
	public void SetDebugColors (DebugColors colors) {
		switch (colors) {
		case DebugColors.Constrained:
			foreach (Chunk chunk in activeChunks) {
				chunk.GetComponent<MeshRenderer>().material = WorldManager.instance.terrainDebugMaterial;
				chunk.SetDebugColors (colors);
			}
			break;
		}
	}
}

#endregion
