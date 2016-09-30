using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class InheritColor : MonoBehaviour {

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
		if (parentGraphic.color != graphic.color)
			graphic.color = parentGraphic.color;
	}
}
