using UnityEngine;
using System.Collections;

public class ChangeProjection : MonoBehaviour {

    public float orthographicNear = 0.3f;
    public float orthographicFar = 1000.0f;
    public float orthographicSize = 70.0f;
    public float duration = 1.0f;

    private Matrix4x4 ortho;
    private Matrix4x4 perspective;

    private bool isOrtho;
    private float aspect;

	// Use this for initialization
	void Start () {
        
        perspective = camera.projectionMatrix;
        
        aspect = (Screen.width + 0.0f) / (Screen.height + 0.0f);

        ortho = Matrix4x4.Ortho(-orthographicSize * aspect, orthographicSize * aspect, -orthographicSize, orthographicSize, orthographicNear, orthographicFar);
        isOrtho = false;
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isOrtho)
            {
                BlendProjection(ortho, perspective, duration);
            }
            else
            {
                BlendProjection(perspective, ortho, duration);
            }

            isOrtho = !isOrtho;
        }
	}

    private Matrix4x4 MatrixLerp (Matrix4x4 from, Matrix4x4 to, float t)
    {
        Matrix4x4 result = new Matrix4x4();
        for (int i = 0; i < 16; i++)
        {
            result[i] = Mathf.lerp(from[i], to[i], t);
        }
        return result;
    }

    private void BlendProjection (Matrix4x4 from, Matrix4x4 to, float t)
    {
        StopAllCoroutines();
        StartCoroutine(BlendCoroutine(from, to, t));
    }

    private IEnumerator BlendCoroutine(Matrix4x4 from, Matrix4x4 to, float t) {
        float startTime = Time.time;

        while (Time.time - startTime < t)
        {
            camera.projectionMatrix = MatrixLerp(from, to, (Time.time - startTime) / t);
            Debug.Log(camera.projectionMatrix.m23);
            yield return null;
        }
        Debug.Log(startTime + " -> " + Time.time);
        camera.projectionMatrix = to;
    }
}
