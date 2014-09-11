using UnityEngine;
using System.Collections;

public class ObjectFilter : MonoBehaviour {

	public string[] tagFilter;

    void Start()
    {
        Vector3 screenPoint = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)) - Camera.main.transform.position;
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        boxCollider.size = new Vector3(screenPoint.x * 2.0f, boxCollider.size.y, boxCollider.size.z);
    }

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
