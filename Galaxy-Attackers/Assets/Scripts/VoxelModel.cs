﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelModel : MonoBehaviour {

	/// <summary>
	/// Text file containing voxel information.
	/// </summary>
    public TextAsset voxelFile;

	/// <summary>
	/// The size of each voxel.
	/// </summary>
    public float voxelSize = 1.0f;

	/// <summary>
	/// If set, updates the attached box collider when generated.
	/// </summary>
    public bool updateBoxCollider = false;

    private int[] voxelData;
    private int volumeWidth;
    private int volumeHeight;
	private int voxelCount;
	
    private Mesh mesh;
	private int quadCount;
	private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();

    private bool flagUpdate = false;

    [ContextMenu ("Generate Mesh")]
    private void GenerateMesh()
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

		if (updateBoxCollider)
        	UpdateBoxCollider();
    }

    private void UpdateBoxCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

		if (boxCollider == null) {
			Debug.LogWarning("VoxelModel::UpdateBoxCollider(): No box collider component found.");
			return;
		}

        boxCollider.size   = new Vector3(volumeWidth * voxelSize, volumeHeight * voxelSize, voxelSize);
        boxCollider.center = new Vector3(boxCollider.size.x / 2.0f, -boxCollider.size.y / 2.0f, boxCollider.size.z / 2.0f);
    }

	/// <summary>
	/// Load voxel data from a file.
	/// </summary>
	/// <param name="file">File containing voxel data</param>
	public void LoadVoxelData(TextAsset file)
	{
		voxelFile = file;
		LoadVoxelData();
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
        string text    = voxelFile.text;
        string[] lines = text.Split(new char[] {'\r','\n'});

        volumeWidth = 0;
        volumeHeight = 0;

		int index = 0;

        foreach (string line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;

            if (volumeWidth == 0)
            {
                volumeWidth = int.Parse(line);
            }
            else if (volumeHeight == 0)
            {
                volumeHeight = int.Parse(line);

                voxelData = new int[volumeWidth * volumeHeight];
            }
            else
            {
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
	/// Return the value of a voxel at a given point.
	/// </summary>
	/// <returns>The value of the voxel.</returns>
	/// <param name="point">Point in local coordinates.</param>
    public int GetVoxel(Vector3 point)
    {
		// Check voxel data has been loaded
		if (voxelData == null)
		{
			Debug.LogWarning("VoxelModel::GetVoxel(): No voxel is null.");
			return -1;
		}

        // Scale
        point = point / voxelSize;

        int x = Mathf.FloorToInt(point.x);
        int y = Mathf.Abs(Mathf.FloorToInt(point.y)) - 1;

		// Bounds check
        if (y * volumeWidth + x >= voxelData.Length)
        {
            Debug.LogWarning("VoxelModel::GetVoxel(): Out of bounds.");
            return -1;
        }

        return voxelData[y * volumeWidth + x];
    }

	/// <summary>
	/// Set the value of a voxel.
	/// </summary>
	/// <param name="point">Local point to set the voxel at.</param>
	/// <param name="value">Value of the voxel.</param>
    public void SetVoxel(Vector3 point, int value)
    {
		// Check voxel data exists
		if (voxelData == null)
		{
			Debug.LogWarning("VoxelModel::SetVoxel(): voxelData is null.");
			return;
		}

        // Scale
        point = point / voxelSize;

        int x = Mathf.FloorToInt(point.x);
        int y = Mathf.Abs(Mathf.FloorToInt(point.y)) - 1;

		// Bounds check, as well as point in range check
        if (y * volumeWidth + x >= voxelData.Length || x < 0 || y < 0)
        {
            Debug.LogWarning("VoxelModel::SetVoxel(): Out of bounds.");
            return;
        }

        voxelData[y * volumeWidth + x] = value;

        flagUpdate = true;
    }

	/// <summary>
	/// Returns the filled voxels as centre points.
	/// </summary>
	/// <returns>Centre points of the voxels.</returns>
	public Vector3[] ToPoints()
	{
		Vector3[] points = new Vector3[voxelCount];

		int index = 0;
		int x = 0, y = 0;
		
		for (; y < volumeHeight; y++)
		{
			for (x = 0; x < volumeWidth; x++)
			{
				if (voxelData[y * volumeWidth + x] == 1)
				{
					points[index] = new Vector3(x * voxelSize + voxelSize / 2.0f, -y * voxelSize - voxelSize / 2.0f,  voxelSize / 2.0f);
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
                if (voxelData[y * volumeWidth + x] == 1)
                {
                    // Always draw front
                    QuadFront(x * voxelSize, -y * voxelSize, 0.0f);

                    // Always draw back
                    QuadBack(x * voxelSize, -y * voxelSize, 0.0f);

                    // Left-quad
                    if (x == 0 || (x > 0 && voxelData[y * volumeWidth + x - 1] == 0))
                        QuadLeft(x * voxelSize, -y * voxelSize, 0.0f);

                    // Right-quad
                    if (x == volumeWidth - 1 || (x < volumeWidth - 1 && voxelData[y * volumeWidth + x + 1] == 0))
                        QuadRight(x * voxelSize, -y * voxelSize, 0.0f);

                    // Top-quad
                    if (y == 0 || (y > 0 && voxelData[(y - 1) * volumeWidth + x] == 0))
                        QuadTop(x * voxelSize, -y * voxelSize, 0.0f);

                    // Bottom-quad
                    if (y == volumeHeight - 1 || (y < volumeHeight - 1 && voxelData[(y + 1) * volumeWidth + x] == 0))
                        QuadBottom(x * voxelSize, -y * voxelSize, 0.0f);   

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

	// Use this for initialization
	void Start () {

        // Load data if missing
        if (voxelData == null)
            LoadVoxelData();

        // Create mesh if missing
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "voxelMesh";
            GetComponent<MeshFilter>().sharedMesh = mesh;

            UpdateMesh();

			if (updateBoxCollider)
            	UpdateBoxCollider();
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (flagUpdate == true)
        {
            flagUpdate = false;

            UpdateMesh();

			if (updateBoxCollider)
            	UpdateBoxCollider();
        }
	}

    private void QuadFront(float x, float y, float z)
    {
        vertices.Add(new Vector3(x    , y    , z    ));
        vertices.Add(new Vector3(x + voxelSize, y, z));
        vertices.Add(new Vector3(x + voxelSize, y - voxelSize, z));
        vertices.Add(new Vector3(x, y - voxelSize, z));

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

	private void QuadBack(float x, float y, float z)
    {
        vertices.Add(new Vector3(x, y, z + voxelSize));
        vertices.Add(new Vector3(x + voxelSize, y, z + voxelSize));
        vertices.Add(new Vector3(x + voxelSize, y - voxelSize, z + voxelSize));
        vertices.Add(new Vector3(x, y - voxelSize, z + voxelSize));

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
    /// <param name="x">x-coordinate of the top left corner of the quad.</param>
    /// <param name="y">y-coordinate of the top left corner of the quad.</param>
    /// <param name="z">z-coordinate of the top left corner of the quad.</param>
	private void QuadTop(float x, float y, float z)
    {
        vertices.Add(new Vector3(x, y, z));
        vertices.Add(new Vector3(x + voxelSize, y, z));
        vertices.Add(new Vector3(x + voxelSize, y, z + voxelSize));
        vertices.Add(new Vector3(x, y, z + voxelSize));

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

	private void QuadBottom(float x, float y, float z)
    {
        vertices.Add(new Vector3(x, y - voxelSize, z));
        vertices.Add(new Vector3(x + voxelSize, y - voxelSize, z));
        vertices.Add(new Vector3(x + voxelSize, y - voxelSize, z + voxelSize));
        vertices.Add(new Vector3(x, y - voxelSize, z + voxelSize));

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

	private void QuadLeft(float x, float y, float z)
    {
        vertices.Add(new Vector3(x, y, z));
        vertices.Add(new Vector3(x, y, z + voxelSize));
        vertices.Add(new Vector3(x, y - voxelSize, z + voxelSize));
        vertices.Add(new Vector3(x, y - voxelSize, z));

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

    private void QuadRight(float x, float y, float z)
    {
        vertices.Add(new Vector3(x + voxelSize, y, z));
        vertices.Add(new Vector3(x + voxelSize, y, z + voxelSize));
        vertices.Add(new Vector3(x + voxelSize, y - voxelSize, z + voxelSize));
        vertices.Add(new Vector3(x + voxelSize, y - voxelSize, z));

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
