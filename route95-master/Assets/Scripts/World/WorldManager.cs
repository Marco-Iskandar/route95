using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;

/// <summary>
/// Manager for all things world-related.
/// </summary>
public class WorldManager : MonoBehaviour {

	#region WorldManager Enums

	/// <summary>
	/// Modes to generate chunks around player.
	/// </summary>
	public enum ChunkGenerationMode {
		Square,
		Circular
	}

	#endregion
	#region WorldManager Vars

	public static WorldManager instance;

	public GameObject vertexIndicator;

	public int loadsToDo;

	// Chunk vars
	const float DEFAULT_CHUNK_SIZE = 100;                // Default chunk size (world units)
	const int DEFAULT_CHUNK_RESOLUTION = 8;              // Default number of vertices per chunk edge
	const int DEFAULT_CHUNK_LOAD_RADIUS = 4;             // Default player radius to load chunks (chunks)

	// Terrain vars
	const float DEFAULT_HEIGHT_SCALE = 800f;             // Default terrain height scale (world units)
	const float DEFAULT_VERTEX_UPDATE_DISTANCE = 600f;   // Default player radius to update vertices (world units)

	// Decoration vars
	const int DEFAULT_MAX_DECORATIONS = 1000;            // Default hard decoration limit
	const int DEFAULT_DECORATIONS_PER_STEP = 100;        // Default max number of decorations to place each cycle

	// Road vars
	const float DEFAULT_ROAD_WIDTH = 10f;                // Default road width (world units)
	const float DEFAULT_ROAD_HEIGHT = 0.2f;              // Default road height (world units)
	const float DEFAULT_ROAD_SLOPE = 0.9f;               // Default ratio of road top plane to bottom plane (percent)
	const float DEFAULT_ROAD_EXTEND_RADIUS = 1000f;      // Default player radius to extend road (world units)
	const int DEFAULT_ROAD_STEPS_PER_CURVE = 100;        // Default number of road mesh subdivision steps per segment
	const float DEFAULT_ROAD_MAX_SLOPE = 0.0015f;        // Default limit on road slope per world unit
	const float DEFAULT_ROAD_PLACEMENT_DISTANCE = 30f;   // Default distance to place road (world units)
	const float DEFAULT_ROAD_VARIANCE = 0.4f;            // Default radius of road placement circle (percent)

	// Day/night cycle vars
	const float DEFAULT_TIME_SCALE = 0.01f;              // Default time scale multiplier

	// Performance vars
	const int DEFAULT_CHUNK_UPDATES_PER_CYCLE = 4;       // Default number of chunks to update per cycle
	const int DEFAULT_FREQ_ARRAY_SIZE = 256;             // Default size of frequency data array (power of 2)
	const float DEFAULT_ROAD_PATH_CHECK_RESOLUTION = 4f; // Default resolution at which to check the road

	#endregion
	#region WorldManager Vars

	//-----------------------------------------------------------------------------------------------------------------
	[Header("Chunk Settings")]

	[Tooltip("The length of one side of a chunk.")]
	[Range(1f, 200f)]
	public float chunkSize = DEFAULT_CHUNK_SIZE;

	[Tooltip("The number of vertices along one side of a chunk.")]
	[Range(2, 32)]
	public int chunkResolution = DEFAULT_CHUNK_RESOLUTION;

	[Tooltip("The radius around the player within which to draw chunks.")]
	[Range(4, 32)]
	public int chunkLoadRadius = DEFAULT_CHUNK_LOAD_RADIUS;

	[Tooltip("Mode to generate chunks.")]
	public ChunkGenerationMode chunkGenerationMode = ChunkGenerationMode.Circular;

	//-----------------------------------------------------------------------------------------------------------------
	[Header("Terrain Settings")]

	[Tooltip("Height scale of generated terrain.")]
	[Range(100f, 1000f)]
	public float heightScale = DEFAULT_HEIGHT_SCALE;

	[Tooltip("The distance from the player at which vertices should update.")]
	[Range(100f, 1000f)]
	public float vertexUpdateDistance = DEFAULT_VERTEX_UPDATE_DISTANCE;

	[Tooltip("Material to use for terrain.")]
	public Material terrainMaterial;

	[Tooltip("Material to use when debugging terrain.")]
	public Material terrainDebugMaterial;

