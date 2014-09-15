using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    
    public delegate void PlayerEventHandler(Player player);

	/// <summary>
	/// Occurs when then player dies.
	/// </summary>
    public event PlayerEventHandler OnDeath;

	/// <summary>
	/// Occurs when the player shoots.
	/// </summary>
	public event PlayerEventHandler OnShoot;

    /// <summary>
    /// Horziontal speed of the player.
    /// </summary>
    public float playerSpeed = 10.0f;

    /// <summary>
    /// Bounding box constraining player movement.
    /// </summary>
	public Bounds playerBounds = new Bounds(Vector3.zero, Vector3.one);

    /// <summary>
    /// Offset from which bullets are fired from.
    /// </summary>
	public Vector3 gunOffset = Vector3.zero;

    /// <summary>
    /// Reference to the bullet prefab.
    /// </summary>
    public Transform bullet;

	/// <summary>
	/// Reference to the debris voxel model.
	/// </summary>
	public Transform debris;

	/// <summary>
	/// Gets a value indicating whether this <see cref="Player"/> is alive.
	/// </summary>
	/// <value><c>true</c> if alive; otherwise, <c>false</c>.</value>
	public bool Alive
	{
		get
		{
			return isAlive;
		}
	}

	private bool isAlive;

	private BoxCollider boxCollider;

	private VoxelModel voxelModel;

    private Vector3 startPosition;

	void Start()
	{
		isAlive = true;
		startPosition = transform.position;

		boxCollider = GetComponent<BoxCollider>();
		voxelModel = GetComponent<VoxelModel>();

		if (voxelModel.Loaded != true)
		{
			voxelModel.OnLoad += LoadBounds;
		}
		else
		{
			LoadBounds(voxelModel);
		}
	}

	/// <summary>
	/// Sets the box collider bounds to that of the voxel model.
	/// </summary>
	/// <param name="model">Model.</param>
	void LoadBounds(VoxelModel model)
	{
		Bounds bounds = model.GetLocalBounds();
		boxCollider.center = bounds.center;
		boxCollider.size = bounds.size;
	}

	// Update is called once per frame
	void Update () {
		if (isAlive == false) return;

        float hMove = Input.GetAxis("Horizontal");
        float vMove = Input.GetAxis("Vertical");
		bool fire1 = Input.GetButtonDown("Fire1");

		// Movement left and right
        if (hMove > 0 || hMove < 0)
        {
			Vector3 newPosition = transform.position + Vector3.right * hMove * Time.deltaTime * playerSpeed;

			// Keep in bounds
			if (playerBounds.Contains(newPosition))
			{
				transform.position = newPosition;
			}
        }

		// Shoot button
		if (fire1 == true && GameObject.FindGameObjectsWithTag("PlayerBullet").Length == 0)
		{
			Instantiate(bullet.gameObject, transform.position + gunOffset, Quaternion.identity);

			if (OnShoot != null)
			{
				OnShoot(this);
			}
		}
	}

    public void Respawn()
    {
        transform.position = startPosition;
        voxelModel.Hidden = false;
		isAlive = true;
    }

	public bool CheckCollision(Vector3 position)
	{
		if (isAlive == false) return false;

		Vector3 localPos = voxelModel.transform.InverseTransformPoint(position);
		return voxelModel.GetVoxel(localPos) > 0;
	}

	public void ExplodeAt(Vector3 position, float force, float radius)
	{
		foreach (Vector3 point in voxelModel.ToLocalPoints())
		{
			GameObject go = Instantiate(debris.gameObject, voxelModel.transform.TransformPoint(point), Quaternion.identity) as GameObject;
			go.rigidbody.AddExplosionForce(force, position, radius);
		}

		voxelModel.Hidden = true;

		isAlive = false;

		// Trigger destroy event
        if (OnDeath != null)
		{
            OnDeath(this);
		}
	}

	void OnTriggerStay(Collider other)
	{
		CollisionHandler(other);
	}

	void OnTriggerEnter(Collider other)
	{
		CollisionHandler(other);
	}

	/// <summary>
	/// Handle collisions with the player.
	/// </summary>
	/// <param name="other">Other.</param>
	void CollisionHandler(Collider other)
	{
		if (isAlive == false) return;

		if (other.tag == "Alien")
		{
			Alien alien = other.GetComponent<Alien>();
			
			IntVector2[] intersections = VoxelModel.Intersect(voxelModel, alien.VoxelModel);

			// Game over!
			if (intersections.Length > 0)
			{
				ExplodeAt(voxelModel.VoxelToWorldSpace(intersections[0]), 1000.0f, 1.0f);
			}
		}
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(playerBounds.center, playerBounds.size);

		Gizmos.DrawIcon(transform.position, "player_icon.png");
		
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position + gunOffset, 1.0f);
	}
}
