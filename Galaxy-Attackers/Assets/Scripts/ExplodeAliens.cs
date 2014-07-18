using UnityEngine;
using System.Collections;

public class ExplodeAliens : MonoBehaviour {

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
                    rayHit.transform.GetComponent<AlienManager>().Explode(rayHit.point);
                }
            }
        }
	}
}
