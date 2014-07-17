using UnityEngine;
using System.Collections;

public class SwapPrefab : MonoBehaviour {

    public GameObject swapTo;
    public KeyCode swapOnKey;
    public float explosionRadius = 5.0f;
    public float explosionPower = 10.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetKey(swapOnKey))
        {
            GameObject swap = (GameObject) Instantiate(swapTo, this.transform.position, this.transform.rotation);

            foreach (Transform child in swap.transform)
            {
                child.rigidbody.AddExplosionForce(explosionPower, transform.position, explosionRadius);
            }

            Destroy(gameObject); // Wreck yo-self
        }
	}
}
