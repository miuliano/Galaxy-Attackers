using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Camera))]
public class ChangeProjection : MonoBehaviour {

	public enum ProjectionState
	{
		Perspective,
		Orthographic,
		TransitionToPerspective,
		TransitionToOrthographic
	}

	public ProjectionState startingState = ProjectionState.Orthographic;

    public float orthographicNear = 0.3f;
    public float orthographicFar  = 1000.0f;
    public float orthographicSize = 70.0f;

	public float perspectiveFOV = 60.0f;
	public float perspectiveNear = 0.3f;
	public float perspectiveFar = 1000.0f;

    private Matrix4x4 ortho;
    private Matrix4x4 perspective;
    private float aspect;
	private ProjectionState projectionState;

	// Use this for initialization
	void Start () {
        
		// Calculate aspect ratio
        aspect = (Screen.width + 0.0f) / (Screen.height + 0.0f);

		// Create perspective projection
		perspective = Matrix4x4.Perspective(perspectiveFOV, aspect, perspectiveNear, perspectiveFar);

		// Create orthographic projection
        ortho = Matrix4x4.Ortho(-orthographicSize * aspect, orthographicSize * aspect, -orthographicSize, orthographicSize, orthographicNear, orthographicFar);

		projectionState = startingState;
	}

	/// <summary>
	/// Smoothly transform the projection to orthographic.
	/// </summary>
	/// <param name="time">Time in seconds for the transformation.</param>
	public void ToOrthographic(float time)
	{
		if (time < 0.0f) return;

		BlendProjection(ProjectionState.Orthographic, time);
	}

	/// <summary>
	/// Smoothly transform the projection to perspective.
	/// </summary>
	/// <param name="time">Time in seconds for the transformation.</param>
	public void ToPerspective(float time)
	{
		if (time < 0.0f) return;

		BlendProjection(ProjectionState.Perspective, time);
	}

	/// <summary>
	/// Returns the current camera projection state.
	/// </summary>
	/// <value>The ProjectionState.</value>
	public ProjectionState State
	{
		get
		{
			return projectionState;
		}
	}

	/// <summary>
	/// Perform a logistic interpolation between two matrices.
	/// </summary>
	/// <returns>The interpolated matrix.</returns>
	/// <param name="from">Matrix to transform from.</param>
	/// <param name="to">Matrix to transform to.</param>
	/// <param name="t">Interpolation percentage (0..1).</param>
	/// <param name="p">Logistic exponent.</param>
    private Matrix4x4 MatrixLogisticInterpolation(Matrix4x4 from, Matrix4x4 to, float t, float p)
    {
        Matrix4x4 result = new Matrix4x4();
        
        // Step function
        float e_p = Mathf.Exp(p);
        float e_pt = Mathf.Exp(p * t);
        
        t = (1.0f + e_p) * (e_pt - 1.0f) / ((e_p - 1.0f) * (1.0f + e_pt));

        for (int i = 0; i < 16; i++)
        {
            result[i] = from[i] + (to[i] - from[i]) * t;
        }

        return result;
    }

    private void BlendProjection(ProjectionState state, float t)
    {
		if (state == ProjectionState.Orthographic)
			projectionState = ProjectionState.TransitionToOrthographic;
		else if (state == ProjectionState.Perspective)
			projectionState = ProjectionState.TransitionToPerspective;

        StopAllCoroutines();
        StartCoroutine(BlendCoroutine(state, t));
    }

    private IEnumerator BlendCoroutine(ProjectionState state, float t) {

        float dt = 0;
		float p = 10.0f;
		float startTime = Time.time;

        while (Time.time - startTime < t)
        {
            dt = (Time.time - startTime) / t;
            dt = (state == ProjectionState.Perspective) ? 1.0f - dt : dt;

            camera.projectionMatrix = MatrixLogisticInterpolation(perspective, ortho, dt, p);

            yield return null;
        }

        camera.projectionMatrix = (state == ProjectionState.Perspective) ? perspective : ortho;

		projectionState = state;
    }
}