	[NonSerialized]
	public DynamicTerrain terrain;                       // Reference to terrain

	public Spectrum2 visualization;

	//-----------------------------------------------------------------------------------------------------------------
	[Header("Physics Settings")]

	[Tooltip("Current wind vector.")]
	public Vector3 wind;

	//-----------------------------------------------------------------------------------------------------------------
	[Header("Decoration Settings")]

	[Tooltip("Enable/disable decoration.")]
	public bool doDecorate = true;

	[SerializeField]
	[Tooltip("Total max decorations.")]
	private int maxDecorations;

	[SerializeField]
	[Tooltip("Current number of active decorations")]
	private int numDecorations;

	[Tooltip("Decoration group info for vegetation.")]
	public Decoration.GroupInfo vegetationGroup;

	[Tooltip("Decoration group info for road signs.")]
	public Decoration.GroupInfo roadSignGroup;

	[Tooltip("Decoration group info for rocks.")]
	public Decoration.GroupInfo rockGroup;

	[NonSerialized]
	public List<GameObject> decorations =                // List of all active decorations
		new List<GameObject>();

	List<string> decorationPaths = new List<string>() {  // List of load paths for decoration prefabs
		"Prefabs/Decoration_50mph",
		"Prefabs/Decoration_50mph45night",
		"Prefabs/Decoration_65MPH",
		"Prefabs/Decoration_70MPH",
		"Prefabs/Decoration_75mph",
		"Prefabs/Decoration_Agave01",
		"Prefabs/Decoration_BarrelCactus",
		"Prefabs/Decoration_Boulder01",
		"Prefabs/Decoration_Boulder02",
		"Prefabs/Decoration_Boulder03",
		"Prefabs/Decoration_Chevron",
		"Prefabs/Decoration_JoshuaTree01",
		"Prefabs/Decoration_PassWithCare",
		"Prefabs/Decoration_Saguaro",
		"Prefabs/DynamicDecoration_Tumbleweed01"
	};
		
	ObjectPool<Decoration> decorationPool;                           // Decoration pool to use
	
	[Tooltip("Current global decoration density.")]
	[Range(0f,2f)]
	public float decorationDensity = 1f;

	[Tooltip("Mesh to use for grass particles.")]
	public Mesh grassModel;

	[Tooltip("Material to use for vegetation decorations.")]
	public Material vegetationMaterial;

	[Tooltip("Template to use for grass particle emitters.")]
	public GameObject grassEmitterTemplate;

	public ParticleSystem decorationParticleEmitter;

	//-----------------------------------------------------------------------------------------------------------------
	[Header("Effects Settings")]

	[Tooltip("Base intensity of lightning effects.")]
	[Range(0.5f, 2f)]
	public float baseLightningIntensity = 1.5f;

	[Tooltip("GameObject to use for lightning strikes.")]
	public GameObject lightningStriker;

	[Tooltip("GameObject to use for in-cloud lightning flashes.")]
	public GameObject lightningFlash;

	[Tooltip("Star particle emitter.")]
	public ParticleSystem starEmitter;

	[Tooltip("Natural star emission rate.")]
	public float starEmissionRate = 6f;
	
	[Tooltip("Template to use to instantiate shooting stars.")]
	public GameObject shootingStarTemplate;

	[Tooltip("Cloud particle emitter.")]
	public ParticleSystem cloudEmitter;

	[Tooltip("Minimum number of cloud particles.")]
	public float minCloudDensity;

	[Tooltip("Maximum number of cloud particles.")]
	public float maxCloudDensity;

	[SerializeField]
	int cloudDensity;

	[Tooltip("Rain particle emitter.")]
	public ParticleSystem rainEmitter;

	[Tooltip("Minimum number of rain particles.")]
	public float minRainDensity;

	[Tooltip("Maximuim number of rain particles.")]
	public float maxRainDensity;

	[SerializeField]
	int rainDensity;

	[Tooltip("Number of active shakers.")]
	public int shakers;

	[Tooltip("All car exhaust puff emitters.")]
	public List<ParticleSystem> exhaustEmitters;

	//-----------------------------------------------------------------------------------------------------------------
	[Header("Road Settings")]

	[Tooltip("Width of generated road.")]
	[Range(1f, 20f)]
	public float roadWidth = DEFAULT_ROAD_WIDTH;

