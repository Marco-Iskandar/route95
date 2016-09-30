using UnityEngine;
using System.Collections;

public class ShootingStar : MonoBehaviour {

	const float MAX_V = 100f;

	public Gradient alpha;
	SpriteRenderer rend;
	public float minLifetime;
	public float maxLifetime;
	float lifetime;
	float life;

	Vector3 v;

	void Start () {
		v = new Vector3 (Random.Range(-MAX_V,MAX_V), Random.Range(-MAX_V,MAX_V), Random.Range(-MAX_V,MAX_V));
		rend = GetComponent<SpriteRenderer>();
		life = 0f;
		lifetime = Random.Range(minLifetime, maxLifetime);
	}

	void Update () {
		life += Time.deltaTime;
		if (life >= 1f) Destroy (gameObject);
		else {
			transform.position += v * Time.deltaTime;
			transform.LookAt (Camera.main.transform);
			v.y += Physics.gravity.y * Time.deltaTime;

			Color color = rend.color;
			color.a = alpha.Evaluate(life/lifetime).a;
			rend.color = color;
		}

	}
}
