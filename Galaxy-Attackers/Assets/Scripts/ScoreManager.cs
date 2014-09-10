using UnityEngine;
using System.Collections;

public class ScoreManager : MonoBehaviour {

	private int score;

	// Use this for initialization
	void Start () {
		score = 0;
	}

	public void GainPoints(int points)
	{
		score += points;
	}

	public void LosePoints(int points)
	{
		score -= points;
	}

	public int GetScore()
	{
		return score;
	}
}
