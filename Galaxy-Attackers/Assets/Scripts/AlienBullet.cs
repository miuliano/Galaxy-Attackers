using UnityEngine;
using System.Collections;

public class AlienBullet : MonoBehaviour {
	
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
	/// Reference to the bullet's frames' models.
	/// </summary>
	public Transform[] bulletFrames;

	/// <summary>
	/// The animation delay.
	/// </summary>
	public float animationDelay = 0.5f;
	
	private int frameIndex = 0;
	private float nextFrame = 0.0f;

	private BoxCollider boxCollider;
	
	[ContextMenu("Preview")]
	void Preview()
	{
		bulletFrames[0].GetComponent<VoxelModel>().Initialize();
	}
	
	// Use this for initialization
	void Start () {
		frameIndex = 0;
		nextFrame = 0.0f;

		// Update the box collider bounds
		boxCollider = GetComponent<BoxCollider>();
		
		Bounds bounds = bulletFrames[0].GetComponent<VoxelModel>().GetBounds();
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
		float frameTime = Time.time;
		
		if (frameTime > nextFrame)
		{
			for (int i = 0; i < bulletFrames.Length; i++)
			{
				if (i == frameIndex)
				{
					bulletFrames[i].gameObject.SetActive(true);

					Bounds bounds = bulletFrames[i].GetComponent<VoxelModel>().GetBounds();
					boxCollider.center = bounds.center;
					boxCollider.size = bounds.size;
				}
				else
				{
					bulletFrames[i].gameObject.SetActive(false);
				}
			}

			frameIndex++;

			if (frameIndex >= bulletFrames.Length)
			{
				frameIndex = 0;
			}

			nextFrame = frameTime + animationDelay;
		}

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
		if (other.tag == "Player")
		{
			Debug.Log("Player potential hit");

			Player player = other.GetComponent<Player>();
			
			Vector3 hitPoint = transform.position + hitOffset;
			
			// Collision check
			if (player.CheckCollision(hitPoint))
			{
				Debug.Log("Player definite hit");

				player.ExplodeAt(hitPoint, explosionForce, explosionRadius);				
				
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
