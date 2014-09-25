using UnityEngine;
using System.Collections;

public class PlayerManager : MonoBehaviour {
	
	public delegate void PlayerManagerHandler(int lives);

    /// <summary>
    /// Get or set the number of lives the player has.
    /// </summary>
    public int Lives { get; set; }

	/// <summary>
	/// Gets the number of shots taken.
	/// </summary>
	/// <value>The shots taken.</value>
	public int ShotsTaken
	{
		get
		{
			return shotsTaken;
		}
	}

	/// <summary>
	/// Gets the player.
	/// </summary>
	/// <value>The player.</value>
	public Player Player
	{
		get
		{
			return player;
		}
	}

    /// <summary>
    /// The number of lives a player starts with.
    /// </summary>
    public int startingLives;

    /// <summary>
    /// Time delay before respawning the player on death.
    /// </summary>
    public float respawnDelay = 0.0f;

    /// <summary>
    /// Player instance.
    private Player player;

    /// <summary>
    /// Flags whether a respawn should occur when the respawn timer finishes.
    /// </summary>
    private bool flagRespawn;

    /// <summary>
    /// Time of the next respawn.
    /// </summary>
    private float nextRespawn = 0.0f;

	private int shotsTaken;

	/// <summary>
	/// Occurs when the number of player lives change.
	/// </summary>
	public event PlayerManagerHandler OnLivesChanged;

    // Use this for initialization
    void Start()
    {
		flagRespawn = false;
		nextRespawn = respawnDelay;
		shotsTaken = 0;

        Lives = startingLives;

        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        player.OnDeath += player_OnDeath;
		player.OnShoot += player_OnShoot;
    }

    void Update()
    {
        if (flagRespawn && Time.time > nextRespawn)
        {
            flagRespawn = false;
            player.Respawn();
        }
    }

    void player_OnDeath(Player player)
    {
        Lives -= 1;

		if (OnLivesChanged != null)
		{
			OnLivesChanged(Lives);
		}

        if (Lives <= 0)
        {
            // GAME OVER;
            return;
        }

        nextRespawn = Time.time + respawnDelay;
        flagRespawn = true;
    }

	void player_OnShoot(Player player)
	{
		shotsTaken += 1;
	}
}
