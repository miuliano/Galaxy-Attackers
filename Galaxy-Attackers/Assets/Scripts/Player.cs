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

	private BoxCollider boxCollider;

	private VoxelModel voxelModel;

    private Vector3 startPosition;

	void Start()
	{
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
		Bounds bounds = model.GetBounds();
		boxCollider.center = bounds.center;
		boxCollider.size = bounds.size;
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

			if (OnShoot != null)
			{
				OnShoot(this);
			}
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
        voxelModel.Hidden = false;
    }

	public bool CheckCollision(Vector3 position)
	{
		Vector3 localPos = voxelModel.transform.InverseTransformPoint(position);
		return voxelModel.GetVoxel(localPos) > 0;
	}

	public void ExplodeAt(Vector3 position, float force, float radius)
	{
		foreach (Vector3 point in voxelModel.ToPoints())
		{
			GameObject go = Instantiate(debris.gameObject, voxelModel.transform.TransformPoint(point), Quaternion.identity) as GameObject;
			go.rigidbody.AddExplosionForce(force, position, radius);
		}

		voxelModel.Hidden = true;

		// Trigger destroy event
        if (OnDeath != null)
		{
            OnDeath(this);
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
