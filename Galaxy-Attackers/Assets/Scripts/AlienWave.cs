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

    public Bounds waveBounds = new Bounds();
    private Bounds alienBounds = new Bounds();
    private Vector3 boundsOffset = new Vector3();

    private Vector3 horizontalMove = Vector3.right;
    private Vector3 verticalMove = Vector3.down;
    private float nextMove = 0.0f;

    private bool flagUpdateBounds = false;

    void Awake()
    {
        // Update alien bounds
        alienBounds = GetBounds();
        boundsOffset = alienBounds.center - transform.position;

        // Listen for alien deaths
        foreach (Alien alien in GetComponentsInChildren<Alien>())
        {
            alien.OnDestroy += alien_OnDestroy;
        }
    }

    // Alien death handler
    void alien_OnDestroy(Transform alien)
    {
        flagUpdateBounds = true;
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
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(waveBounds.center, waveBounds.size);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(alienBounds.center, alienBounds.size);
    }
}
