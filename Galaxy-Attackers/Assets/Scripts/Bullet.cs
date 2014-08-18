using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	public Vector3 velocity = Vector3.zero;
	public Vector3 hitOffset = Vector3.zero;

	private GameObject explosionManager;

	// Use this for initialization
	void Start () {
		explosionManager = GameObject.Find("Explosion Manager");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position + hitOffset, 1.0f);
	}

	void FixedUpdate()
	{
		// Move at a fixed velocity
		transform.position += velocity * Time.fixedDeltaTime;
	}

	void Explode(Collider other)
	{
		// Hit enemy
		if (other.tag == "Enemy")
		{
			VoxelModel enemyModel = other.GetComponent<VoxelModel>();

			Vector3 hitPoint = transform.position + hitOffset;
			Vector3 localHitPoint = other.transform.InverseTransformPoint(hitPoint);

			// Collision check
			if (enemyModel.GetVoxel(localHitPoint) > 0)
			{
				// Explode
				explosionManager.GetComponent<ExplosionManager>().ExplodeAt(hitPoint, other.gameObject);

				// Kill thyself
				Destroy(gameObject);
			}
		}
	}

	void OnTriggerEnter(Collider other)
	{
		Explode(other);
	}

	void OnTriggerStay(Collider other)
	{
		Explode(other);
	}
}
