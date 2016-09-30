using System;
using System.Collections.Generic;

/// <summary>
/// Class to add extension functions to lists.
/// </summary>
public static class ListExtension {

	/// <summary>
	/// Returns the first element of a list.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="list"></param>
	/// <returns></returns>
	public static T Head<T> (this List<T> list) {
		return list[0];
	}

	/// <summary>
	/// Returns the final element of a list.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="list"></param>
	/// <returns></returns>
	public static T Tail<T> (this List<T> list) {
		return list[list.Count-1];
	}

	/// <summary>
	/// Inserts an element into a sorted list.
	/// </summary>
	/// <typeparam name="T">Must implement IComparable.</typeparam>
	/// <param name="list"></param>
	/// <param name="toInsert">Element to insert.</param>
	/// <param name="highestFirst">Is the list greatest to least?</param>
	public static void InsertSorted<T> (this List<T> list, T toInsert, bool highestFirst) where T : IComparable {
		if (list.Count == 0) list.Add (toInsert);
		else {
			for (int i = 0; i < list.Count; i++) {
				if (highestFirst && toInsert.CompareTo (list[i]) > 0) {
					list.Insert (i, toInsert);
					return;
				} else if (!highestFirst && toInsert.CompareTo (list[i]) < 0) {
					list.Insert (i, toInsert);
					return;
				}
			}
			list.Add (toInsert);
		}
	}

	public static T PopFront<T> (this List<T> list) {
		T result = list[0];
		list.RemoveAt(0);
		return result;
	}

	public static T Random<T> (this List<T> list) {
		return list[UnityEngine.Random.Range(0, list.Count)];
	}
	
}

