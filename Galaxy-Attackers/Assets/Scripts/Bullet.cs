using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	public Vector3 velocity = Vector3.zero;
	public Vector3 hitOffset = Vector3.zero;
    public float explosionForce = 10000.0f;
    public float explosionRadius = 20.0f;
    public Transform bulletModel;

    private BoxCollider boxCollider;

    [ContextMenu("Preview")]
    void Preview()
    {
        bulletModel.GetComponent<VoxelModel>().Initialize();
    }

	// Use this for initialization
	void Start () {
        boxCollider = GetComponent<BoxCollider>();

        Bounds bounds = bulletModel.GetComponent<VoxelModel>().GetBounds();
        boxCollider.center = bounds.center;
        boxCollider.size = bounds.size;
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
			Alien enemy = other.GetComponent<Alien>();

			Vector3 hitPoint = transform.position + hitOffset;

			// Collision check
            if (enemy.CheckCollision(hitPoint))
			{
                enemy.ExplodeAt(hitPoint, explosionForce, explosionRadius);				

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
