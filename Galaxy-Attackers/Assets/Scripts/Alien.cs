using UnityEngine;
using System.Collections;

public class Alien : MonoBehaviour {
	
    /// <summary>
    /// Reference to the first frame of the voxel animation.
    /// </summary>
	public Transform frame0;

    /// <summary>
    /// Reference to the second frame of the voxel animation.
    /// </summary>
	public Transform frame1;

    /// <summary>
    /// Reference to the debris voxel model.
    /// </summary>
    public Transform debris;

    /// <summary>
    /// Time delay between voxel animation frames.
    /// </summary>
	public float animationDelay = 0.5f;

	private int frameIndex = 0;
	private float nextFrame = 0.0f;
    private BoxCollider boxCollider;

    [ContextMenu("Preview")]
    void Preview ()
    {
        frame0.GetComponent<VoxelModel>().Initialize();
    }

	// Use this for initialization
	void Start () {
		frameIndex = 0;
        nextFrame = 0.0f;

        boxCollider = GetComponent<BoxCollider>();        
	}
	
	void FixedUpdate()
	{
		float frameTime = Time.time;

		if (frameTime > nextFrame)
		{
			if (frameIndex == 0)
			{
				frame0.gameObject.SetActive(true);
				frame1.gameObject.SetActive(false);

                Bounds bounds = frame0.GetComponent<VoxelModel>().GetBounds();
                boxCollider.center = bounds.center;
                boxCollider.size = bounds.size;

				frameIndex = 1;
			}
			else if (frameIndex == 1)
			{
				frame0.gameObject.SetActive(false);
				frame1.gameObject.SetActive(true);

                Bounds bounds = frame1.GetComponent<VoxelModel>().GetBounds();
                boxCollider.center = bounds.center;
                boxCollider.size = bounds.size;

				frameIndex = 0;
			}

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
        if (frameIndex == 0)
        {
            Vector3 localPos = frame0.InverseTransformPoint(position);
            return frame0.GetComponent<VoxelModel>().GetVoxel(localPos) > 0;
        }
        else if (frameIndex == 1)
        {
            Vector3 localPos = frame0.InverseTransformPoint(position);
            return frame1.GetComponent<VoxelModel>().GetVoxel(localPos) > 0;
        }

        return false;
    }

    /// <summary>
    /// Explodes the alien into debris by a force at a given location.
    /// </summary>
    /// <param name="position">Location of the explosion force.</param>
    /// <param name="force">Magnitude of the explosion force.</param>
    /// <param name="radius">Radius of the explosion force.</param>
    public void ExplodeAt(Vector3 position, float force, float radius)
    {
        VoxelModel vm = (frameIndex == 0) ? frame0.GetComponent<VoxelModel>() : frame1.GetComponent<VoxelModel>();

        foreach (Vector3 point in vm.ToPoints())
        {
            GameObject go = Instantiate(debris.gameObject, vm.transform.TransformPoint(point), Quaternion.identity) as GameObject;
            go.rigidbody.AddExplosionForce(force, position, radius);
        }

        Destroy(gameObject);
    }
}
