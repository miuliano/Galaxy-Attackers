using UnityEngine;
using System.Collections;

public class ExplodeAliens : MonoBehaviour {

    public GameObject debris;

	// Use this for initialization
	void Start () {
	
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
                        vm.SetVoxel(localHitPoint, 0);

                        Instantiate(debris, vm.transform.TransformPoint(vm.GetVoxelCentre(localHitPoint)), Quaternion.identity);
                    }

                }
            }
        }
	}
}
