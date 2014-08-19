using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public float playerSpeed = 10.0f;
	public Bounds playerBounds = new Bounds(Vector3.zero, Vector3.one);
	public Vector3 gunOffset = Vector3.zero;
    public GameObject bullet;
    public Transform playerModel;

    [ContextMenu("Preview")]
    void Preview()
    {
        playerModel.GetComponent<VoxelModel>().Initialize();
    }

	// Use this for initialization
	void Start () {
	
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(playerBounds.center, playerBounds.size);

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position + gunOffset, 1.0f);
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
		if (fire1 == true && GameObject.FindGameObjectsWithTag("Bullet").Length == 0)
		{
			Instantiate(bullet, transform.position + gunOffset, Quaternion.identity);
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
}
