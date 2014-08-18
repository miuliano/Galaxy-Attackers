using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExplosionManager : MonoBehaviour {
	
    public GameObject debris;
	public float explosionForce = 100.0f;
	public float explosionRadius = 10.0f;

	/// <summary>
	/// Explodes a voxel model at a given position.
	/// </summary>
	/// <param name="position">Position of the explosion.</param>
	/// <param name="obj">Game object to explode.</param>
	public void ExplodeAt (Vector3 position, GameObject obj)
	{
		Vector3 hitPoint = position;
		Vector3 localHitPoint = obj.transform.InverseTransformPoint(hitPoint);
		
		VoxelModel vm = obj.transform.GetComponent<VoxelModel>();
		
		if (vm.GetVoxel(localHitPoint) > 0)
		{
			Vector3[] debrisPoints = vm.ToPoints();
			
			foreach (Vector3 point in debrisPoints)
			{
				GameObject go = Instantiate(debris, vm.transform.TransformPoint(point), Quaternion.identity) as GameObject;
				
				go.rigidbody.AddExplosionForce(explosionForce, hitPoint, explosionRadius);
			}
			
			Destroy(vm.gameObject);
		}
	}
}
