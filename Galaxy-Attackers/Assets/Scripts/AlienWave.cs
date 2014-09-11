using UnityEngine;
using System.Collections;

public class AlienWave : MonoBehaviour {

    /// <summary>
    /// Distance to move horizontally in each movement step.
    /// </summary>
    public float horizontalMoveDistance = 1.0f;

    /// <summary>
    /// Distance to move horizontally in each movement step.
    /// </summary>
    public float verticalMoveDistance = 4.0f;

	/// <summary>
	/// The maximum move delay.
	/// </summary>
	public float maxMoveDelay = 1.0f;

	/// <summary>
	/// The minimum move delay.
	/// </summary>
	public float minMoveDelay = 0.5f;

	/// <summary>
	/// Time delay between alien shots.
	/// </summary>
	public float shootDelay = 10.0f;

	/// <summary>
	/// The offset from the alien position that is shooting.
	/// </summary>
	public Vector3 shootOffset;

	/// <summary>
	/// Text file containing wave information.
	/// </summary>
	public TextAsset waveFile;
	
	/// <summary>
	/// The size of the alien wave.
	/// </summary>
    public Vector3 waveSize;

	/// <summary>
	/// The bounds in which the wave can move.
	/// </summary>
	public Bounds moveBounds;

	/// <summary>
	/// List of available alien types.
	/// </summary>
	public Transform[] alienTypes;

	/// <summary>
	/// List of available bullets that the aliens fire.
	/// </summary>
	public Transform[] bulletTypes;

	private Alien[] wave;

	private int[] waveData;
	private int waveWidth = 0;
	private int waveHeight = 0;
	private int waveAlive = 0;
	private int maxWaveAlive = 0;

    private Bounds alienBounds = new Bounds();
    private Vector3 boundsOffset = new Vector3();

    private Vector3 horizontalMoveDirection = Vector3.right;
    private Vector3 verticalMoveDirection = Vector3.down;

	public float moveDelay = 1.0f;
    private float nextMove = 0.0f;
	private float nextShoot = 0.0f;

	private ScoreManager scoreManager;

    private bool flagUpdateBounds = false;

	void Start()
	{
		LoadWaveData();

		wave = new Alien[waveHeight * waveWidth];

		// Create aliens
		float xScale = waveSize.x / waveWidth;
		float yScale = waveSize.y / waveHeight;
		float xOffset = waveSize.x / 2.0f - (xScale / 2.0f);
		float yOffset = waveSize.y / 2.0f - (yScale / 2.0f);

		for (int y = 0; y < waveHeight; y++)
		{
			for (int x = 0; x < waveWidth; x++)
			{
				int alienIndex = waveData[y * waveWidth + x];

				GameObject alienGameObject = Instantiate(alienTypes[alienIndex].gameObject) as GameObject;

				alienGameObject.name = "Alien_" + x + ":" + y;
				alienGameObject.transform.parent = this.transform;
				alienGameObject.transform.localPosition = new Vector3(x * xScale - xOffset, -1.0f * (y * yScale - yOffset), 0);

				Alien alien = alienGameObject.GetComponent<Alien>();

				alien.OnDestroy += alien_OnDestroy;
				wave[y * waveWidth + x] = alien;

				waveAlive++;
			}
		}

		maxWaveAlive = waveAlive;

		moveDelay = maxMoveDelay;
		nextShoot = shootDelay;
		nextMove = moveDelay;

		scoreManager = GameObject.FindObjectOfType<ScoreManager>();

		flagUpdateBounds = true;
	}

	// Alien death handler
    void alien_OnDestroy(Transform alien)
    {
		// Give em points
		int points = alien.GetComponent<Alien>().pointValue;

		scoreManager.GainPoints(points);

		// Linearly interpolate between max and min move delay based on aliens left
		moveDelay = (maxMoveDelay - minMoveDelay) * (waveAlive / (float)maxWaveAlive) + minMoveDelay;

		waveAlive--;

		flagUpdateBounds = true;
    }
	
	/// <summary>
	/// Gets the height (y-coordinate) of the lowest alien.
	/// </summary>
	/// <returns>The lowest alien height.</returns>
	/// <param name="x">The x coordinate of the colunm.</param>
	int GetLowestAlienHeight(int x)
	{
		int lowest = -1;

		for (int y = 0; y < waveHeight; y++)
		{
			if (wave[y * waveWidth + x].alive)
			{
				lowest = y;
			}
		}

		return lowest;
	}

