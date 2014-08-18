using UnityEngine;
using System.Collections;

public class RemoveBullets : MonoBehaviour {

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Bullet")
		{
			Destroy(other.gameObject);
		}
	}
}
