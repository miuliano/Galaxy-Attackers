using UnityEngine;
using System.Collections;

public class GameGUI : MonoBehaviour {

    public GUIStyle style;

    private ScoreManager scoreManager;
    private PlayerManager playerManager;

	void Start()
	{
		scoreManager = GameObject.FindObjectOfType<ScoreManager>();
        playerManager = GameObject.FindObjectOfType<PlayerManager>();
	}

	void OnGUI()
	{
        // Left-aligned stuff
        style.alignment = TextAnchor.UpperLeft;

		GUI.Label(new Rect(10, 10, 100, 45), "SCORE <1>", style);
        GUI.Label(new Rect(10, 30, 100, 45), string.Format("{0,3}", scoreManager.GetScore().ToString("D3")), style);
        GUI.Label(new Rect(10, Screen.height - 30, 100, 45), playerManager.lives.ToString(), style);

        // Center-aligned stuff
        style.alignment = TextAnchor.UpperCenter;

        GUI.Label(new Rect(Screen.width / 2.0f - 50, 10, 100, 45), "HI-SCORE", style);

        // Right-aligned stuff
        style.alignment = TextAnchor.UpperRight;

        GUI.Label(new Rect(Screen.width - 110, 10, 100, 45), "SCORE <2>", style);
        GUI.Label(new Rect(Screen.width - 110, Screen.height - 30, 100, 45), "CREDIT 00", style);       
	}
}
