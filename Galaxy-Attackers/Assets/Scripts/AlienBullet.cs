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

	/// <summary>
	/// Reference to the debris voxel model.
	/// </summary>
	public Transform debris;
	
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
		if (other.tag == "Player")
		{
            // Potential player hit
			Player player = other.GetComponent<Player>();
			
			Vector3 hitPoint = transform.position + hitOffset;
						
			if (player.CheckCollision(hitPoint))
			{
                // Definite hit
				player.ExplodeAt(hitPoint, explosionForce, explosionRadius);				
				
				// Kill thyself
				Destroy(gameObject);
			}
		}
		else if (other.tag == "Building")
		{
            // Potential building hit
			Building building = other.GetComponent<Building>();
			
			Vector3 hitPoint = transform.position + hitOffset;
						
			if (building.CheckCollision(hitPoint))
			{
                // Definite hit
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

	/// <summary>
	/// Check for collision between a point and the bullet.
	/// </summary>
	/// <param name="position">Point in world coordinates.</param>
	/// <returns>True on collision, false otherwise.</returns>
	public bool CheckCollision(Vector3 position)
	{
		Vector3 localPos = bulletFrames[frameIndex].InverseTransformPoint(position);
		return bulletFrames[frameIndex].GetComponent<VoxelModel>().GetVoxel(localPos) > 0;
	}

	/// <summary>
	/// Explodes the bullet into debris by a force at a given location.
	/// </summary>
	/// <param name="position">Location of the explosion force.</param>
	/// <param name="force">Magnitude of the explosion force.</param>
	/// <param name="radius">Radius of the explosion force.</param>
	public void ExplodeAt(Vector3 position, float force, float radius)
	{
		VoxelModel vm = bulletFrames[frameIndex].GetComponent<VoxelModel>();
		
		foreach (Vector3 point in vm.ToPoints())
		{
			GameObject go = Instantiate(debris.gameObject, vm.transform.TransformPoint(point), Quaternion.identity) as GameObject;
			go.rigidbody.AddExplosionForce(force, position, radius);
		}
		
		Destroy(gameObject);
	}
}
