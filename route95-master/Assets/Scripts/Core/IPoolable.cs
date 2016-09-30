using UnityEngine;
using System.Collections;

public interface IPoolable {

	void OnPool ();
	void OnDepool ();

}