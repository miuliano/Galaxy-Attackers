using UnityEngine;
using System.Collections;

public class MothershipManager : MonoBehaviour {

	/// <summary>
	/// The time delay before spawning a mothership.
	/// </summary>
	public float spawnDelay = 25.0f;
	
	/// <summary>
	/// The spawn location for the bonus ship when coming from the left.
	/// </summary>
	public Vector3 spawnLeft;
	
	/// <summary>
	/// The spawn location for the bonus ship when coming from the right.
	/// </summary>
	public Vector3 spawnRight;

	/// <summary>
	/// The bonus alien prefab.
	/// </summary>
	public Transform mothershipPrefab;

	private float nextSpawn;

	private ScoreManager scoreManager;
	private PlayerManager playerManager;

	// Use this for initialization
	void Start () {
		nextSpawn = spawnDelay;

		scoreManager = GameObject.FindObjectOfType<ScoreManager>();
		playerManager = GameObject.FindObjectOfType<PlayerManager>();
	}
	
	// Update is called once per frame
	void Update () {
		float frameTime = Time.time;

		if (frameTime > nextSpawn)
		{
			int spawnCondition = Random.Range(0, 2); // 0 - left, 1 - right
			
			Vector3 spawnPosition = spawnCondition == 0 ? spawnLeft : spawnRight;
			
			GameObject go = Instantiate(mothershipPrefab.gameObject, spawnPosition, Quaternion.identity) as GameObject;

			// Make child
			go.transform.parent = transform;

			Mothership mothership = go.GetComponent<Mothership>();
			mothership.moveDirection = spawnCondition == 0 ? Vector3.right : Vector3.left;
			mothership.destination = spawnCondition == 0 ? spawnRight : spawnLeft;
			mothership.OnDestroy += mothership_OnDestroy;
			
			// Determine how many points this bonus is worth
			if (playerManager.ShotsTaken == 23 || (playerManager.ShotsTaken - 23) % 15 == 0)
			{
				mothership.pointValue = 300;
			}
			else
			{
				int pointCondition = Random.Range(0, 4);
				
				if (pointCondition == 0)
					mothership.pointValue = 50;
				else if (pointCondition == 1)
					mothership.pointValue = 100;
				else if (pointCondition == 2)
					mothership.pointValue = 150;
				else if (pointCondition == 3)
					mothership.pointValue = 300;
			}
			
			nextSpawn = frameTime + spawnDelay;
		}
	}

	// Mothership death handler
	void mothership_OnDestroy(Alien alien)
	{
		// Give em points
		scoreManager.GainPoints(alien.pointValue);
	}

	void OnDrawGizmos()
	{
		// Draw spawn/destination locations
		Gizmos.DrawIcon(spawnLeft, "alien_icon.png");
		Gizmos.DrawIcon(spawnRight, "alien_icon.png");
	}
}
