using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Non-MonoBehaviour class to handle IO.
/// </summary>
public static class SaveLoad {

	#region Exceptions

	/// <summary>
	/// Exception thrown whenever a file fails to save.
	/// </summary>
	[Serializable]
	public class FailedToSaveException : ApplicationException {
		public FailedToSaveException(string info, Exception standard) : base(info, standard) { }
		public FailedToSaveException(string info) : base(info) { }
		public FailedToSaveException() { }
	}

	/// <summary>
	/// Exception thrown whenever a file fails to load.
	/// </summary>
	[Serializable]
	public class FailedToLoadException : ApplicationException {
		public FailedToLoadException(string info, Exception standard) : base(info, standard) { }
		public FailedToLoadException(string info) : base(info) { }
		public FailedToLoadException() { }
	}

	#endregion
	#region SaveLoad Vars

	public static string projectSaveExtension = ".r95p"; // File extension for projects
	public static string songSaveExtension = ".r95s";    // File extension for songs

	#endregion
	#region Save Methods

	/// <summary>
	/// Saves an item to a particular path with a certain name.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="toSave">Object to save (must be serializable).</param>
	/// <param name="dirPath">Full path to save directory.</param>
	/// <param name="filePath">Full save path, including file name, extension, and directory.</param>
	public static void Save<T> (T toSave, string dirPath, string filePath) where T : new() {

		// Check if type is serializable
		if (!typeof(T).IsSerializable)
			throw new FailedToSaveException("SaveLoad.Save(): type " + toSave.GetType().ToString() + " is not serializable!");

		// Init BinaryFormatter
		BinaryFormatter bf = new BinaryFormatter();

		// Create directory
		Directory.CreateDirectory(dirPath);

		// Open file
		FileStream file = File.Open(filePath, FileMode.Create);

		// Serialize object
		bf.Serialize(file, toSave);

		// Close file
		file.Close();
	}

	/// <summary>
	/// Saves the current open project.
	/// </summary>
	public static void SaveCurrentProject () {

		// Build paths
		string directoryPath = GameManager.instance.projectSavePath;
		string filePath = directoryPath + MusicManager.instance.currentProject.name + projectSaveExtension;

		try {

			// Save project
			Save<Project>(MusicManager.instance.currentProject, directoryPath, filePath);

			// Prompt
			Prompt.instance.PromptMessage("Save Project", "Successfully saved project to \"" + filePath + "\".", "Okay");

		// Catch exceptions
		} catch (FailedToSaveException e) {
			Debug.LogError(e.Message);
			return;
		}
	}

	/// <summary>
	/// Saves the indicated song.
	/// </summary>
	/// <param name="song">Song to save.</param>
	public static void SaveSong(Song song) {
	
		// Build paths
		string directoryPath = Application.dataPath + "/Songs/";
		string filePath = directoryPath + song.name + songSaveExtension;

		// Save song
		Save<Song>(song, directoryPath, filePath);
		
		// Prompt
		Prompt.instance.PromptMessage("Save Project", "Successfully saved Song!", "Okay");
	}

	/// <summary>
	/// Saves the currently open song.
	/// </summary>
	public static void SaveCurrentSong() {
		Song song = MusicManager.instance.currentSong;

		// Check if song is null
		if (song == null) {
			Debug.LogError("SaveLoad.SaveCurrentSong(): tried to save null song!");
			return;
		}

		// Save song
		SaveSong(song);
	}

	#endregion
	#region Load Methods

	/// <summary>
	/// Loads the specified object.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="dirPath">Full path to load directory.</param>
	/// <param name="filePath">Full load path, including file name, extension, and directory.</param>
	public static T Load<T> (string filePath) where T : new() {

		// Check if file exists
		if (!File.Exists(filePath))
			throw new FailedToLoadException("SaveLoad.Load(): invalid path \"" + filePath + "\"");

		try {

			Debug.Log("SaveLoad.Load<" + typeof(T).ToString() + ">(): loading path \"" + filePath + "\".");

			// Init BinaryFormatter
			BinaryFormatter bf = new BinaryFormatter();

			// Open file
			FileStream file = File.Open(filePath, FileMode.Open);

			// Deserialize object
			T result = (T)bf.Deserialize(file);

			// Check if valid result
			if (result == null) throw new FailedToLoadException (
				"SaveLoad.Load(): loaded file \"" + filePath + "\" was null.\n"
				);

			// Close file
			file.Close();

			// Return object
			return result;

		// Catch SerializationExceptions
		} catch (SerializationException e) {
			throw new FailedToLoadException(
				"SaveLoad.Load(): failed to deserialize file \"" + filePath + "\".\n" + e.Message
			);

		// Catch EndOfStreamExceptions
		} catch (EndOfStreamException e) {
			throw new FailedToLoadException(
				"SaveLoad.Load(): hit unexpected end of stream in file \"" + filePath + "\".\n" + e.Message
			);
		}
	}

	/// <summary>
	/// Loads the project at the specified path.
	/// </summary>
	/// <param name="path">Project path.</param>
	public static void LoadProject (string path) {

		// Backup project and song
		Project backupProject = MusicManager.instance.currentProject;
		Song backupSong = MusicManager.instance.currentSong;

		try {

			// Load project
			Project project = Load<Project>(path);
			MusicManager.instance.currentProject = project;

			// Load first song, if available
			if (!project.Empty)
				MusicManager.instance.currentSong = project.songs[0];

		// Catch and print any FailedToLoadExceptions
		} catch (FailedToLoadException f) {

			// Restore backups
			MusicManager.instance.currentProject = backupProject;
			MusicManager.instance.currentSong = backupSong;

			throw f;
		}
	}

	/// <summary>
	/// Loads the specified song to the project.
	/// </summary>
	/// <param name="path">Song path.</param>
	public static void LoadSongToProject (string path) {

		// Load song
		Song song = LoadSong(path);

		// Add song to project
		MusicManager.instance.currentProject.AddSong(song);

		// Set current song to song
		MusicManager.instance.currentSong = song;
	}

	/// <summary>
	/// Loads a song.
	/// </summary>
	/// <param name="path"></param>
	/// <returns>Song path.</returns>
	public static Song LoadSong (string path) {

		try {

			// Load song
			Song result = Load<Song>(path);

			//Prompt.instance.PromptMessage("Load Song", "Successfully loaded Song!", "Okay");

			return result;

		// Catch and print FailedToLoadExceptions
		} catch (FailedToLoadException e) {

			// Print exception
			Debug.LogError(e.Message);

			// Prompt
			Prompt.instance.PromptMessage("Failed to load song", 
				"Failed to load song \"" + path + "\". It may be in an older format or corrupt.", 
				"Okay");

			return null;
		}
	}

	#endregion

}
