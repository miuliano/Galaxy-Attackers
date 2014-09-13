using UnityEngine;
using System.Collections;

public class GameGUI : MonoBehaviour {

	/// <summary>
	/// Model to use for life tokens
	/// </summary>
	public Transform lifeToken;

	/// <summary>
	/// The life token position.
	/// </summary>
	public Vector3 lifeTokenPosition;

	/// <summary>
	/// The width of the life token.
	/// </summary>
	public float lifeTokenWidth;

	private GameObject[] lifeTokens;

	private TextMesh player1ScoreText;
	private TextMesh livesText;

	private ScoreManager scoreManager;
	private PlayerManager playerManager;

	// Use this for initialization
	void Start () {
		player1ScoreText = GameObject.Find("Player1ScoreText").GetComponent<TextMesh>();
		livesText = GameObject.Find("LivesText").GetComponent<TextMesh>();

		// Register event listeners
		scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
		scoreManager.OnScoreChanged += scoreManager_OnScoreChanged;

		PlayerManager playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
		playerManager.OnLivesChanged += playerManager_OnLivesChanged;

		// Setup life token overlay
		int lives = playerManager.startingLives;

		lifeTokens = new GameObject[lives - 1];
		
		for (int i = 0; i < lives - 1; i++)
		{
			lifeTokens[i] = Instantiate(lifeToken.gameObject) as GameObject;
			lifeTokens[i].transform.parent = transform;
			lifeTokens[i].transform.localPosition = lifeTokenPosition + i * new Vector3(lifeTokenWidth, 0, 0);
		}

		livesText.text = lives.ToString();
	}

	void scoreManager_OnScoreChanged(int score)
	{
		player1ScoreText.text = score.ToString("D3");
	}

	void playerManager_OnLivesChanged(int lives)
	{
		// Turn on/off the relevant number of life tokens
		for (int i = 0; i < lifeTokens.Length; i++)
		{
			if (i < lives - 1)
				lifeTokens[i].GetComponent<VoxelModel>().hidden = false;
			else
				lifeTokens[i].GetComponent<VoxelModel>().hidden = true;
		}

		livesText.text = lives.ToString();
	}

	void OnDrawGizmos()
	{
		// Life token indicator
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(lifeTokenPosition, 2.0f);
	}
}
