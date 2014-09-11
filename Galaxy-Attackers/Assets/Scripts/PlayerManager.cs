using UnityEngine;
using System.Collections;

public class PlayerManager : MonoBehaviour {

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
    /// Object to use as life tokens.
    /// </summary>
    public Transform lifeToken;

    /// <summary>
    /// Offset from the bottom left corner of the screen to draw life tokens.
    /// </summary>
    public Vector2 lifeTokenOffset;

    /// <summary>
    /// Width of life token.
    /// </summary>
    public float lifeTokenWidth;

    /// <summary>
    /// Player instance.
    /// </summary>
    private Player player;

    /// <summary>
    /// List of life tokens.
    /// </summary>
    private GameObject[] lifeTokens;

    /// <summary>
    /// Flags whether a respawn should occur when the respawn timer finishes.
    /// </summary>
    private bool flagRespawn = false;

    /// <summary>
    /// Time of the next respawn.
    /// </summary>
    private float nextRespawn = 0.0f;

    // Use this for initialization
    void Start()
    {
        lives = startingLives;

        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        player.OnDeath += player_OnDeath;

        Vector3 screenPoint = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0)) - Camera.main.transform.position;

        lifeTokens = new GameObject[lives - 1];

        for (int i = 0; i < lives - 1; i++)
        {
            lifeTokens[i] = Instantiate(lifeToken.gameObject, new Vector3(
                lifeTokenOffset.x - (screenPoint.x) + lifeTokenWidth * i,
                lifeTokenOffset.y - (screenPoint.y),
                0.0f
                ), Quaternion.identity) as GameObject;
        }
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

        if (lives <= 0)
        {
            // GAME OVER;
            return;
        }

        lifeTokens[lives - 1].GetComponent<VoxelModel>().hidden = true;

        nextRespawn = Time.time + respawnDelay;
        flagRespawn = true;
    }
}
