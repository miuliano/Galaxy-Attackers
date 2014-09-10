using UnityEngine;
using System.Collections;

public class GameGUI : MonoBehaviour {

	ScoreManager scoreManager;

	void Start()
	{
		scoreManager = GameObject.FindObjectOfType<ScoreManager>();
	}

	void OnGUI()
	{
		GUI.Label(new Rect(10, 10, 100, 45), "SCORE <1>");
		GUI.Label(new Rect(Screen.width / 2.0f - 50, 10, 100, 45), "HI-SCORE");
		GUI.Label(new Rect(Screen.width - 100, 10, 100, 45), "SCORE <2>");

		GUI.Label(new Rect(10, 30, 100, 45), string.Format("{0,3}", scoreManager.GetScore().ToString("D3")));

		GUI.Label(new Rect(Screen.width - 100, Screen.height - 45, 100, 45), "CREDIT 00");
	}
}
