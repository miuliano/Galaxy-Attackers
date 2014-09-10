using UnityEngine;
using System.Collections;

public class Alien : MonoBehaviour {

	/// <summary>
	/// List of children comprising the frames of this animation.
	/// </summary>
	public Transform[] frames;

	/// <summary>
	/// Time delay between voxel animation frames.
	/// </summary>
	public float animationDelay = 0.5f;
	
    /// <summary>
    /// Reference to the debris voxel model.
    /// </summary>
    public Transform debris;

	/// <summary>
	/// Gets a value indicating whether this <see cref="Alien"/> is alive.
	/// </summary>
	/// <value><c>true</c> if alive; otherwise, <c>false</c>.</value>
	public bool alive
	{
		get 
		{
			return isAlive;
		}
	}

	private bool isAlive = true;

	private int frameIndex = 0;
	private float nextFrame = 0.0f;

    private BoxCollider boxCollider;

    // Event handlers
    public delegate void AlienEventHandler(Transform alien);
    public event AlienEventHandler OnDestroy;

    [ContextMenu("Preview")]
    void Preview ()
    {
        frames[0].GetComponent<VoxelModel>().Initialize();
    }

	// Use this for initialization
	void Start () {
		frameIndex = 0;
		nextFrame = 0.0f;

		boxCollider = GetComponent<BoxCollider>();

		Bounds bounds = frames[0].GetComponent<VoxelModel>().GetBounds();
		boxCollider.center = bounds.center;
		boxCollider.size = bounds.size;
	}
	
	void Update()
	{
		float frameTime = Time.time;

		if (frameTime > nextFrame && isAlive)
		{
			frames[frameIndex].gameObject.SetActive(true);
			frames[(frameIndex - 1 >= 0 ? frameIndex - 1 : frames.Length - 1)].gameObject.SetActive(false); 

			Bounds bounds = frames[frameIndex].GetComponent<VoxelModel>().GetBounds();
			boxCollider.center = bounds.center;
			boxCollider.size = bounds.size;

			frameIndex = (frameIndex + 1 <= frames.Length - 1 ? frameIndex + 1 : 0);

			nextFrame = frameTime + animationDelay;
		}
	}

    /// <summary>
    /// Check for collision between a point and the alien.
    /// </summary>
    /// <param name="position">Point in world coordinates.</param>
    /// <returns>True on collision, false otherwise.</returns>
    public bool CheckCollision(Vector3 position)
    {
		if (isAlive == false) return false;

	    Vector3 localPos = frames[frameIndex].InverseTransformPoint(position);
		return frames[frameIndex].GetComponent<VoxelModel>().GetVoxel(localPos) > 0;
    }

	/// <summary>
	/// Returns the bounds of the alien.
	/// </summary>
	/// <returns>The alien's bounds.</returns>
	public Bounds GetBounds()
	{
		return boxCollider.bounds;
	}

    /// <summary>
    /// Explodes the alien into debris by a force at a given location.
    /// </summary>
    /// <param name="position">Location of the explosion force.</param>
    /// <param name="force">Magnitude of the explosion force.</param>
    /// <param name="radius">Radius of the explosion force.</param>
    public void ExplodeAt(Vector3 position, float force, float radius)
    {
		VoxelModel vm = frames[frameIndex].GetComponent<VoxelModel>();

        foreach (Vector3 point in vm.ToPoints())
        {
            GameObject go = Instantiate(debris.gameObject, vm.transform.TransformPoint(point), Quaternion.identity) as GameObject;
            go.rigidbody.AddExplosionForce(force, position, radius);
        }

		isAlive = false;

		// Hide frames
		foreach (Transform frame in frames)
		{
			frame.GetComponent<VoxelModel>().hidden = true;
		}

		// Turn off collisions
		boxCollider.enabled = false;

        // Trigger destroy event
        if (OnDestroy != null)
        {
            OnDestroy(transform);
        }
    }
}