	[Tooltip("Height of generated road.")]
	[Range(0.1f, 1.0f)]
	public float roadHeight = DEFAULT_ROAD_HEIGHT;

	[Tooltip("Ratio of top of road to bottom.")]
	public float roadSlope = DEFAULT_ROAD_SLOPE;

	[Tooltip("Number of mesh subdivisions per road segment.")]
	public int roadStepsPerCurve = DEFAULT_ROAD_STEPS_PER_CURVE;

	[NonSerialized]
	public float roadExtendRadius = DEFAULT_ROAD_EXTEND_RADIUS;

	[NonSerialized]
	public float roadCleanupRadius;

	[Tooltip("Radius within which to place road.")]
	public float roadPlacementDistance = DEFAULT_ROAD_PLACEMENT_DISTANCE;

	[Tooltip("Percentage radius of road placement distance within which to place road.")]
	public float roadVariance = DEFAULT_ROAD_VARIANCE;

	[Tooltip("Max road slope per world unit of distance.")]
	public float roadMaxSlope = DEFAULT_ROAD_MAX_SLOPE;

	[NonSerialized]
	public Road road;                                    // Road object

	[Tooltip("Material to use for road.")]
	public Material roadMaterial;

	//-----------------------------------------------------------------------------------------------------------------
	[Header("Day/Night Cycle Settings")]

	[Tooltip("Global time scale for day/night cycle.")]
	[Range(0.001f, 0.1f)]
	public float timeScale = DEFAULT_TIME_SCALE;

	[Tooltip("Current time of day.")]
	[Range(0f, 2f*Mathf.PI)]
	public float timeOfDay;

	public GameObject sun;                               // Sun object
	Light sunLight;                                      // Sun object's light

	[Tooltip("Scale to use for the sun.")]
	public float sunScale;

	[Tooltip("Daytime intensity of the sun.")]
	[Range(0f, 8f)]
	public float maxSunIntensity;

	[Tooltip("Nighttime intensity of the sun.")]
	[Range(0f, 8f)]
	public float minSunIntensity;

	float sunIntensityAxis;                              // Axis of sun intensity oscillation
	float sunIntensityAmplitude;                         // Amplitude of sun intensity oscillation
	
	[Tooltip("Flare texture to use for the sun.")]
	public Flare sunFlare;

	public GameObject moon;                              // Moon object
	Light moonLight;                                     // Moon object's light

	[Tooltip("Scale to use for the moon.")]
	public float moonScale;

	[Tooltip("Nighttime intensity of the moon.")]
	[Range(0f, 8f)]
	public float maxMoonIntensity;

	[Tooltip("Daytime intensity of the moon.")]
	[Range(0f, 8f)]
	public float minMoonIntensity;

	float moonIntensityAxis;                             // Axis of moon intensity oscillation
	float moonIntensityAmplitude;                        // Amplitude of moon intensity oscillation

	[Tooltip("Sprites to randomize for the moon.")]
	public List<Sprite> moonSprites;

	[Tooltip("Current primary color.")]
	[SerializeField]
	private Color primaryColor;

	[Tooltip("Current secondary color.")]
	[SerializeField]
	private Color secondaryColor;

	[Tooltip("Primary color cycle.")]
	public Gradient primaryColors;

	[Tooltip("Secondary color cycle.")]
	public Gradient secondaryColors;

	[Tooltip("Skybox transition cycle.")]
	public Gradient skyboxFade;

	//-----------------------------------------------------------------------------------------------------------------
	[Header("Performance Settings")]

	[Tooltip("Maximum number of chunk updates per cycle.")]
	[Range(1,16)]
	public int chunkUpdatesPerCycle = DEFAULT_CHUNK_UPDATES_PER_CYCLE;

	[Tooltip("Resolution used in frequency spectrum analysis. Must be a power of 2.")]
	public int freqArraySize = DEFAULT_FREQ_ARRAY_SIZE;

	LineRenderer visualizer;                                // Frequency visualizer

	[Tooltip("FFT window to use when sampling music frequencies.")]
	public FFTWindow freqFFTWindow;

	[Tooltip("Maximum number of decorations to place per update cycle.")]
	[Range(10, 200)]
	public int decorationsPerStep = DEFAULT_DECORATIONS_PER_STEP;

	[Tooltip("Maximum number of grass particles to place per chunk.")]
	[Range(0,100)]
	public int grassPerChunk;

