using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Data storage class to handle poolable
/// GameObjects.
/// </summary>
public class ObjectPool<T> where T: MonoBehaviour, IPoolable {

	List<T> pool; // List of inactive GameObjects

	/// <summary>
	/// Init this instance.
	/// </summary>
	public ObjectPool () {
		pool = new List<T>();
	}

	/// <summary>
	/// Adds an item to the pool, and deactivates it.
	/// </summary>
	/// <param name="item">Item to add to the pool.</param>
	public void Add (T item) {
		pool.Add(item);
		item.OnPool();
	}

	/// <summary>
	/// Returns a reference to the object at the top
	/// of the pool.
	/// </summary>
	/// <returns>Reference to the top GameObject.</returns>
	public T Peek () {
		if (Empty) return default(T);
		return pool[0];
	}

	/// <summary>
	/// Removes and returns an item from the pool
	/// and activates it.
	/// </summary>
	/// <returns>GameObject at the top of the pool.</returns>
	public T Get () {

		// Return null if empty
		if (Empty) return default(T);

		T result = pool[0];
		pool.RemoveAt(0);
		result.OnDepool();
		return result;
	}

	/// <summary>
	/// Returns whether or not the pool is empty.
	/// </summary>
	public bool Empty {
		get {
			return pool.Count == 0;
		}
	}

}
