using UnityEngine;
using System.Collections;

public class Ground : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Vector3 screenPoint = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)) - Camera.main.transform.position;
        transform.localScale = new Vector3(screenPoint.x * 2.0f, transform.localScale.y, transform.localScale.z);
	}
}
