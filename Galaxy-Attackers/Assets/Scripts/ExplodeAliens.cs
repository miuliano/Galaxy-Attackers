using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExplodeAliens : MonoBehaviour {

    public GameObject debris;
	public float explosionForce = 100.0f;
	public float explosionRadius = 10.0f;

	private LinkedList<GameObject> debrisList;

	// Use this for initialization
	void Start () {
		debrisList = new LinkedList<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Fire1"))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;

            if (Physics.Raycast(ray, out rayHit))
            {
                if (rayHit.transform.tag == "Enemy")
                {
                    Vector3 hitPoint = rayHit.point;
                    Vector3 localHitPoint = rayHit.transform.InverseTransformPoint(hitPoint);

                    VoxelModel vm = rayHit.transform.GetComponent<VoxelModel>();

                    if (vm.GetVoxel(localHitPoint))
                    {
						Vector3[] debrisPoints = vm.ToPoints();

						foreach (Vector3 point in debrisPoints)
						{
							GameObject go = Instantiate(debris, vm.transform.TransformPoint(point), Quaternion.identity) as GameObject;

							go.rigidbody.AddExplosionForce(explosionForce, hitPoint, explosionRadius);

							debrisList.AddLast(go);
						}

						vm.DestroyVoxelModel();
                    }
                }
            }
        }
	}

	void LateUpdate ()
	{
		// Prune sleeping debris
		LinkedListNode<GameObject> node = debrisList.First;

		while(node != null)
		{
			if (node.Value.rigidbody != null && node.Value.rigidbody.IsSleeping())
			{
				Destroy(node.Value.rigidbody);

				LinkedListNode<GameObject> toRemove = node;
				node = node.Next;
				debrisList.Remove(toRemove);
			}
			else
			{
				node = node.Next;
			}
		}
	}
}
