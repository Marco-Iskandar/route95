using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class to track application performance across its
/// various states.
/// </summary>
public class PerfTracker : MonoBehaviour {

	#region Nested Structs

	/// <summary>
	/// Struct to store performance info for a game state.
	/// </summary>
	struct Info {
		public float seconds; // Number of seconds active
		public float frames;  // Number of frames active
		public float avgFPS;  // Current average FPS

		/// <summary>
		/// Creates a PerfTracker.Info struct.
		/// </summary>
		/// <param name="startSeconds"></param>
		/// <param name="startFrames"></param>
		/// <param name="startFPS"></param>
		public Info(float startSeconds=0f, float startFrames=0f, float startFPS=0f)
		{
			seconds = startSeconds;
			frames = startFrames;
			avgFPS = startFPS;
		}

		/// <summary>
		/// String conversion.
		/// </summary>
		/// <returns>String representation of this PerfTracker.Info.</returns>
		public override string ToString()
		{
			return
				"Seconds: " + seconds.ToString("##.0000") + "\n" +
				"Frames: " + frames.ToString("##.00") + "\n" +
				"Average FPS: " + avgFPS.ToString("##.000") + "\n\n";
		}
	};

	#endregion
	#region PerfTracker Vars

	Dictionary<GameManager.State, Info> perfTracker; // Dictionary to map game states to Info structs

	#endregion
	#region UnityCallbacks

	void Awake () {

		// Init dictionary
		perfTracker = new Dictionary<GameManager.State, Info>();
	}
	
	void Update () {

		// Add new key for current game state if it doesn't exist
		if (!perfTracker.ContainsKey(GameManager.instance.currentState))
			perfTracker.Add(GameManager.instance.currentState, new Info());

		// Otherwise, update appropriate Info
		else {
			Info perf = perfTracker[GameManager.instance.currentState];
			perf.seconds += Time.deltaTime;
			perf.frames += 1;
			perf.avgFPS += ((1f/Time.deltaTime) - perf.avgFPS) / perf.frames;
		}
	}

	void OnApplicationQuit () {

		// Only save perf info in debug builds
		if (Debug.isDebugBuild) {

			// Get current time
			DateTime currTime = System.DateTime.Now;
			string log = currTime.ToString() + "\n";

			// Print info for each state
			foreach (GameManager.State state in Enum.GetValues(typeof (GameManager.State))) {
				if (perfTracker.ContainsKey(state))
					log += state.ToString() + "\n-----\n" + perfTracker[state].ToString();
			}

			// Create PerfInfo directory if possible
			try {
				Directory.CreateDirectory (Application.persistentDataPath+"/PerfLogs");
				System.IO.File.WriteAllText(PerfInfoPath(currTime), log);
			
			// If fails, print to console
			} catch (UnauthorizedAccessException) {
				Debug.Log (log);
			}
		}
	}

	#endregion
	#region PerfTracker Callbacks

	/// <summary>
	/// Returns a path for the PerfInfo file based on the
	/// current date and time.
	/// </summary>
	/// <returns></returns>
	string PerfInfoPath (DateTime time) {
		return
			Application.persistentDataPath +
			"/PerfLogs/PerfLog_" +
			time.Year.ToString() +
			time.Month.ToString() +
			time.Day.ToString() + "_" +
			time.Hour.ToString() +
			time.Minute.ToString() +
			time.Second.ToString() +
			".txt";
	}

	#endregion

}
