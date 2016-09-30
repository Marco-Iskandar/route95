using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RiffAI {

	//public RiffAI instance;
//	int minimumSimilarityValue = 0;

	//Compares the given riff to all of the cases in 
	//melodyCaseList and RhtyhmCaseList, to find the case that
	//is most similar. Rhythmic and melodic similarities are
	//calculated seperately and weighted evenly.
	//
	//"minimumSimilarityValue" is an option value that determines
	//how similar a riff has to be to be considered similar at all.
	//This is useful if none of the cases match very well, and you 
	//want to only highlight hints if you have a good hint.
	public Riff FindSimilarCase(Riff playerRiff){
		/*Dictionary<Riff, int> SimilarityDictionary = new Dictionary<Riff, int>();
		foreach(Riff caseRiff in CaseLibrary.cases) {
			if (caseRiff.instrumentIndex != playerRiff.instrumentIndex)
				continue;
			SimilarityDictionary.Add(caseRiff, 0);
			//Compare riff in each case to the given riff,
			//adding points to that case's score in the dicitonary.
			for (int i = 0; i < playerRiff.beatsShown*4; i++) {

				for (int j=0; j <playerRiff.beats[i].NumNotes(); j++) {
					try {
						//Note test_a = playerRiff.beats[i].notes[j];
						//Note test_b = caseRiff.beats[i].notes[j];
					}
					catch {
						break;
					}
					if (playerRiff.beats[i].notes[j].Equals(caseRiff.beats[i].notes[j])) {
						Debug.Log("playeRiff name " + playerRiff.name);
						//Debug.Log(playerRiff.notes[i].Count);
						SimilarityDictionary [caseRiff] += playerRiff.beats[i].NumNotes() + 1;
					}
				}
			}*/
		return null;
		//}

		//Select the case that has the highest score, indicating that it is
		//overall the most similar case.
		/*int bestScore = -1;
		Riff bestCase = null;
		//int bestRhythmScore = -1;
		//string bestRhythmCase = null;
		//find best rhythm case
		foreach (Riff key in SimilarityDictionary.Keys) {
			if (SimilarityDictionary[key] > bestScore){
				bestScore = SimilarityDictionary[key];
				bestCase = key;
			}
		}
		//if we have a good match, return it
		Debug.Log ("inside similarity, bestscore " + bestScore);
		if (bestScore > minimumSimilarityValue){
			//Debug.Log ("bestcase: " + bestCase.ToString());
			return bestCase;
		}
		//else, return null
		else{
			return null;
		}*/

	}

	//Returns the note to be highlighted.
	//If no further note should be highlighted, returns null.
	//
	//Loops backward through the player's riff, finding the furthest
	//non-empty note, since any hints will be after that note.
	//Then loops forward from that position in the closest case,
	//until it finds a non-empty note. This non-empty note is the hint
	//for the next note the player should play.
	public int FindHintXPosition(Riff playerRiff, int subdivs){
		/*int playerPosition = -1;
		Riff closestCase = FindSimilarCase(playerRiff);
		if (closestCase == null) {
			return -1;
		}
		int suggestionPosition = -1;
		Debug.Log ("inside findhintX: " );
		//int i = 16;
		//foreach(Note note in playerRiff[i]){ 		*(int)(Math.Pow(2, subdivs))
		for (int i = playerRiff.beatsShown*4 - 1; i >= 0; --i){
			if (playerRiff.beats[i].notes.Any() == true){
				playerPosition = i;
				break;
			}
		}
		Debug.Log ("inside findhintX: playerPosition = " + playerPosition);
		//Note caseNote = new Note();
		//caseNote = closestCase.notes[playerPosition][0];
		for (int i = playerPosition + 1; i < playerRiff.beatsShown*4; ++i){
			//Debug.Log ("inside findhintY, in for, outside if ");
			int q = 99;
			switch (subdivs) {
				case 2:
					q = 0;
					break;
				case 1:
					q = 1;
					break;
				case 0:
					q = 3;
					break;
			}
			Debug.Log ("q = " + q);
			if (i + q >= closestCase.beats.Count) {
				return -1;
			}
			if (closestCase.beats[i].notes.Any() == true){
				//Debug.Log ("inside findhintY, in if");
				suggestionPosition = i;
				Debug.Log ("inside findhintX, suggestedX = " + suggestionPosition);
				break;
			}
		}

		if (suggestionPosition % Math.Pow (2, 2-subdivs) != 0) {
			return -1;

		}
		return suggestionPosition;*/
	return -1;
	}

	public int FindHintYPosition (Riff playerRiff, Scale scaleDude, int subdivs) {
		/*//Debug.Log ("inside findhintY ");
		Riff closestCase = FindSimilarCase(playerRiff);
		if (closestCase == null) {
			return -1;
		}
		int suggestedPosition = FindHintXPosition (playerRiff, subdivs);
		Debug.Log ("inside findhintY, suggestedPosition = " + suggestedPosition);
		if (suggestedPosition < 0) {
			return -1;
		}
		Note caseNote = new Note();
		//caseNote = closestCase.notes[playerPosition][0];
		//Debug.Log ("inside findhintY, below Note");
		Debug.Log ("inside findhintY, closestCase is null " + (closestCase == null));
		caseNote = closestCase.beats[suggestedPosition].notes[0];// return note at a specific position still broken
		Debug.Log ("inside findhintY, casenote = " + caseNote.ToString());

		//Debug.Log ("inside findhintY scale dude " + scaleDude.allNotes.ToString());
		List<string> matchNotes = scaleDude.allNotes;
		Debug.Log ("scaleDude.allnotes count = " + matchNotes.Count);
		for (int i = 0; i < matchNotes.Count; i++) {
			//Debug.Log ("scale[" + i + "] = " + matchNotes[i]);
			Debug.Log ("caseNote.filename: " + caseNote.filename);
			Debug.Log ("matchNotes [i]: " + matchNotes [i]);
			if (matchNotes [i] == caseNote.filename) {
				//int j = i - 1;
				Debug.Log ("return findhintY: y = " + i);
				Debug.Log ("caseNote.filename: " + caseNote.filename);
				Debug.Log ("matchNotes [i]: " + matchNotes [i]);
				return i;
			}
		}*/



		return -1;
	}

}
