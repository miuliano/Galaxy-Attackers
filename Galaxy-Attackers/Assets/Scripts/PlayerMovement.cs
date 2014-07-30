using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

    public float playerSpeed = 10.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        float hMove = Input.GetAxis("Horizontal");
        float vMove = Input.GetAxis("Vertical");

        if (hMove > 0)
        {
            transform.position += Vector3.right * Mathf.Abs(hMove) * Time.deltaTime * playerSpeed;
        }
        else if (hMove < 0)
        {
            transform.position += Vector3.left * Mathf.Abs(hMove) * Time.deltaTime * playerSpeed;
        }

		if (Input.GetKeyUp(KeyCode.P) && Camera.main.GetComponent<ChangeProjection>().State == ChangeProjection.ProjectionState.Orthographic)
		{
			Camera.main.GetComponent<ChangeProjection>().ToPerspective(1.0f);
		}
		if (Input.GetKeyUp(KeyCode.O) && Camera.main.GetComponent<ChangeProjection>().State == ChangeProjection.ProjectionState.Perspective)
		{
			Camera.main.GetComponent<ChangeProjection>().ToOrthographic(1.0f);
		}
	}
}
