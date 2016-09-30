using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class InheritAlpha : MonoBehaviour {

	RectTransform parent;
	MaskableGraphic parentGraphic;
	MaskableGraphic graphic;

	void Awake () {
		if (parent == null)
			parent = gameObject.RectTransform().parent as RectTransform;

		graphic = GetComponent<MaskableGraphic>();
		if (graphic == null)
			throw new ArgumentException("No MaskableGraphic found on this object!");

		parentGraphic = parent.GetComponent<MaskableGraphic>();
		if (parentGraphic == null)
			throw new ArgumentException ("No MaskableGraphic found on parent!");
	}

	void Update () {
		if (parentGraphic.color.a != graphic.color.a)
			graphic.SetAlpha(parentGraphic.color.a);
	}
}
