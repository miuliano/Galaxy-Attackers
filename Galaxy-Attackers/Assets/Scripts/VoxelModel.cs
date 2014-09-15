using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelModel : MonoBehaviour {

	public delegate void VoxelModelHandler(VoxelModel model);

	/// <summary>
	/// Text file containing voxel information.
	/// </summary>
    public TextAsset voxelFile;

	/// <summary>
	/// The size of each voxel.
	/// </summary>
    public float voxelSize = 1.0f;

	/// <summary>
	/// If set, positions the origin at the centre of the model.
	/// </summary>
	public bool centreOrigin = false;

	/// <summary>
	/// Gets a value indicating whether this instance has loaded.
	/// </summary>
	/// <value><c>true</c> if this instance has loaded; otherwise, <c>false</c>.</value>
	public bool Loaded
	{
		get
		{
			return isLoaded;
		}
	}

    /// <summary>
    /// Gets or sets a value indicating whether this instance is hidden.
    /// </summary>
    /// <value><c>true</c> if this instance is hidden; otherwise, <c>false</c>.</value>
	public bool Hidden
	{
		get
		{
			return isHidden;
		}
		set
		{
			if (value == true)
			{
				renderer.enabled = false;
			}
			else
			{
				renderer.enabled = true;
			}

			isHidden = value;
		}
	}

	/// <summary>
	/// Occurs when the model loads.
	/// </summary>
	public event VoxelModelHandler OnLoad;

    private int[] voxelData;
    private int volumeWidth;
    private int volumeHeight;
	private int voxelCount;

    private Mesh mesh;
	private int quadCount;
	private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();

	private bool isLoaded = false;
	private bool isHidden = false;
    private bool flagUpdate = false;

    [ContextMenu ("Preview")]
    private void Preview()
    {
        Initialize();
    }
	
    // Initialize before start(s) are called
	void Awake () {
		Initialize();
	}

    /// <summary>
    /// Initializes the voxel model.
    /// </summary>
    public void Initialize ()
    {
        LoadVoxelData();

        // Create mesh if missing
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "voxelMesh";
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        UpdateMesh();

		isLoaded = true;

		if (OnLoad != null)
		{
			OnLoad(this);
		}
    }

	// Update is called once per frame
	void Update () {
		if (flagUpdate == true)
		{
			flagUpdate = false;
			
			UpdateMesh();
		}
	}

	/// <summary>
	/// Returns the voxels in a intersected by b.
	/// </summary>
	/// <param name="a">The base voxel model.</param>
	/// <param name="b">The intersecting voxel model.</param>
	public static IntVector2[] Intersect(VoxelModel a, VoxelModel b)
	{
		List<IntVector2> intersections = new List<IntVector2>();

		Bounds bounds = a.GetWorldBounds();
		Vector3[] points = b.ToWorldPoints();

		foreach (Vector3 point in points)
		{
			if (bounds.Contains(point))
			{
				IntVector2 voxelPoint = a.WorldToVoxelSpace(point);

				if (a.GetVoxel(voxelPoint) > 0)
				{
					intersections.Add(voxelPoint);
				}
			}
		}

		return intersections.ToArray();
	}
	
    /// <summary>
    /// Returns the bounds of the voxel model in local space.
    /// </summary>
    /// <returns>Bounds of the voxel model.</returns>
    public Bounds GetLocalBounds()
    {
        Bounds bounds = new Bounds();

        // Place box collider origin at the centre of the model
        if (centreOrigin)
        {
            bounds.center = Vector3.zero;
        }
        else
        {
            bounds.center = new Vector3(voxelSize * volumeWidth / 2.0f, -voxelSize * volumeHeight / 2.0f, voxelSize / 2.0f);
        }

        bounds.size = new Vector3(volumeWidth * voxelSize, volumeHeight * voxelSize, voxelSize);

        return bounds;
    }

	/// <summary>
	/// Returns the bounds of the voxel model in world space.
	/// </summary>
	/// <returns>The world bounds.</returns>
	public Bounds GetWorldBounds()
	{
		Bounds bounds = GetLocalBounds();
		bounds.center = transform.TransformPoint(bounds.center);
		return bounds;
	}

	/// <summary>
	/// Load voxel data from the current file.
	/// </summary>
    public void LoadVoxelData()
    {
        if (voxelFile == null) 
		{
			Debug.LogWarning("VoxelModel::LoadVoxelData(): No voxel file specified.");
			return;
		}

		// Slurp and split
        string text = voxelFile.text;
        string[] lines = text.Split(new char[] {'\r','\n'});

        volumeWidth = 0;
        volumeHeight = 0;

		int index = 0;

        foreach (string line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;

            if (volumeWidth == 0)
            {
                // Assume first line is width
                volumeWidth = int.Parse(line);
            }
            else if (volumeHeight == 0)
            {
                // Assume second line is height
                volumeHeight = int.Parse(line);

                voxelData = new int[volumeWidth * volumeHeight];
            }
            else
            {
                // Assume remaining data are voxel rows
                string[] voxels = line.Split(new char[] { ' ' });

                foreach (string voxel in voxels)
                {
                    if (index < voxelData.Length)
                    {
                        voxelData[index] = int.Parse(voxel);

                        index++;
                    }
                }
            }
        }
    }

	/// <summary>
	/// Convert a point in world space to voxel space.
	/// </summary>
	/// <returns>The to voxel space.</returns>
	/// <param name="point">Point.</param>
	public IntVector2 WorldToVoxelSpace(Vector3 point)
	{
		return LocalToVoxelSpace(transform.InverseTransformPoint(point));
	}

    /// <summary>
    /// Convert a point in local space to voxel space.
    /// </summary>
    /// <param name="point">Point in world space.</param>
    /// <returns>Point in voxel space.</returns>
    public IntVector2 LocalToVoxelSpace(Vector3 point)
    {
        Vector3 offset = Vector3.zero;

        if (centreOrigin)
        {
            offset = new Vector3(-voxelSize * volumeWidth / 2.0f, voxelSize * volumeHeight / 2.0f, -voxelSize / 2.0f);
        }

        // Convert to voxel coordinates
        Vector3 voxPoint = (point - offset) / voxelSize;

        int x = Mathf.FloorToInt(voxPoint.x);
        int y = (-Mathf.FloorToInt(voxPoint.y)) - 1;

        return new IntVector2(x, y);
    }

	/// <summary>
	/// Converts a point in voxel space to world space.
	/// </summary>
	/// <returns>The point in world space.</returns>
	/// <param name="point">Point.</param>
	public Vector3 VoxelToWorldSpace(IntVector2 point)
	{
		return transform.TransformPoint(VoxelToLocalSpace(point));
	}

	/// <summary>
	/// Converts a point in voxel space to world space.
	/// </summary>
	/// <returns>Point in world space.</returns>
	/// <param name="x">X component of the point.</param>
	/// <param name="y">Y component of the point.</param>
	public Vector3 VoxelToWorldSpace(int x, int y)
	{
		return transform.TransformPoint(VoxelToLocalSpace(x, y));
	}

    /// <summary>
    /// Converts a point in voxel space to local space.
    /// </summary>
    /// <param name="point">Point in voxel space.</param>
    /// <returns>Point in local space.</returns>
    public Vector3 VoxelToLocalSpace(IntVector2 point)
    {
        return VoxelToLocalSpace(point.x, point.y);
    }

    /// <summary>
    /// Converts a point in voxel space to local space.
    /// </summary>
    /// <param name="x">X component of the point.</param>
    /// <param name="y">Y component of the point.</param>
    /// <returns>Point in local space.</returns>
    public Vector3 VoxelToLocalSpace(int x, int y)
    {
        Vector3 offset = Vector3.zero;

        if (centreOrigin)
        {
            offset = new Vector3(-voxelSize * volumeWidth / 2.0f, voxelSize * volumeHeight / 2.0f, -voxelSize / 2.0f);
        }

        return new Vector3(x * voxelSize + voxelSize / 2.0f + offset.x, -y * voxelSize - voxelSize / 2.0f + offset.y, voxelSize / 2.0f + offset.z);
    }

    /// <summary>
    /// Return the value of a voxel at a given point.
    /// </summary>
    /// <param name="point">Point in world space.</param>
    /// <returns>The value of the voxel.</returns>
    public int GetVoxel(Vector3 point)
    {
        return GetVoxel(LocalToVoxelSpace(point));
    }

	/// <summary>
	/// Return the value of a voxel at a given point.
	/// </summary>
	/// <param name="point">Point in voxel space.</param>
    /// <returns>The value of the voxel.</returns>
    public int GetVoxel(IntVector2 point)
    {
        return GetVoxel(point.x, point.y);
    }

    /// <summary>
    /// Returns the value of a voxel at a given point.
    /// </summary>
    /// <param name="x">X component of the point in voxel space.</param>
    /// <param name="y">Y component of the point in voxel space.</param>
    /// <returns>The value of the voxel.</returns>
    public int GetVoxel(int x, int y)
    {
        // Bounds check
        if (x < 0 || x >= volumeWidth ||
            y < 0 || y >= volumeHeight)
        {
            Debug.LogWarning("VoxelModel::GetVoxel(): Out of bounds");

            return -1;
        }

        return voxelData[y * volumeWidth + x];
    }

	/// <summary>
	/// Set the value of a voxel.
	/// </summary>
	/// <param name="point">Point in world space.</param>
	/// <param name="value">Value of the voxel.</param>
    public void SetVoxel(Vector3 point, int value)
    {        
        SetVoxel(LocalToVoxelSpace(point), value);
    }

    /// <summary>
    /// Set the value of a voxel.
    /// </summary>
    /// <param name="point">Point in voxel space.</param>
    /// <param name="value">Value of the voxel.</param>
    public void SetVoxel(IntVector2 point, int value)
    {
        SetVoxel(point.x, point.y, value);
    }

    /// <summary>
    /// Set the value of a voxel.
    /// </summary>
    /// <param name="x">X component of the point in voxel space.</param>
    /// <param name="y">Y component of the point in voxel space.</param>
    /// <param name="value">Value of the voxel.</param>
    public void SetVoxel(int x, int y, int value)
    {
        // Bounds check
        if (x < 0 || x >= volumeWidth ||
            y < 0 || y >= volumeHeight)
        {
            Debug.LogWarning("VoxelModel::SetVoxel(): Out of bounds");

            return;
        }

        voxelData[y * volumeWidth + x] = value;

        flagUpdate = true;
    }

	/// <summary>
	/// Returns the centre points of the voxels in world space.
	/// </summary>
	/// <returns>Centre points of the voxels.</returns>
	public Vector3[] ToWorldPoints()
	{
		Vector3[] points = new Vector3[voxelCount];
		
		int index = 0;
		int x = 0, y = 0;
		
		for (; y < volumeHeight; y++)
		{
			for (x = 0; x < volumeWidth; x++)
			{
				if (voxelData[y * volumeWidth + x] > 0)
				{
					points[index] = VoxelToWorldSpace(x, y);
					index++;
				}
			}
		}
		
		return points;

	}

	/// <summary>
	/// Returns the centre points of voxels in local space.
	/// </summary>
	/// <returns>Centre points of the voxels.</returns>
	public Vector3[] ToLocalPoints()
	{
		Vector3[] points = new Vector3[voxelCount];

		int index = 0;
		int x = 0, y = 0;
		
		for (; y < volumeHeight; y++)
		{
			for (x = 0; x < volumeWidth; x++)
			{
				if (voxelData[y * volumeWidth + x] > 0)
				{
                    points[index] = VoxelToLocalSpace(x, y);
					index++;
				}
			}
		}

		return points;
	}

	/// <summary>
	/// Updates the mesh with the current voxel data.
	/// </summary>
    public void UpdateMesh()
    {
		if (mesh == null)
		{
			Debug.LogWarning("VoxelModel::UpdateMesh(): No mesh found.");
			return;
		}

		// Clear/reset everything
		voxelCount = 0;
        quadCount  = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        int x = 0, y = 0;

        for (; y < volumeHeight; y++)
        {
            for (x = 0; x < volumeWidth; x++)
            {
                // Handle filled voxels
                if (voxelData[y * volumeWidth + x] > 0)
                {
                    Vector3 point = VoxelToLocalSpace(x, y);

                    // Always draw front
                    QuadFront(point.x, point.y, point.z);
					
					// Always draw back
                    QuadBack(point.x, point.y, point.z);

                    // Left-quad
                    if (x == 0 || (x > 0 && voxelData[y * volumeWidth + x - 1] == 0))
                        QuadLeft(point.x, point.y, point.z);

                    // Right-quad
                    if (x == volumeWidth - 1 || (x < volumeWidth - 1 && voxelData[y * volumeWidth + x + 1] == 0))
                        QuadRight(point.x, point.y, point.z);

                    // Top-quad
                    if (y == 0 || (y > 0 && voxelData[(y - 1) * volumeWidth + x] == 0))
                        QuadTop(point.x, point.y, point.z);

                    // Bottom-quad
                    if (y == volumeHeight - 1 || (y < volumeHeight - 1 && voxelData[(y + 1) * volumeWidth + x] == 0))
                        QuadBottom(point.x, point.y, point.z);   

					voxelCount++;
                }
            }
        }

		mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.Optimize();
        mesh.RecalculateNormals();
		mesh.RecalculateBounds();
    }

	/// <summary>
	/// Draw a front facing quad.
	/// </summary>
	/// <param name="x">X component of the centre of the cube.</param>
	/// <param name="y">Y component of the centre of the cube.</param>
	/// <param name="z">Z component of the centre of the cube.</param>
    private void QuadFront(float x, float y, float z)
    {
		float halfSize = voxelSize / 2.0f;
        vertices.Add(new Vector3(x + halfSize, y - halfSize, z - halfSize));
        vertices.Add(new Vector3(x - halfSize, y - halfSize, z - halfSize));
		vertices.Add(new Vector3(x - halfSize, y + halfSize, z - halfSize));
        vertices.Add(new Vector3(x + halfSize, y + halfSize, z - halfSize));

        triangles.Add(quadCount * 4);
        triangles.Add(quadCount * 4 + 1);
        triangles.Add(quadCount * 4 + 3);
        triangles.Add(quadCount * 4 + 1);
        triangles.Add(quadCount * 4 + 2);
        triangles.Add(quadCount * 4 + 3);

        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(1.0f, 0.0f));
        uvs.Add(new Vector2(1.0f, 1.0f));
        uvs.Add(new Vector2(0.0f, 1.0f));        

        quadCount += 1;
    }

	/// <summary>
	/// Draw a back facing quad.
	/// </summary>
	/// <param name="x">X component of the centre of the cube.</param>
	/// <param name="y">Y component of the centre of the cube.</param>
	/// <param name="z">Z component of the centre of the cube.</param>
	private void QuadBack(float x, float y, float z)
    {
		float halfSize = voxelSize / 2.0f;
		vertices.Add(new Vector3(x + halfSize, y - halfSize, z + halfSize));
		vertices.Add(new Vector3(x - halfSize, y - halfSize, z + halfSize));
		vertices.Add(new Vector3(x - halfSize, y + halfSize, z + halfSize));
		vertices.Add(new Vector3(x + halfSize, y + halfSize, z + halfSize));

        triangles.Add(quadCount * 4);
        triangles.Add(quadCount * 4 + 3);
        triangles.Add(quadCount * 4 + 1);
        triangles.Add(quadCount * 4 + 1);
        triangles.Add(quadCount * 4 + 3);
        triangles.Add(quadCount * 4 + 2);

        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(1.0f, 0.0f));
        uvs.Add(new Vector2(1.0f, 1.0f));
        uvs.Add(new Vector2(0.0f, 1.0f));

        quadCount += 1;
    }

    /// <summary>
    /// Draw a top facing quad.
    /// </summary>
    /// <param name="x">X component of the centre of the cube.</param>
	/// <param name="y">Y component of the centre of the cube.</param>
	/// <param name="z">Z component of the centre of the cube.</param>
	private void QuadTop(float x, float y, float z)
    {
		float halfSize = voxelSize / 2.0f;
		vertices.Add(new Vector3(x + halfSize, y + halfSize, z - halfSize));
		vertices.Add(new Vector3(x + halfSize, y + halfSize, z + halfSize));
		vertices.Add(new Vector3(x - halfSize, y + halfSize, z + halfSize));
		vertices.Add(new Vector3(x - halfSize, y + halfSize, z - halfSize));

        triangles.Add(quadCount * 4);
        triangles.Add(quadCount * 4 + 3);
        triangles.Add(quadCount * 4 + 1);
        triangles.Add(quadCount * 4 + 1);
        triangles.Add(quadCount * 4 + 3);
        triangles.Add(quadCount * 4 + 2);

        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(1.0f, 0.0f));
        uvs.Add(new Vector2(1.0f, 1.0f));
        uvs.Add(new Vector2(0.0f, 1.0f));

        quadCount += 1;
    }

	/// <summary>
	/// Draw a bottom facing quad.
	/// </summary>
	/// <param name="x">X component of the centre of the cube.</param>
	/// <param name="y">Y component of the centre of the cube.</param>
	/// <param name="z">Z component of the centre of the cube.</param>
	private void QuadBottom(float x, float y, float z)
    {
		float halfSize = voxelSize / 2.0f;
		vertices.Add(new Vector3(x + halfSize, y - halfSize, z - halfSize));
		vertices.Add(new Vector3(x + halfSize, y - halfSize, z + halfSize));
		vertices.Add(new Vector3(x - halfSize, y - halfSize, z + halfSize));
		vertices.Add(new Vector3(x - halfSize, y - halfSize, z - halfSize));

        triangles.Add(quadCount * 4);
        triangles.Add(quadCount * 4 + 1);
        triangles.Add(quadCount * 4 + 3);
        triangles.Add(quadCount * 4 + 1);
        triangles.Add(quadCount * 4 + 2);
        triangles.Add(quadCount * 4 + 3);

        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(1.0f, 0.0f));
        uvs.Add(new Vector2(1.0f, 1.0f));
        uvs.Add(new Vector2(0.0f, 1.0f));

        quadCount += 1;
    }

	/// <summary>
	/// Draw a left facing quad.
	/// </summary>
	/// <param name="x">X component of the centre of the cube.</param>
	/// <param name="y">Y component of the centre of the cube.</param>
	/// <param name="z">Z component of the centre of the cube.</param>
	private void QuadLeft(float x, float y, float z)
    {
		float halfSize = voxelSize / 2.0f;
		vertices.Add(new Vector3(x - halfSize, y - halfSize, z - halfSize));
		vertices.Add(new Vector3(x - halfSize, y + halfSize, z - halfSize));
		vertices.Add(new Vector3(x - halfSize, y + halfSize, z + halfSize));
		vertices.Add(new Vector3(x - halfSize, y - halfSize, z + halfSize));

        triangles.Add(quadCount * 4);
        triangles.Add(quadCount * 4 + 3);
        triangles.Add(quadCount * 4 + 1);
        triangles.Add(quadCount * 4 + 1);
        triangles.Add(quadCount * 4 + 3);
        triangles.Add(quadCount * 4 + 2);

        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(1.0f, 0.0f));
        uvs.Add(new Vector2(1.0f, 1.0f));
        uvs.Add(new Vector2(0.0f, 1.0f));

        quadCount += 1;
    }

	/// <summary>
	/// Draw a right facing quad.
	/// </summary>
	/// <param name="x">X component of the centre of the cube.</param>
	/// <param name="y">Y component of the centre of the cube.</param>
	/// <param name="z">Z component of the centre of the cube.</param>
    private void QuadRight(float x, float y, float z)
    {
		float halfSize = voxelSize / 2.0f;
		vertices.Add(new Vector3(x + halfSize, y - halfSize, z - halfSize));
		vertices.Add(new Vector3(x + halfSize, y + halfSize, z - halfSize));
		vertices.Add(new Vector3(x + halfSize, y + halfSize, z + halfSize));
		vertices.Add(new Vector3(x + halfSize, y - halfSize, z + halfSize));

        triangles.Add(quadCount * 4);
        triangles.Add(quadCount * 4 + 1);
        triangles.Add(quadCount * 4 + 3);
        triangles.Add(quadCount * 4 + 1);
        triangles.Add(quadCount * 4 + 2);
        triangles.Add(quadCount * 4 + 3);

        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(1.0f, 0.0f));
        uvs.Add(new Vector2(1.0f, 1.0f));
        uvs.Add(new Vector2(0.0f, 1.0f));

        quadCount += 1;
    }
}
