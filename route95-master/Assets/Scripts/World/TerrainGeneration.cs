using UnityEngine;
using System.Collections;

public class TerrainGeneration : MonoBehaviour {

	public int SIZE; //the width of the terrain square in Unity distance units
	public int LINEAR_RESOLUTION; //the number of vertices per row/column, minimum 2
	public Material terrainMat; //the material to apply to the terrain

	private GameObject terrain;

	//if no value given for parameters in Unity Editor, set to these defaults

	void InitializeParams () { //if given defaults of 0 for SIZE and LINEAR RESOLUTION, set to working values
		if (SIZE == 0)
			SIZE = 10;
		if (LINEAR_RESOLUTION == 0)
			LINEAR_RESOLUTION = 100;
		if (terrainMat == null) {
			Debug.LogError ("No material given for terrain.");
		}
	}

	//takes the number of vertices per side, returns a square array of vertices equidistant to eachother
	Vector3[] createUniformVertexArray(int vertexSize){ 
		float scale = (float)SIZE / LINEAR_RESOLUTION;
		int numVertices = vertexSize * vertexSize;
		Vector3[] uniformArray = new Vector3 [numVertices];
		for (int vert = 0; vert < numVertices; vert++) {
			uniformArray [vert] = new Vector3 (vert % vertexSize, 0, vert / vertexSize); //create vertex
			uniformArray [vert] = uniformArray[vert] * scale; //scale vector appropriately
		}
		return uniformArray;
	}

	//takes the number of vertices per side, returns a uniform array of UV coords for a square vertex array
	Vector2[] createUniformUVArray(int vertexSize) {
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

	//takes the number of vertices per side, returns indices (each group of three defines a triangle) into a square vertex array to form mesh 
	int[] createSquareArrayTriangles (int vertexSize){ 
		int numTriangles = 2 * vertexSize * (vertexSize - 1);//a mesh with n^2 vertices has 2n(n-1) triangles
		int[] triangleArray = new int[numTriangles * 3]; //three points per triangle 
		int numVertices = vertexSize * vertexSize;
		int i = 0; //index into triangleArray (next two are the sibling vertices for its triangle, add 3 to jump to next triangle)
		for (int vert = 0; vert < numVertices - vertexSize; vert++) {
			/* Make these types of triangles
			 * 1---2
			 * *\**|
			 * **\*|
			 * ***\|
			 * ****3
			 */
			if (((vert + 1) % vertexSize) != 0) { //if vertex is not on the right edge
				triangleArray [i] = vert; //vertex 1
				triangleArray [i + 1] = vert + 1; //vertex 2
				triangleArray [i + 2] = vert + vertexSize; //vertex 3
				i = i + 3; //jump to next triangle
			}

			/* Make these types of triangles
			 * ****1 
			 * ***7|
			 * **7*|
			 * *7**|
			 * 3---2
			 */
			if ((vert % vertexSize) != 0) { //if vertex is not on the left edge
				triangleArray [i] = vert; //vertex 1
				triangleArray [i + 1] = vert + vertexSize; //vertex 2
				triangleArray [i + 2] = vert + vertexSize - 1; //vertex 3
				i = i + 3; //jump to next triangle
			}
		}
		return triangleArray;
	}
		
	//create terrain gameobject with mesh
	GameObject createTerrain (Vector3[] vertices, Vector2[] UVcoords, int[] triangles) {
		terrain = new GameObject ("terrain", typeof(MeshFilter), typeof(MeshRenderer));
		terrain.transform.position = new Vector3 (-SIZE/2, 0, -SIZE/2);

		//mesh filter stuff
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.uv = UVcoords;
		mesh.triangles = triangles;
		terrain.GetComponent<MeshFilter> ().mesh = mesh;

		//mesh renderer stuff
		terrain.GetComponent<MeshRenderer> ().sharedMaterial = terrainMat;

		return terrain;
	}


	void Start () {
		InitializeParams ();
		Vector3[] vertexArray = createUniformVertexArray(LINEAR_RESOLUTION);
		Vector2[] uvArray = createUniformUVArray (LINEAR_RESOLUTION);
		int[] trianglesArray = createSquareArrayTriangles(LINEAR_RESOLUTION);
		terrain = createTerrain (vertexArray, uvArray, trianglesArray);
	}

	void Update () {
	
	}
}
