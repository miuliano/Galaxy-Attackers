using UnityEngine;
using System.Collections;

public class OrbitTarget : MonoBehaviour {

    public Transform target;
    public float rotationSpeed = 60.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.RotateAround(target.position, Vector3.up, rotationSpeed * Time.deltaTime);
	}
}
