using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Vertex {

	public DynamicTerrain terrain;
	static int chunkRes;
	static float chunkSize;
	public VertexMap map;
	public List<KeyValuePair<IntVector2, int>> chunkVertices;
	public bool locked = false;
	public int x;
	public int y;
	public float height = 0f;
	public float currHeight = 0f;
	public bool nearRoad = false;
	public bool noDecorations = false;
	public Vector3 normal = Vector3.up;
	public float slope = 0f;
	public Color color;
	public List<GameObject> decorations;
	public bool loaded = false;

	public Vertex (int x, int y) {
		this.x = x;
		this.y = y;
		chunkVertices = new List<KeyValuePair<IntVector2, int>>();
		decorations = new List<GameObject>();
		chunkRes = WorldManager.instance.chunkResolution;
		chunkSize = WorldManager.instance.chunkSize;
		color = new Color (
			0f,
			UnityEngine.Random.Range(0f, 1f), 
			UnityEngine.Random.Range(0f, 0.75f), 
			0.5f
		);
	}

	public void SmoothHeight (float h, float factor, float range) {
		SetHeight (h);
		Vector2 v = new Vector2 ((float)x, (float)y);
		for (int ix = map.xMin; ix <= map.xMax; ix++) {
			if ((float)Mathf.Abs(ix - x) > range) continue;
			for (int iy = map.yMin; iy <= map.yMax; iy++) {
				if ((float)Mathf.Abs(iy - y) > range) continue;

				Vector2 n = new Vector2 ((float)ix, (float)iy);
				float dist = Vector2.Distance (n,v);
				if (dist > range) continue;

				if (ix == x && iy == y) continue;

				Vertex vert = map.VertexAt(ix, iy);
				if (vert != null && !vert.locked) vert.Smooth(h, factor * (range-dist) / range);
			}
		}
	}

	public void SmoothHeight (float h, float factor) {
		SetHeight (h);
		Vertex l = map.LeftNeighbor (this);
		if (l != null && !l.locked && !l.nearRoad) l.Smooth (h, factor);

		Vertex r = map.RightNeighbor (this);
		if (r != null && !r.locked && r.nearRoad) r.Smooth (h, factor);

		Vertex d = map.DownNeighbor (this);
		if (d != null && !d.locked && d.nearRoad) d.Smooth (h, factor);

		Vertex u = map.UpNeighbor (this);
		if (u != null && !u.locked && u.nearRoad) u.Smooth (h, factor);

		float factor_squared = factor * factor;

		if (u != null) {
			Vertex ul = map.LeftNeighbor (u);
			if (ul != null && !ul.locked && ul.nearRoad) ul.Smooth (h, factor_squared);

			Vertex ur = map.RightNeighbor (u);
			if (ur != null && !ur.locked && ur.nearRoad) ur.Smooth (h, factor_squared);
		}

		if (d != null) {

			Vertex dl = map.LeftNeighbor (d);
			if (dl != null && !dl.locked && dl.nearRoad) dl.Smooth (h, factor_squared);

			Vertex dr = map.RightNeighbor (d);
			if (dr != null && !dr.locked && dr.nearRoad) dr.Smooth (h, factor_squared);
		}
	}

	public void Smooth (float h, float factor) {
		//if (UnityEngine.Random.Range(0,100) == 1) Debug.Log(factor);
		//Debug.Log(h);
		SetHeight (height + (h-height) * factor);
		//Debug.Log(h);
	}

	/// <summary>
	/// Returns true if the given x/y coordinate is an edge between chunks.
	/// </summary>
	/// <param name="coord"></param>
	/// <returns></returns>
	bool IsEdge (int coord) {
		return (coord % (chunkRes-1) == 0);
	}

	/// <summary>
	/// Converts a coordinate into chunk space.
	/// </summary>
	/// <param name="coord"></param>
	/// <returns></returns>
	int ChunkAt (int coord) {
		return coord / (chunkRes-1) - (coord < 0 ? 1 : 0);
	}

	/// <summary>
	/// Returns the left-/down-most chunk on an edge.
	/// </summary>
	/// <param name="coord"></param>
	/// <returns></returns>
	int ChunkMin (int coord) {
		return ChunkMax(coord) - 1;
	}

	/// <summary>
	/// Returns the right-/up-most chunk on an edge.
	/// </summary>
	/// <param name="coord"></param>
	/// <returns></returns>
	int ChunkMax (int coord) {
		return coord / (chunkRes-1);
	}

	/// <summary>
	/// Converts a vertex coordinate to a mesh vert index.
	/// </summary>
	/// <param name="chunkX"></param>
	/// <param name="chunkY"></param>
	/// <returns></returns>
	int CoordToIndex (int chunkX, int chunkY) {
		int localX;
		if (chunkX >= 0) localX = x - chunkX * (chunkRes-1);
		else localX = x + Mathf.Abs(chunkX) * (chunkRes-1);

		if (localX >= chunkRes || localX < 0)
			throw new ArgumentOutOfRangeException ("Vertex.CoordToIndex(): x coord "+x+" not on chunk "+chunkX+"!");

		int localY;
		if (chunkY >= 0) localY = y - chunkY * (chunkRes-1);
		else localY = y + Mathf.Abs(chunkY) * (chunkRes-1);

		if (localY >= chunkRes || localY < 0) 
			throw new ArgumentOutOfRangeException ("Vertex.CoordToIndex(): y coord "+y+" not on chunk "+chunkY+"!");

		int i = localY * chunkRes + localX;

		return i;
	}

	void CalculateBlend () {
		bool debug = (UnityEngine.Random.Range(0,10000) == 1);
		if (debug) Debug.Log(ToString());

		int numNeighbors = 0;
		float delta = 0f;

		Vertex l = map.VertexAt(x-1, y);
		if (l != null && l.loaded) {
			numNeighbors++;
			float diff = Mathf.Abs (height - l.height);
			if (debug) Debug.Log("Left: "+l.ToString()+" "+diff);
			delta += diff;
		}

		Vertex r = map.VertexAt(x+1, y);
		if (r != null && r.loaded) {
			numNeighbors++;
			float diff = Mathf.Abs (height - r.height);
			if (debug) Debug.Log("Right: "+r.ToString()+" "+diff);
			delta += diff;
		}

		Vertex u = map.VertexAt(x, y+1);
		if (u != null && u.loaded) {
			numNeighbors++;
			float diff = Mathf.Abs (height - u.height);
			if (debug) Debug.Log("Up: "+u.ToString()+" "+diff);
			delta += diff;
		}

		Vertex d = map.VertexAt(x, y-1);
		if (d != null && d.loaded) {
			numNeighbors++;
			float diff = Mathf.Abs (height - d.height);
			if (debug) Debug.Log("Down: "+d.ToString()+" "+diff);
			delta += diff;
		}

		color.a = delta / (float)numNeighbors / 100f;
		if (debug) Debug.Log("final blend: "+color.a);
	}

	void CalculateBlend2 () {
		float max = 0f;

		Vertex l = map.VertexAt(x-1, y);
		if (l != null && l.loaded) {
			float diff = Mathf.Abs(l.height - height);
			if (diff > max) max = diff;
		}

		Vertex r = map.VertexAt(x+1, y);
		if (r != null && r.loaded) {
			float diff = Mathf.Abs(r.height - height);
			if (diff > max) max = diff;
		}

		Vertex u = map.VertexAt(x, y+1);
		if (u != null && u.loaded) {
			float diff = Mathf.Abs(u.height - height);
			if (diff > max) max = diff;
		}

		Vertex d = map.VertexAt(x, y-1);
		if (d != null && d.loaded) {
			float diff = Mathf.Abs(d.height - height);
			if (diff > max) max = diff;
		}

		color.a = Mathf.Clamp01 (max / 50f);
	}

		
	public void SetHeight (float h) {
		// Skip locked vertices
		if (locked || h == height) return;

		loaded = true;

		// Set height
		height = h;

		color.a = 0f;
		/*Vertex l = map.VertexAt(x-1,y);
		color.a += (l != null ? Mathf.Abs (h - l.height) : 0f);

		Vertex r = map.VertexAt(x+1,y);
		color.a += (r != null ? Mathf.Abs (h - r.height) : 0f);

		Vertex u = map.VertexAt(x,y+1);
		color.a += (u != null ? Mathf.Abs (h - u.height) : 0f);

		Vertex d = map.VertexAt(x,y-1);
		color.a += (d != null ? Mathf.Abs (h - d.height) : 0f);

		color.a /= (WorldManager.instance.heightScale/10f);
		color.a = Mathf.Clamp01(color.a);*/

		CalculateBlend2();

		int index;

		if (IsEdge (x)) {

			// Corner
			if (IsEdge (y)) {

				Chunk ul = terrain.ChunkAt (ChunkMin(x), ChunkMax (y));
				if (ul != null) {
					index = CoordToIndex (ul.x, ul.y);
					ul.UpdateVertex (index, height, true);
					ul.UpdateColor (index, color);
				}

				Chunk ur = terrain.ChunkAt (ChunkMax(x), ChunkMax (y));
				if (ur != null) {
					index = CoordToIndex (ur.x, ur.y);
					ur.UpdateVertex (index, height, true);
					ur.UpdateColor (index, color);
				}

				Chunk dl = terrain.ChunkAt (ChunkMin(x), ChunkMin (y));
				if (dl != null) {
					index = CoordToIndex (dl.x, dl.y);
					dl.UpdateVertex (index, height, true);
					dl.UpdateColor (index, color);
				}

				Chunk dr = terrain.ChunkAt (ChunkMax(x), ChunkMin (y));
				if (dr != null) {
					index = CoordToIndex (dr.x, dr.y);
					dr.UpdateVertex (index, height, true);
					dr.UpdateColor (index, color);
				}

			// X edge
			} else {
				Chunk left = terrain.ChunkAt(ChunkMin (x), ChunkAt(y));
				if (left != null) {
					index = CoordToIndex (left.x, left.y);
					left.UpdateVertex (index, height, true);
					left.UpdateColor (index, color);
				} 

				Chunk right = terrain.ChunkAt(ChunkMax (x), ChunkAt(y));
				if (right != null) {
					index = CoordToIndex(right.x, right.y);
					right.UpdateVertex(index, height, true);
					right.UpdateColor (index, color);
				}
			} 

		// Y edge
		} else if (IsEdge (y)) {
			Chunk bottom = terrain.ChunkAt(ChunkAt(x), ChunkMin(y));
			if (bottom != null) {
				index = CoordToIndex (bottom.x, bottom.y);
				bottom.UpdateVertex (index, height, true);
				bottom.UpdateColor (index, color);
			}

			Chunk top = terrain.ChunkAt(ChunkAt(x), ChunkMax(y));
			if (top != null) {
				index = CoordToIndex (top.x, top.y);
				top.UpdateVertex (index, height, true);
				top.UpdateColor (index, color);
			}
		
		// No edge
		} else {
			Chunk chunk = terrain.ChunkAt(ChunkAt(x), ChunkAt(y));
			if (chunk != null) {
				index = CoordToIndex (chunk.x, chunk.y);
				chunk.UpdateVertex (index, height, false);
				chunk.UpdateColor (index, color);
			}
		}
	}

	public void AddHeight (float h) {
		SetHeight (height + h);
	}

	public void lerpHeight(float factor) {
		float diff = height - currHeight;
		currHeight += diff * factor;
	}

	// Returns the world position of a vertex
	public Vector3 WorldPos () {
		Vector3 result = new Vector3 (
			(float)x / (float)(chunkRes-1) * chunkSize - chunkSize/2f,
			//x * WorldManager.instance.chunkSize - WorldManager.instance.chunkSize/2f,
			height,
			//y * WorldManager.instance.chunkSize - WorldManager.instance.chunkSize/2f
			(float)y / (float)(chunkRes-1) * chunkSize - chunkSize/2f
		);
		//Debug.Log(ToString() + result.ToString());
		return result;
	}

	public void RemoveDecorations () {
		while (decorations.Count > 0)
			WorldManager.instance.RemoveDecoration(decorations.PopFront());
	}

	public override string ToString ()
	{
		string result = "Vertex ("+x+","+y+") Height: "+height+" | nearRoad: "+nearRoad;
		//foreach (KeyValuePair<Chunk, int> member in chunkVertices) {
		//	result += "\nChunk "+member.Key.x +","+member.Key.y;
		//}
		return result;
	}
}
