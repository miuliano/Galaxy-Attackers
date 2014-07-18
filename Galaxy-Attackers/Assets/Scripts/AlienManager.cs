using UnityEngine;
using System.Collections;

public class AlienManager : MonoBehaviour
{    
    public float frameDelay;
    public string frameName1 = "Frame1";
    public string frameName2 = "Frame2";
    public string deathFrameName1 = "DeathFrame1";
    public string deathFrameName2 = "DeathFrame2";

    private Transform frame1;
    private Transform frame2;
    private Transform deathFrame1;
    private Transform deathFrame2;

    private int frame;
    private float frameElapsed;

    private bool alive;

    // Use this for initialization
    void Start()
    {
        alive = true;
        frameElapsed = 0.0f;
        frame = 1;

        foreach (Transform child in transform)
        {
            if (child.name == frameName1)
            {
                frame1 = child;
            }
            else if (child.name == frameName2)
            {
                frame2 = child;
            }
            else if (child.name == deathFrameName1)
            {
                deathFrame1 = child;
            }
            else if (child.name == deathFrameName2)
            {
                deathFrame2 = child;
            }
        }

        frame1.gameObject.SetActive(true);
        frame2.gameObject.SetActive(false);
        deathFrame1.gameObject.SetActive(false);
        deathFrame2.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Don't animate if dead
        if (alive == false) return;

        frameElapsed += Time.deltaTime;

        if (frameElapsed >= frameDelay)
        {
            if (frame == 1)
            {
                frame1.gameObject.SetActive(false);
                frame2.gameObject.SetActive(true);
                frame = 2;
            }
            else if (frame == 2)
            {
                frame1.gameObject.SetActive(true);
                frame2.gameObject.SetActive(false);
                frame = 1;
            }

            frameElapsed = 0.0f;
        }
    }

    // Asplode
    public void Explode(Vector3 atPosition)
    {
        if (alive == false) return;

        alive = false;

        frame1.gameObject.SetActive(false);
        frame2.gameObject.SetActive(false);
        collider.enabled = false;

        if (frame == 1)
        {
            deathFrame1.gameObject.SetActive(true);
            
            foreach (Transform child in deathFrame1)
            {
                child.rigidbody.AddExplosionForce(200.0f, atPosition, 50.0f);
            }
        }
        else if (frame == 2)
        {
            deathFrame2.gameObject.SetActive(true);

            foreach (Transform child in deathFrame2)
            {
                child.rigidbody.AddExplosionForce(200.0f, atPosition, 50.0f);
            }
        }
    }
}
