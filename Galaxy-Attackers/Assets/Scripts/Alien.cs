using UnityEngine;
using System.Collections;

[RequireComponent( typeof(VoxelAnimation) )]
public class Alien : MonoBehaviour {

	public delegate void AlienEventHandler(Alien alien);

	/// <summary>
	/// The points scored for killing this alien.
	/// </summary>
	public int pointValue;

    /// <summary>
    /// Reference to the debris voxel model.
    /// </summary>
    public Transform debris;

	/// <summary>
	/// Gets a value indicating whether this <see cref="Alien"/> is alive.
	/// </summary>
	/// <value><c>true</c> if alive; otherwise, <c>false</c>.</value>
	public bool Alive
	{
		get
		{
			return isAlive;
		}
	}

	/// <summary>
	/// Occurs when the alien is destroyed.
	/// </summary>
	public event AlienEventHandler OnDestroy;

	private VoxelAnimation voxelAnimation;

    private BoxCollider boxCollider;

	private bool isAlive;

	// Use this for initialization
	void Start () {
		boxCollider = GetComponent<BoxCollider>();

		voxelAnimation = GetComponent<VoxelAnimation>();
		voxelAnimation.OnFrameChange += voxelAnimation_OnFrameChange;

		// If the first frame hasn't loaded yet, listen for it
		if (voxelAnimation.CurrentFrame.loaded == false)
		{
			voxelAnimation.CurrentFrame.OnLoad += voxelModel_OnLoad;
		}
		else
		{
			Bounds bounds = voxelAnimation.CurrentFrame.GetBounds();
			boxCollider.center = bounds.center;
			boxCollider.size = bounds.size;
		}

		isAlive = true;
	}

	// Update bounds once the first frame has loaded
	void voxelModel_OnLoad(VoxelModel model)
	{
		Bounds bounds = model.GetBounds();
		boxCollider.center = bounds.center;
		boxCollider.size = bounds.size;
	}

	// Update bounds on frame change
	void voxelAnimation_OnFrameChange(VoxelAnimation animation)
	{
		Bounds bounds = animation.CurrentFrame.GetBounds();
		boxCollider.center = bounds.center;
		boxCollider.size = bounds.size;
	}

    /// <summary>
    /// Check for collision between a point and the alien.
    /// </summary>
    /// <param name="position">Point in world coordinates.</param>
    /// <returns>True on collision, false otherwise.</returns>
    public bool CheckCollision(Vector3 position)
    {
		if (isAlive == false) return false;

	    Vector3 localPos = voxelAnimation.CurrentFrame.transform.InverseTransformPoint(position);
		return voxelAnimation.CurrentFrame.GetVoxel(localPos) > 0;
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
		// Create explosion
		VoxelModel vm = voxelAnimation.CurrentFrame;

        foreach (Vector3 point in vm.ToPoints())
        {
            GameObject go = Instantiate(debris.gameObject, vm.transform.TransformPoint(point), Quaternion.identity) as GameObject;
            go.rigidbody.AddExplosionForce(force, position, radius);
        }

		// Hide frames
		voxelAnimation.Hidden = true;

		// Turn off collisions
		boxCollider.enabled = false;

		isAlive = false;

        // Trigger destroy event
        if (OnDestroy != null)
        {
            OnDestroy(this);
        }
    }
}
