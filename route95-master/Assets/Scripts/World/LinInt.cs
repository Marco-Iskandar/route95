using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A class that can linearly interpolate between key points of data
/// </summary>
public class LinInt {

	#region LinInt Vars

	List<float> keyPoints; // All key points (from frequency data)
	float averagedEnds;    // Averaged endpoints

	#endregion
	#region LinInt Methods

	/// <summary>
	/// Default constructor.
	/// </summary>
	public LinInt () {
		keyPoints = new List<float> ();
		averagedEnds = 0f;
	}

	/// <summary>
	/// Updates LinInt using frequency data.
	/// </summary>
	/// <param name="data"></param>
	public void Update(float[] data) {
		keyPoints.Clear();
		foreach (float f in data) keyPoints.Add (f);
		averagedEnds = (keyPoints [0] + keyPoints [keyPoints.Count - 1]) / 2;
	}

	/*takes in a float from 0-1, inclusive
	 *returns the data value that matches
	 *the linearly interpreted key points
	 */
	 /// <summary>
	 /// Takes in a float from 0-1, inclusive,
	 /// and returns the data value that matches
	 /// the linearly interpreted key points.
	 /// </summary>
	 /// <param name="pos"></param>
	 /// <returns></returns>
	public float GetDataPoint (float pos){
		//if input is out of bounds
		if (pos > 1f) {
			Debug.Log ("Position given to LinInt (" + pos + ") is too large.");
			pos = 1f;
			Debug.Log ("LinInt accepts values between 0 and 1, inclusive.");
		} else if (pos < 0f) {
			Debug.Log ("Position given to LinInt (" + pos + ") is too negative.");
			pos = 0f;
			Debug.Log ("LinInt accepts values between 0 and 1, inclusive.");
		}

		//if there's only one key point, just return it
		if (keyPoints.Count == 1) {
			return keyPoints [0];
		}
		float data = 0f;
		int bucket = 0;
		float bucketWidth = 1f / (keyPoints.Count - 1);
		//float step = 0f;

		//get appropriate bucket
		bucket = (int)Mathf.Floor(pos/bucketWidth);
		if (bucket == (keyPoints.Count - 1)) {
			bucket--;
		}
		float startData = keyPoints [bucket];
		if (bucket == 0) { //if first bucket, set startData to averaged data
			startData = averagedEnds;
		}
		float endData = keyPoints [bucket + 1];
		if (bucket == (keyPoints.Count - 2)) { //if last bucket, set endData to averaged data
			endData = averagedEnds;
		}
		float bucketPos = pos % bucketWidth;
		float DataDifference = endData - startData;
		data = startData + bucketPos * DataDifference;
		return data;
	}

	#endregion
}