	[Tooltip("The accuracy used in road distance checks.")]
	[Range(1f, 500f)]
	public float roadPathCheckResolution = DEFAULT_ROAD_PATH_CHECK_RESOLUTION;

	float startLoadTime;
	bool loaded = false;
	public bool loadedTerrain = false;
	bool hasRandomized = false;

	#endregion
	#region Unity Callbacks

	// Use this for initialization
	void Awake () {
		instance = this;

		maxDecorations = roadSignGroup.maxActive + rockGroup.maxActive + vegetationGroup.maxActive;
		//Debug.Log(maxDecorations);

		terrain = new GameObject ("Terrain",
			typeof(DynamicTerrain)
		).GetComponent<DynamicTerrain> ();
		wind = UnityEngine.Random.insideUnitSphere;

		visualization.enabled = false;

		//roadPlacementDistance = chunkSize * 2f;
		roadCleanupRadius = chunkSize * (chunkLoadRadius);
		roadPlacementDistance = chunkSize * 0.4f;
		roadExtendRadius = chunkSize * (chunkLoadRadius/2);
		road = CreateRoad();

		numDecorations = 0;
		decorationPool = new ObjectPool<Decoration>();

		timeOfDay = UnityEngine.Random.Range(0, 2*Mathf.PI);
		CreateSun();
		sunLight = sun.Light();
		sunIntensityAmplitude = (maxSunIntensity-minSunIntensity)/2f;
		sunIntensityAxis = minSunIntensity + sunIntensityAmplitude;

		CreateMoon();
		moonLight = moon.Light();
		moonIntensityAmplitude = (maxMoonIntensity-maxMoonIntensity)/2f;
		moonIntensityAxis = minMoonIntensity + moonIntensityAmplitude;

		//RenderSettings.ambientMode = AmbientMode.Flat;
		//RenderSettings.ambientIntensity = 0.5f;

		lightningStriker.SetActive(false);
		rainEmitter.SetRate(0f);
		shootingStarTemplate.SetActive(false);

		terrainMaterial.SetFloat("_WaveProgress", 1f);
	}

	void Start () {
		loadsToDo = chunkLoadRadius * chunkLoadRadius + 
			(doDecorate ? maxDecorations + decorationPaths.Count : 0) +
			terrain.loadsToDo;
	}

	// Update is called once per frame
	void Update () {
		if (loadedTerrain && !hasRandomized) 
			Load();
		if (loaded && road.loaded) {

			//terrain.Update(freqDataArray);
			UpdateTime();
			UpdateColor();
			if (doDecorate) {
				AttemptDecorate();
				Vector3 dWind = UnityEngine.Random.insideUnitSphere;
				wind += dWind * Time.deltaTime;
				wind.Normalize();
			}
			
			float cd = shakers / (Riff.MAX_BEATS * 4f);
			cloudDensity = Mathf.FloorToInt(minCloudDensity + cd * (maxCloudDensity - minCloudDensity));
			cloudEmitter.maxParticles = cloudDensity;

			float rd = shakers / (Riff.MAX_BEATS * 2f);
			rainDensity = Mathf.FloorToInt(minRainDensity + rd * (maxRainDensity - minRainDensity));
			rainEmitter.SetRate(rainDensity);

			starEmitter.SetRate(0.5f*starEmissionRate*-Mathf.Sin(timeOfDay)+starEmissionRate/2f);
		}

	}

	#endregion
	#region WorldManager Methods

	public void Load () {

		Camera.main.GetComponent<SunShafts>().sunTransform = sun.transform;

		// Get start time
		startLoadTime = Time.realtimeSinceStartup;

		// Start by loading chunks
		terrain.DoLoadChunks();
	}

	public void FinishLoading() {

		loaded = true;

		// Print time taken
		Debug.Log("WorldManager.Load(): finished in "+(Time.realtimeSinceStartup-startLoadTime).ToString("0.0000")+" seconds.");

		visualization.enabled = true;

		// Call GameManager to finish loading
		GameManager.instance.FinishLoading();
	}
	public void DoLoadRoad () {
		road.DoLoad();
	}

	public void DoLoadDecorations () {
		StartCoroutine("LoadDecorations");
	}

