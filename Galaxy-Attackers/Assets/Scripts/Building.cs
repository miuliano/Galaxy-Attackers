using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour {

    /// <summary>
    /// Reference to the building voxel model.
    /// </summary>
    public Transform buildingModel;

    /// <summary>
    /// Reference to the debris prefab.
    /// </summary>
    public Transform debris;

	/// <summary>
	/// The radius of the building to destroy when hit.
	/// </summary>
    public float destroyRadius = 1.0f;

	private VoxelModel voxelModel;

    private BoxCollider boxCollider;

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

    /// <summary>
    /// Check for collision between a point and the building.
    /// </summary>
    /// <param name="position">Point in world coordinates.</param>
    /// <returns>True on collision, false otherwise.</returns>
    public bool CheckCollision(Vector3 position)
    {
        Vector3 localPos = voxelModel.transform.InverseTransformPoint(position);
        return voxelModel.GetVoxel(localPos) > 0;
    }

    /// <summary>
    /// Explodes the building into debris by a force at a given location.
    /// </summary>
    /// <param name="position">Location of the explosion force.</param>
    /// <param name="force">Magnitude of the explosion force.</param>
    /// <param name="radius">Radius of the explosion force.</param>
    public void ExplodeAt(Vector3 position, float force, float radius)
    {
		Vector3 localPoint = voxelModel.transform.InverseTransformPoint(position);
		IntVector2 voxelPoint = voxelModel.WorldToVoxelSpace(localPoint);

        // Magic explosion radius
        const int voxelRadius = 6;

        // Check 9x9 grid around colliding voxel
        for (int y = -voxelRadius; y <= voxelRadius; y++)
        {
            for (int x = -voxelRadius; x <= voxelRadius; x++)
            {
				if (voxelModel.GetVoxel(voxelPoint.x + x, voxelPoint.y + y) > 0 &&
                    (Random.value >= (x * x + y * y) * (1.0f / (2 * voxelRadius * voxelRadius)) || (x == 0 && y == 0)))
                {
                    // Convert voxel coordinate to world space
					Vector3 worldPoint = voxelModel.transform.TransformPoint(voxelModel.VoxelToWorldSpace(voxelPoint.x + x, voxelPoint.y + y));

                    GameObject go = Instantiate(debris.gameObject, worldPoint, Quaternion.identity) as GameObject;

                    go.rigidbody.AddExplosionForce(force, position, radius);

					voxelModel.SetVoxel(voxelPoint.x + x, voxelPoint.y + y, 0);
                }
            }
        }
    }

	void OnDrawGizmos()
	{
		Gizmos.DrawIcon(transform.position, "building_icon.png");
	}
}
