using UnityEngine;
using System.Collections;

public class PlayerManager : MonoBehaviour {
	
	public delegate void PlayerManagerHandler(int lives);

    /// <summary>
    /// Get or set the number of lives the player has.
    /// </summary>
    public int lives { get; set; }

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
    private bool flagRespawn = false;

    /// <summary>
    /// Time of the next respawn.
    /// </summary>
    private float nextRespawn = 0.0f;

	/// <summary>
	/// Occurs when the number of player lives change.
	/// </summary>
	public event PlayerManagerHandler OnLivesChanged;

    // Use this for initialization
    void Start()
    {
        lives = startingLives;

        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        player.OnDeath += player_OnDeath;
    }

    void Update()
    {
        if (flagRespawn && Time.time > nextRespawn)
        {
            flagRespawn = false;
            player.Respawn();
        }
    }

    void player_OnDeath(Transform player)
    {
        lives -= 1;

		if (OnLivesChanged != null)
		{
			OnLivesChanged(lives);
		}

        if (lives <= 0)
        {
            // GAME OVER;
            return;
        }

        nextRespawn = Time.time + respawnDelay;
        flagRespawn = true;
    }
}