	IEnumerator LoadDecorations () {
		GameManager.instance.ChangeLoadingMessage("Loading decorations...");
		float startTime = Time.realtimeSinceStartup;
		int numLoaded = 0;

		foreach (string path in decorationPaths) {
			LoadDecoration (path);
			numLoaded++;

			if (Time.realtimeSinceStartup - startTime > GameManager.instance.targetDeltaTime) {
				yield return null;
				startTime = Time.realtimeSinceStartup;
				GameManager.instance.ReportLoaded(numLoaded);
				numLoaded = 0;
			}
		}

		if (decorations.Count == decorationPaths.Count)
			DoDecoration();
		yield return null;
	}

	public void DoDecoration () {
		StartCoroutine("DecorationLoop");
	}

	IEnumerator DecorationLoop () {

		List<string> loadMessages = new List<string>() {
			"Planting cacti...",
			"Placing doodads...",
			"Landscaping...",
		};

		GameManager.instance.ChangeLoadingMessage(loadMessages.Random());
		float startTime = Time.realtimeSinceStartup;
		int numLoaded = 0;

		while (true) {
			if (numDecorations < maxDecorations) {
				numLoaded += (AttemptDecorate () ? 1 : 0);

				if (numDecorations == maxDecorations && !loaded) {
					FinishLoading();
					yield return null;
				}

				if (Time.realtimeSinceStartup - startTime > GameManager.instance.targetDeltaTime) {
					yield return null;
					startTime = Time.realtimeSinceStartup;
					if (!loaded) GameManager.instance.ReportLoaded(numLoaded);
					numLoaded = 0;
				}

			} else {
				yield return null;
			}

	
		}
			
	}
		
	void UpdateColor() {
		
		float progress = timeOfDay/(Mathf.PI*2f);

		primaryColor = primaryColors.Evaluate(progress);
		secondaryColor = secondaryColors.Evaluate(progress);

		sunLight.intensity = sunIntensityAxis + sunIntensityAmplitude * Mathf.Sin (timeOfDay);
		sunLight.color = primaryColor;
		sun.GetComponent<Sun>().shadowCaster.intensity = sunLight.intensity/2f;
		sun.GetComponent<Sun>().shadowCaster.color = sunLight.color;

		moonLight.color = Color.white;
		moonLight.intensity = moonIntensityAxis + moonIntensityAmplitude * Mathf.Cos(timeOfDay-(Mathf.PI/2f));
		moon.GetComponent<Moon>().shadowCaster.intensity = moonLight.intensity/4f;
		moon.GetComponent<Moon>().shadowCaster.color = moonLight.color;

		RenderSettings.fogColor = secondaryColor;
		RenderSettings.ambientLight = secondaryColor;

		RenderSettings.skybox.SetFloat("_Value", skyboxFade.Evaluate(progress).a);

		if (Spectrum2.instance != null) {

			if (visualizer == null) visualizer = Spectrum2.instance.GetComponent<LineRenderer>();

			Color temp = primaryColor;
			temp.a = Spectrum2.instance.opacity;

			visualizer.SetColors (temp, temp);
			visualizer.material.color = temp;
		}
	}

	private void UpdateTime() {
		timeOfDay += timeScale * Time.deltaTime;
		while (timeOfDay > (2 * Mathf.PI)) { //clamp timeOfDay between 0 and 2PI)
			timeOfDay -= 2 * Mathf.PI;
		}
	}
    
	bool AttemptDecorate () {

		// Pick a random decoration and decorate with it
		GameObject decoration;
		bool createNew = false;
		if (decorationPool.Empty) {
			decoration = decorations[UnityEngine.Random.Range(0, decorations.Count)];
			createNew = true;
		} else decoration = decorationPool.Peek().gameObject;

		//if (!createNew) Debug.Log("old");
	
		Decoration deco = decoration.GetComponent<Decoration>();
	
		int numActive = 0;
		int maxActive = 0;
		switch (deco.group) {
		case Decoration.Group.RoadSigns:
			numActive = roadSignGroup.numActive;
			maxActive = roadSignGroup.maxActive;
			break;
		case Decoration.Group.Rocks:
			numActive = rockGroup.numActive;
			maxActive = rockGroup.maxActive;
			break;
		case Decoration.Group.Vegetation:
			numActive = vegetationGroup.numActive;
			maxActive = vegetationGroup.maxActive;
			break;
		}
		if (numActive < maxActive) {
			switch (deco.distribution) {
			case Decoration.Distribution.Random:
				Chunk chunk = terrain.RandomChunk ();
				if (chunk == null) return false;
				return DecorateRandom (chunk, decoration, createNew);
			case Decoration.Distribution.Roadside:
				float bezierProg = UnityEngine.Random.Range (PlayerMovement.instance.progress, 1f);
				return DecorateRoadside (bezierProg, decoration, createNew);
			case Decoration.Distribution.CloseToRoad:
				Chunk chunk2 = terrain.RandomCloseToRoadChunk();
				if (chunk2 == null) return false;
				return DecorateRandom (chunk2, decoration, createNew);
			}
		}
	
		return false;
	}
    
