using UnityEngine;
using System.Collections;

public class Mothership : Alien {

	/// <summary>
	/// The direction this mothership moves in.
	/// </summary>
	public Vector3 moveDirection = Vector3.zero;
	
	/// <summary>
	/// The distance to move on each update.
	/// </summary>
	public float moveDistance = 1.0f;
	
	/// <summary>
	/// The time delay before moving.
	/// </summary>
	public float moveDelay = 0.1f;

	/// <summary>
	/// The destination for this mothership.
	/// </summary>
	public Vector3 destination;

	private float nextMove;

	void Start()
	{
		base.Start();

		nextMove = moveDelay;
	}
	
	void Update()
	{
		float frameTime = Time.time;
		
		if (frameTime > nextMove)
		{
			transform.localPosition += moveDirection * moveDistance;

			// Remove if reached destination
			if ((moveDirection == Vector3.right && transform.localPosition.x > destination.x) ||
			    (moveDirection == Vector3.left && transform.localPosition.x < destination.x))
			{
				Destroy(gameObject);
			}
			
			nextMove = frameTime + moveDelay;
		}
	}
}
