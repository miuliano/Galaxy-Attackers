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

        Debug.Log("Perspective:\r\n" + perspective.ToString());
        Debug.Log("Orthogonal:\r\n" + ortho.ToString());
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetKeyUp(KeyCode.Space))
        {
            if (isOrtho)
            {
                BlendProjection(perspective, ortho, duration, true);
            }
            else
            {
                BlendProjection(perspective, ortho, duration, false);
            }

            isOrtho = !isOrtho;
        }
	}

    private Matrix4x4 MatrixHeavisideInterpolation(Matrix4x4 from, Matrix4x4 to, float t)
    {
        Matrix4x4 result = new Matrix4x4();
        
        // Step function
        float p = 10.0f;
        float e_p = Mathf.Exp(p);
        float e_pt = Mathf.Exp(p * t);
        
        t = (1.0f + e_p) * (e_pt - 1.0f) / ((e_p - 1.0f) * (1.0f + e_pt));

        for (int i = 0; i < 16; i++)
        {
            result[i] = from[i] + (to[i] - from[i]) * t;
        }
        return result;
    }

    private void BlendProjection (Matrix4x4 from, Matrix4x4 to, float t, bool reverse)
    {
        StopAllCoroutines();
        StartCoroutine(BlendCoroutine(from, to, t, reverse));
    }

    private IEnumerator BlendCoroutine(Matrix4x4 from, Matrix4x4 to, float t, bool reverse) {
        float startTime = Time.time;
        float dt = 0;

        while (Time.time - startTime < t)
        {
            dt = (Time.time - startTime) / t;
            dt = (reverse) ? 1.0f - dt : dt;

            camera.projectionMatrix = MatrixHeavisideInterpolation(from, to, dt);
            yield return null;
        }

        camera.projectionMatrix = reverse ? from : to;
    }
}
