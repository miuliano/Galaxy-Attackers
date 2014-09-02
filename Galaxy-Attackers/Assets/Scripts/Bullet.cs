using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

    /// <summary>
    /// The velocity of the bullet.
    /// </summary>
	public Vector3 velocity = Vector3.zero;
    
    /// <summary>
    /// Collision point offset.
    /// </summary>
	public Vector3 hitOffset = Vector3.zero;

    /// <summary>
    /// Force of the bullet collision explosion.
    /// </summary>
    public float explosionForce = 10000.0f;

    /// <summary>
    /// Radius of the bullet collision explosion.
    /// </summary>
    public float explosionRadius = 20.0f;

    /// <summary>
    /// Reference to the bullet voxel model.
    /// </summary>
    public Transform bulletModel;

    private BoxCollider boxCollider;

    [ContextMenu("Preview")]
    void Preview()
    {
        bulletModel.GetComponent<VoxelModel>().Initialize();
    }

	// Use this for initialization
	void Start () {
        // Update the box collider bounds
        boxCollider = GetComponent<BoxCollider>();

        Bounds bounds = bulletModel.GetComponent<VoxelModel>().GetBounds();
        boxCollider.center = bounds.center;
        boxCollider.size = bounds.size;
	}

    /// <summary>
    /// Draws the bullet's collision point gizmo.
    /// </summary>
	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position + hitOffset, 1.0f);
	}

	void Update()
	{
		// Move at a fixed velocity
		transform.position += velocity * Time.deltaTime;
	}

    /// <summary>
    /// Bullet collision handler.
    /// </summary>
    /// <param name="other">Collider colliding with.</param>
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
        // Hit building
        else if (other.tag == "Building")
        {
            Building building = other.GetComponent<Building>();

            Vector3 hitPoint = transform.position + hitOffset;

            // Collision check
            if (building.CheckCollision(hitPoint))
            {
                building.ExplodeAt(hitPoint, explosionForce, explosionRadius);

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
