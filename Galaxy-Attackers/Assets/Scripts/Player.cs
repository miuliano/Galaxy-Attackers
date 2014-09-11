using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    
    public delegate void PlayerEventHandler(Transform player);
    public event PlayerEventHandler OnDeath;

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
    ///  Reference to the player voxel model.
    /// </summary>
    public Transform playerModel;

	/// <summary>
	/// Reference to the debris voxel model.
	/// </summary>
	public Transform debris;

	private BoxCollider boxCollider;
    private Vector3 startPosition;

    [ContextMenu("Preview")]
    void Preview()
    {
        playerModel.GetComponent<VoxelModel>().Initialize();
    }

	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(playerBounds.center, playerBounds.size);

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position + gunOffset, 1.0f);
	}

	void Start()
	{
		boxCollider = GetComponent<BoxCollider>();

		Bounds bounds = playerModel.GetComponent<VoxelModel>().GetBounds();
		boxCollider.center = bounds.center;
		boxCollider.size = bounds.size;

        startPosition = transform.position;
	}

	// Update is called once per frame
	void Update () {
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
		}

		// Transform to perspective projection
		if (Input.GetKeyUp(KeyCode.P) && Camera.main.GetComponent<ChangeProjection>().State == ChangeProjection.ProjectionState.Orthographic)
		{
			Camera.main.GetComponent<ChangeProjection>().ToPerspective(1.0f);
		}

		// Transform to orthographic projection
		if (Input.GetKeyUp(KeyCode.O) && Camera.main.GetComponent<ChangeProjection>().State == ChangeProjection.ProjectionState.Perspective)
		{
			Camera.main.GetComponent<ChangeProjection>().ToOrthographic(1.0f);
		}
	}

    public void Respawn()
    {
        transform.position = startPosition;
        playerModel.GetComponent<VoxelModel>().hidden = false;
    }

	public bool CheckCollision(Vector3 position)
	{
		Vector3 localPos = playerModel.InverseTransformPoint(position);
		return playerModel.GetComponent<VoxelModel>().GetVoxel(localPos) > 0;
	}

	public void ExplodeAt(Vector3 position, float force, float radius)
	{
		VoxelModel vm = playerModel.GetComponent<VoxelModel>();
		
		foreach (Vector3 point in vm.ToPoints())
		{
			GameObject go = Instantiate(debris.gameObject, vm.transform.TransformPoint(point), Quaternion.identity) as GameObject;
			go.rigidbody.AddExplosionForce(force, position, radius);
		}

        vm.hidden = true;

		// Trigger destroy event
        if (OnDeath != null)
		{
            OnDeath(transform);
		}
	}
}
