using UnityEngine;
using System.Collections;

public class AlienWave : MonoBehaviour {

    /// <summary>
    /// Distance to move in each movement step.
    /// </summary>
    public float moveDistance = 1.0f;

    /// <summary>
    /// Time delay between movement steps.
    /// </summary>
    public float moveDelay = 1.0f;

	/// <summary>
	/// Time delay between alien shots.
	/// </summary>
	public float shootDelay = 10.0f;

	/// <summary>
	/// Text file containing wave information.
	/// </summary>
	public TextAsset waveFile;
	
	/// <summary>
	/// The bounds in which the aliens can move in.
	/// </summary>
    public Bounds waveBounds;

	/// <summary>
	/// List of available alien types.
	/// </summary>
	public Transform[] alienTypes;

	/// <summary>
	/// List of available bullets that the aliens fire.
	/// </summary>
	public Transform[] bulletTypes;

	private int[] waveData;
	private int waveWidth = 0;
	private int waveHeight = 0;
	private int waveAlive = 0;

    private Bounds alienBounds = new Bounds();
    private Vector3 boundsOffset = new Vector3();

    private Vector3 horizontalMove = Vector3.right;
    private Vector3 verticalMove   = Vector3.down;

    private float nextMove = 0.0f;
	private float nextShoot = 0.0f;

    private bool flagUpdateBounds = false;

	void Start()
	{
		LoadWaveData();

		// Create aliens
		float xScale = waveBounds.size.x / waveWidth;
		float yScale = waveBounds.size.y / waveHeight;
		float xOffset = waveBounds.size.x / 2.0f - (xScale / 2.0f);
		float yOffset = waveBounds.size.y / 2.0f - (yScale / 2.0f);

		for (int y = 0; y < waveHeight; y++)
		{
			for (int x = 0; x < waveWidth; x++)
			{
				int alienIndex = waveData[y * waveWidth + x];

				GameObject alien = Instantiate(alienTypes[alienIndex].gameObject) as GameObject;

				alien.name = x + ":" + y;
				alien.transform.parent = this.transform;
				alien.transform.localPosition = new Vector3(x * xScale - xOffset, -1.0f * (y * yScale - yOffset), 0);
				alien.GetComponent<Alien>().OnDestroy += alien_OnDestroy;

				waveAlive++;
			}
		}

		flagUpdateBounds = true;
	}

    // Alien death handler
    void alien_OnDestroy(Transform alien)
    {
		// Hacky way to get coordinate and remove alien
		string[] strCoords = alien.name.Split(new char[]{':'});
		int x = int.Parse(strCoords[0]);
		int y = int.Parse(strCoords[1]);
		waveData[y * waveWidth + x] = -1;
		Debug.Log(string.Format("Alien at {0},{1} dieded", x, y));

        flagUpdateBounds = true;
		waveAlive--;
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
			Vector3 newPosition = transform.position + horizontalMove * moveDistance;
			
			alienBounds.center = newPosition + boundsOffset;

            if (BoundsContainsBounds(waveBounds, alienBounds))
            {
                transform.position = newPosition;
            }
            else
            {
                // Try vertical movement
                horizontalMove *= -1;
                newPosition = transform.position + verticalMove * moveDistance;

                alienBounds.center = newPosition + boundsOffset;

                if (BoundsContainsBounds(waveBounds, alienBounds))
                {
                    transform.position = newPosition;
                }
                else
                {
                    verticalMove *= -1;
                    transform.position += verticalMove * moveDistance;
                }
            }

            alienBounds.center = transform.position + boundsOffset;

            nextMove = frameTime + moveDelay;
        }

		if (frameTime > nextShoot)
		{
			// Initialize 
			int[] colSequence = new int[waveWidth];

			for (int i = 0; i < waveWidth; i++)
			{
				colSequence[i] = i;
			}

			// Shuffle
			for (int i = waveWidth - 1; i >= 1; i--)
			{
				int j = Random.Range(0, i + 1);
				int temp = colSequence[i];
				colSequence[i] = colSequence[j];
				colSequence[j] = temp;
			}

			// Find lowest living alien - assumes at least one exists
			int x = colSequence[0];
			int y = 0;

			for (; y < waveHeight; y++)
			{
				if (waveData[y * waveWidth + x] == -1)
				{
					break;
				}
			}

			y--;

			// Create bullet
			float xScale = waveBounds.size.x / waveWidth;
			float yScale = waveBounds.size.y / waveHeight;
			float xOffset = waveBounds.size.x / 2.0f - (xScale / 2.0f);
			float yOffset = waveBounds.size.y / 2.0f - (yScale / 2.0f);

			int bulletIndex = Random.Range(0, bulletTypes.Length);

			Vector3 bulletPos = new Vector3(x * xScale - xOffset, -1.0f * (y * yScale - yOffset), 0);
			bulletPos = transform.TransformPoint(bulletPos);

			Instantiate(bulletTypes[bulletIndex], bulletPos, Quaternion.identity);

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

    Bounds GetBounds(Transform parent)
    {
        Bounds bounds = new Bounds();
 
        foreach (Transform child in parent)
        {
            // Skip self and avoid adding empty bounds
            if (parent.renderer != child.renderer && child.renderer.bounds.extents != Vector3.zero)
            {                
                if (bounds.extents == Vector3.zero)
                {             
                    bounds = child.renderer.bounds;
                }                
                else
                {                    
                    bounds.Encapsulate(child.renderer.bounds);
                }
            }

            // Recursively retrieve bounds
            Bounds childBounds = GetBounds(child);

            if (childBounds.extents != Vector3.zero)
            {
                if (bounds.extents == Vector3.zero)
                {
                    bounds = childBounds;
                }
                else
                {
                    bounds.Encapsulate(childBounds);
                }
            }
        }

        return bounds;
    }

    Bounds GetBounds()
    {
        return GetBounds(transform);
    }

    void OnDrawGizmos()
    {
		// Draw wave bounds
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(waveBounds.center, waveBounds.size);

		// Draw alien bounds
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(alienBounds.center, alienBounds.size);
    }
}
