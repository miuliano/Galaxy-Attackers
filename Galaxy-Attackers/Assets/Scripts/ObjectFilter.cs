using UnityEngine;
using System.Collections;

public class ObjectFilter : MonoBehaviour {

	public string[] tagFilter;

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
	}

	void OnTriggerEnter(Collider other)
	{
		foreach (string tag in tagFilter)
		{
			if (other.tag == tag)
			{
				Destroy(other.gameObject);
				return;
			}
		}
	}
}