	/*GameObject CreateSun(){
		GameObject sun = new GameObject ("Sun",
			typeof (Light),
			typeof (Sun),
			typeof (LensFlare)
		);

		Light light = sun.Light();
		light.shadows = LightShadows.Soft;
		light.flare = sunFlare;

		LensFlare flare = sun.GetComponent<LensFlare>();
		flare.flare = sunFlare;
		//flare.

		return sun;
	}*/

	void CreateSun () {

	}

	void CreateMoon () {
		moon.GetComponent<SpriteRenderer>().sprite = 
			moonSprites[UnityEngine.Random.Range(0,moonSprites.Count)];
	}

	/*GameObject CreateMoon(){
		GameObject moon = new GameObject ("Moon",
			typeof (Light),
			typeof (Moon),
			typeof (SpriteRenderer)
		);

		Light light = moon.Light();
		light.shadows = LightShadows.Soft;

		// Random moon phase
		moon.GetComponent<SpriteRenderer>().sprite = 
			moonSprites[UnityEngine.Random.Range(0,moonSprites.Count)];
		return  moon;
	}*/

	Road CreateRoad() {

		// Create road object
		GameObject roadObj = new GameObject ("Road",
			typeof (MeshFilter),
			typeof (MeshRenderer),
			typeof (Road)
		);

		// Change renderer properties
		MeshRenderer roadRenderer = roadObj.GetComponent<MeshRenderer>();
		roadRenderer.sharedMaterial = roadMaterial;
		roadRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

		// Pass on road properties
		Road rd = roadObj.GetComponent<Road>();
		rd.width = roadWidth;
		rd.height = roadHeight;

		return rd;
	}

	bool DecorateRandom (Chunk chunk, GameObject decorationPrefab, bool createNew) {
		if (chunk == null) {
			Debug.LogError("WorldManager.DecorateRandom(): invalid chunk!");
			return false;
		}

		//if (!createNew) Debug.Log("old");

		// Pick a random coordinate
		Vector2 coordinate = new Vector2 (
			chunk.x*chunkSize+UnityEngine.Random.Range(-chunkSize/2f, chunkSize/2f),
			chunk.y*chunkSize+UnityEngine.Random.Range(-chunkSize/2f, chunkSize/2f)
		);

		// Find nearest vertex
		IntVector2 nearestVertex = Chunk.ToNearestVMapCoords(coordinate.x, coordinate.y);
		Vertex vert = terrain.vertexmap.VertexAt (nearestVertex);
		if (vert == null) {
			Debug.LogError ("WorldManager.DecorateRandom(): picked nonexistent vertex at " + nearestVertex.ToString ());
			return false;
		}

		// Check if constrained
		if (terrain.vertexmap.VertexAt(nearestVertex).noDecorations) {
			//Debug.Log(nearestVertex.ToString() + " was constrained, picked chunk "+chunk.name);
			return false;
		}

		// Roll based on density
		float density = decorationPrefab.GetComponent<Decoration>().density;
		float spawnThreshold = density / terrain.NumActiveChunks * decorationDensity;

		// If roll succeeded
		if (!createNew || Mathf.PerlinNoise (coordinate.x, coordinate.y) < spawnThreshold) {

			// Instantiate or grab object
			GameObject decoration;
			if (createNew) decoration = (GameObject)Instantiate(decorationPrefab);
			else decoration = decorationPool.Get().gameObject;
			Decoration deco = decoration.GetComponent<Decoration>();

			// Raycast down 
			RaycastHit hit;
			float y;
			Vector3 rayOrigin = new Vector3 (coordinate.x, heightScale, coordinate.y);
			if (Physics.Raycast(rayOrigin, Vector3.down,out hit, Mathf.Infinity)) y = hit.point.y;
			else y = 0f;

			// Transform decoration
			decoration.transform.position = new Vector3 (coordinate.x, y, coordinate.y);

			// Parent decoration to chunk (if not dynamic)
			if (deco.dynamic) decoration.transform.parent = terrain.transform;
			else {
				decoration.transform.parent = chunk.gameObject.transform;
				chunk.decorations.Add(decoration);
			}

			// Register decoration
			numDecorations++;
			terrain.vertexmap.RegisterDecoration (nearestVertex, decoration);
			switch (deco.group) {
			case Decoration.Group.Rocks:
				rockGroup.numActive++;
				break;
			case Decoration.Group.Vegetation:
				vegetationGroup.numActive++;
				break;
			}


			decorationParticleEmitter.transform.position = decoration.transform.position;
			//ParticleSystem.EmitParams emitOverride = new ParticleSystem.EmitParams();
			//emitOverride.position = decoration.transform.position;
			//decorationParticleEmitter.Emit(emitOverride, 5);
			decorationParticleEmitter.Emit(5);
				
			return true;
		}

		return false;
	}