	/// <summary>
	/// Loads the wave data from the associated file.
	/// </summary>
	void LoadWaveData()
	{
		if (waveFile == null) 
		{
			Debug.LogWarning("AlienWave::LoadWaveData(): No wave file specified.");
			return;
		}
		
		// Slurp and split
		string text    = waveFile.text;
		string[] lines = text.Split(new char[] {'\r','\n'});
		
		waveWidth = 0;
		waveHeight = 0;
		
		int index = 0;
		
		foreach (string line in lines)
		{
			if (string.IsNullOrEmpty(line)) continue;
			
			if (waveWidth == 0)
			{
				waveWidth = int.Parse(line);
			}
			else if (waveHeight == 0)
			{
				waveHeight = int.Parse(line);
				
				waveData = new int[waveWidth * waveHeight];
			}
			else
			{
				string[] aliens = line.Split(new char[] { ' ' });
				
				foreach (string alien in aliens)
				{
					if (index < waveData.Length)
					{
						waveData[index] = int.Parse(alien);
						
						index++;
					}
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		float frameTime = Time.time;
		
		if (frameTime > nextMove)
		{
			// Try horizontal movement
			Vector3 newPosition = transform.position + horizontalMoveDirection * horizontalMoveDistance;
			
			alienBounds.center = newPosition + boundsOffset;

			if (BoundsContainsBounds(moveBounds, alienBounds))
            {
                transform.position = newPosition;
            }
            else
            {
                // Try vertical movement
                horizontalMoveDirection *= -1;
                newPosition = transform.position + verticalMoveDirection * verticalMoveDistance;

                alienBounds.center = newPosition + boundsOffset;

				if (BoundsContainsBounds(moveBounds, alienBounds))
                {
                    transform.position = newPosition;
                }
            }

            alienBounds.center = transform.position + boundsOffset;

            nextMove = frameTime + moveDelay;
        }

		if (frameTime > nextShoot)
		{
			bool foundShootPoint = false;
			int x = 0, y = 0;

			// Create sequence of columns to check for shooting
			int[] shootSequence = new int[waveWidth];

			for (int i = 0; i < waveWidth; i++) shootSequence[i] = i;

			Utility.Shuffle<int>(shootSequence);

			// Find a viable shooting point
			for (int i = 0; i < waveWidth && foundShootPoint == false; i++)
			{
				x = shootSequence[i];
				y = GetLowestAlienHeight(x);

				if (y >= 0) {
					foundShootPoint = true;
				}
			}

			if (foundShootPoint)
			{
				// Create bullet
				float xScale = waveSize.x / waveWidth;
				float yScale = waveSize.y / waveHeight;
				float xOffset = waveSize.x / 2.0f - (xScale / 2.0f);
				float yOffset = waveSize.y / 2.0f - (yScale / 2.0f);

				int bulletIndex = Random.Range(0, bulletTypes.Length);

				Vector3 bulletPos = new Vector3(x * xScale - xOffset, -1.0f * (y * yScale - yOffset), 0) + shootOffset;

				// Transform to world space
				bulletPos = transform.TransformPoint(bulletPos);

				Instantiate(bulletTypes[bulletIndex], bulletPos, Quaternion.identity);
			}
			else
			{
				Debug.Log("No aliens to shoot from. Game should be over ;)");
			}

			nextShoot = frameTime + shootDelay;
		}
	}

    void LateUpdate()
    {
        if (flagUpdateBounds == true)
        {
            flagUpdateBounds = false;

            alienBounds = GetBounds();

            boundsOffset = alienBounds.center - transform.position;
        }
    }

    /// <summary>
    /// Tests whether one bounds lies in another bounds.
    /// </summary>
    /// <param name="a">Outer bounds.</param>
    /// <param name="b">Inner bounds.</param>
    /// <returns>True if a contains b.</returns>
    bool BoundsContainsBounds(Bounds a, Bounds b)
    {
        if (a.Contains(b.max) == false) return false;
        if (a.Contains(b.min) == false) return false;
        return true;
    }

	/// <summary>
	/// Return the bounds of living alien models.
	/// </summary>
	/// <returns>The model bounds.</returns>
    Bounds GetBounds()
    {
		Bounds bounds = new Bounds();

		foreach (Alien alien in wave)
		{
			if (alien.alive)
			{
				if (bounds.extents == Vector3.zero)
				{
					bounds = alien.GetBounds();
				}
				else
				{
					bounds.Encapsulate(alien.GetBounds());
				}
			}
		}

		return bounds;
    }

    void OnDrawGizmos()
    {
		// Draw wave bounds
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireCube(transform.position, waveSize);

		// Draw movement bounds
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(moveBounds.center, moveBounds.size);

		// Draw alien bounds
		Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(alienBounds.center, alienBounds.size);
    }
}
