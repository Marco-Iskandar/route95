using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class to store riffs.
/// </summary>
[System.Serializable]
public class Measure {

	[SerializeField]
	public int index;             // Project-specific index

	[SerializeField]
	public List<int> riffIndices; // List of indices of riffs used

	/// <summary>
	/// Default constructor.
	/// </summary>
	public Measure () {
		riffIndices = new List<int>();
	}
}