	bool DecorateRoadside (float prog, GameObject decorationPrefab, bool createNew) {

		// Get road point
		Vector3 point = road.GetPoint(prog);

		// Pick a road side
		int side = UnityEngine.Random.Range (0, 2); // 0 = player side, 1 = other side

		// Calculate coordinate
		Vector3 coordinate = point + road.BezRight(point) * 
			roadWidth * UnityEngine.Random.Range(1.5f, 1.6f) * (side == 0 ? 1 : -1);

		// Find nearest chunk
		int chunkX = Mathf.RoundToInt((coordinate.x-chunkSize/2f)/chunkSize);
		int chunkY = Mathf.RoundToInt((coordinate.z-chunkSize/2f)/chunkSize);
		Chunk chunk = terrain.ChunkAt(chunkX,chunkY);

		if (chunk == null) return false;

		// Raycast down
		RaycastHit hit;
		Vector3 rayOrigin = new Vector3 (coordinate.x, heightScale, coordinate.y);
		if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity)) coordinate.y = hit.point.y;
		else coordinate.y = 0f;

		// Instantiate or grab decoration
		GameObject decoration;
		if (createNew) decoration = 
			(GameObject)Instantiate(decorationPrefab, coordinate, Quaternion.Euler(Vector3.zero));
		else decoration = decorationPool.Get().gameObject;

		// Randomize
		Decoration deco = decoration.GetComponent<Decoration>();
		deco.Randomize();

		// Point decoration in road direction
		Vector3 target = coordinate + road.GetVelocity(prog);
		decoration.transform.LookAt (target, Vector3.up);
		decoration.transform.Rotate(-90f, side == 1 ? 180f : 0f, 0f);

		// Parent to nearest chunk
		decoration.transform.parent = chunk.gameObject.transform;
		chunk.decorations.Add(decoration);

		// Register
		numDecorations++;
		if (deco.group == Decoration.Group.RoadSigns) roadSignGroup.numActive++;

		return true;
	}

	void LoadDecoration (string path) {
		GameObject decoration = (GameObject) Resources.Load(path);
		if (decoration == null) {
			Debug.LogError ("Failed to load decoration at "+path);
		} else {
			//Debug.Log("Loaded "+path);
			decorations.Add(decoration);
			//GameManager.instance.IncrementLoadProgress();
		}
	}

	public void RemoveDecoration (GameObject deco) {
		Decoration d = deco.GetComponent<Decoration>();

		// Deparent decoration
		deco.transform.parent = null;


		// Deregister
		switch (d.group) {
		case Decoration.Group.RoadSigns:
			roadSignGroup.numActive--;
			break;
		case Decoration.Group.Rocks:
			rockGroup.numActive--;
			break;
		case Decoration.Group.Vegetation:
			vegetationGroup.numActive--;
			break;
		}
		numDecorations--;

		// Pool decoration
		decorationPool.Add(d);
	}

	/// <summary>
	/// Creates a lightning strike at a random point in view.
	/// </summary>
	public void LightningStrike (float strength) {

		// Find camera forward direction (flat)
		Vector3 forward = Camera.main.transform.forward;
		forward.y = 0f;
		forward.Normalize();

		// Define an offset
		Vector2 r = UnityEngine.Random.insideUnitCircle;
		Vector3 offset = new Vector3 (400f*r.x, 250f, 400f*r.y);

		// Pick a point
		Vector3 origin = PlayerMovement.instance.transform.position + forward * UnityEngine.Random.Range(0.9f, 1.1f) *
			vertexUpdateDistance + offset;

		// Play sound
		// TODO

		// Enable lightning striker and move to point
		lightningStriker.SetActive(true);
		lightningStriker.transform.position = origin;
		lightningStriker.Light().intensity = baseLightningIntensity*strength;
	}

	/// <summary>
	/// Creates a lightning flash within the clouds.
	/// </summary>
	/// <param name="strength">Strength.</param>
	public void LightningFlash (float strength) {

		// Find camera forward direction (flat)
		Vector3 forward = Camera.main.transform.forward;
		forward.y = 0f;
		forward.Normalize();

		// Define an offset
		Vector2 r = UnityEngine.Random.insideUnitCircle;
		Vector3 offset = new Vector3 (800f*r.x, 800f, 800f*r.y);

		// Pick a point
		Vector3 origin = PlayerMovement.instance.transform.position + forward * UnityEngine.Random.Range(1.9f, 2.1f) *
			vertexUpdateDistance + offset;

		// Play sound
		// TODO

		// Enable lightning flash and move to point
		lightningFlash.SetActive(true);
		lightningFlash.transform.position = origin;
		lightningFlash.Light().intensity = baseLightningIntensity*strength;
	}

	public void StarBurst () {
		starEmitter.Emit(UnityEngine.Random.Range(10,20));
	}

	public void ExhaustPuff () {
		foreach (ParticleSystem sys in exhaustEmitters) sys.Emit(1);
	}

	public void ShootingStar () {

		// Find camera forward direction (flat)
		Vector3 forward = Camera.main.transform.forward;
		forward.y = 0f;
		forward.Normalize();

		// Define an offset
		Vector2 r = UnityEngine.Random.insideUnitCircle;
		Vector3 offset = new Vector3 (1000f*r.x, 600f, 1000f*r.y);

		// Pick a point
		Vector3 origin = PlayerMovement.instance.transform.position + forward * UnityEngine.Random.Range(2.9f, 3.1f) *
			vertexUpdateDistance + offset;
		
		GameObject shootingStar = (GameObject)Instantiate(shootingStarTemplate, origin, Quaternion.identity);
		shootingStar.SetActive(true);
	}

	public void DeformRandom () {
		// Find camera forward direction (flat)
		Vector3 forward = Camera.main.transform.forward;
		forward.y = 0f;
		forward.Normalize();

		// Define an offset
		Vector2 r = UnityEngine.Random.insideUnitCircle;
		Vector3 offset = new Vector3 (400f*r.x, 0f, 400f*r.y);

		// Pick a point
		Vector3 origin = PlayerMovement.instance.transform.position + forward * UnityEngine.Random.Range(0.9f, 1.1f) *
			vertexUpdateDistance + offset;

		IntVector2 coords = Chunk.ToNearestVMapCoords(origin.x, origin.z);
		Vertex v = terrain.vertexmap.VertexAt(coords);
		if (v == null) v = terrain.vertexmap.AddVertex (coords);
		if (!v.locked && !v.nearRoad) v.SmoothHeight (v.height + heightScale/32f, 0.95f);

		//Debug.Log(v.ToString());
	}

	public void DebugTerrain () {
		terrain.SetDebugColors(DynamicTerrain.DebugColors.Constrained);
	}

	public void PrintVertexMap () {
		VertexMap vmap = terrain.vertexmap;
		string log = "";
		for (int i = vmap.xMin; i <= vmap.xMax; i++) {
			for (int j = vmap.yMin; j <= vmap.yMax; j++) {
				Vertex vert = vmap.VertexAt(i, j);
				log += "[" + (vert != null ? (vert.height < 0f ? "-" : " ") + vert.height.ToString("000") : "    ") + "]";
			}
			log += "\n";
		}
		System.IO.File.WriteAllText(Application.persistentDataPath + "/vmap.txt", log);
	}

	#endregion

}
