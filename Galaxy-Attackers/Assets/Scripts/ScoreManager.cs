using UnityEngine;
using System.Collections;

public class ScoreManager : MonoBehaviour {

	public delegate void ScoreHandler(int score);
	public event ScoreHandler OnScoreChanged;

	private int score;

	// Use this for initialization
	void Start () {
		score = 0;
	}

	public void GainPoints(int points)
	{
		score += points;

		if (OnScoreChanged != null)
		{
			OnScoreChanged(score);
		}
	}

	public void LosePoints(int points)
	{
		score -= points;

		if (OnScoreChanged != null)
		{
			OnScoreChanged(score);
		}
	}

	public int GetScore()
	{
		return score;
	}
}
