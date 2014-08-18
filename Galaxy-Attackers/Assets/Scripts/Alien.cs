using UnityEngine;
using System.Collections;

public class Alien : MonoBehaviour {
	
	public Transform frame0;
	public Transform frame1;
	public float animationDelay = 0.5f;

	private int frameIndex = 0;
	private float nextFrame = 0.0f;

	// Use this for initialization
	void Start () {
		frameIndex = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
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

				frameIndex = 1;
			}
			else if (frameIndex == 1)
			{
				frame0.gameObject.SetActive(false);
				frame1.gameObject.SetActive(true);

				frameIndex = 0;
			}

			nextFrame = frameTime + animationDelay;
		}
	}
}
