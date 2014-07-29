using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelModel : MonoBehaviour {

    public TextAsset voxelFile;
    public float voxelSize = 1.0f;
    public bool updateBoxCollider = false;

    private int[] voxelData;
    private int volumeWidth;
    private int volumeHeight;
    
    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private int quadCount;

    private bool flagUpdate = false;

    [ContextMenu ("Generate Mesh")]
    void GenerateMesh()
    {
        LoadVoxelData();
        UpdateMesh();
        UpdateBoxCollider();
    }

    private void UpdateBoxCollider()
    {
        if (!updateBoxCollider) return;

        BoxCollider box = GetComponent<BoxCollider>();

        box.size = new Vector3(volumeWidth * voxelSize, volumeHeight * voxelSize, voxelSize);
        box.center = new Vector3(box.size.x / 2.0f, -box.size.y / 2.0f, box.size.z / 2.0f);
    }

    private void LoadVoxelData()
    {
        if (voxelFile == null) return;

        string text = voxelFile.text;
        string[] lines = text.Split(new char[] {'\r','\n'});

        int index = 0;

        volumeWidth = 0;
        volumeHeight = 0;

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

                        index += 1;
                    }
                }
            }
        }
    }

    public bool GetVoxel(Vector3 point)
    {
        // Scale
        point = point / voxelSize;

        int x = Mathf.FloorToInt(point.x);
        int y = Mathf.Abs(Mathf.FloorToInt(point.y)) - 1;

        if (voxelData == null)
        {
            Debug.Log("VoxelModel::GetVoxel(): voxelData is null.");
            return false;
        }

        if (y * volumeWidth + x >= voxelData.Length)
        {
            Debug.Log("VoxelModel::GetVoxel(): Out of bounds.");
            return false;
        }

        return voxelData[y * volumeWidth + x] == 1 ? true : false;
    }

    public void SetVoxel(Vector3 point, int value)
    {
        // Scale
        point = point / voxelSize;

        int x = Mathf.FloorToInt(point.x);
        int y = Mathf.Abs(Mathf.FloorToInt(point.y)) - 1;

        if (voxelData == null)
        {
            Debug.Log("VoxelModel::SetVoxel(): voxelData is null.");
            return;
        }

        if (y * volumeWidth + x >= voxelData.Length)
        {
            Debug.Log("VoxelModel::SetVoxel(): Out of bounds.");
            return;
        }

        voxelData[y * volumeWidth + x] = value;

        flagUpdate = true;
    }

    public Vector3 GetVoxelCentre(Vector3 point)
    {
        float x = Mathf.Floor(point.x) + 0.5f;
        float y = Mathf.Floor(point.y) + 0.5f;
        float z = point.z + 0.5f;

        return new Vector3(x, y, z);
    }

    public void UpdateMesh()
    {
        // Have to use shared mesh here otherwise meshes leak
        mesh = GetComponent<MeshFilter>().sharedMesh;

        // Create mesh if missing
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "voxelMesh";
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        quadCount = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        int x = 0, y = 0;

        for (; y < volumeHeight; y++)
        {
            for (x = 0; x < volumeWidth; x++)
            {
                // Front-quad
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
                }
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

	// Use this for initialization
	void Start () {

        // Load data if missing
        if (voxelData == null)
        {
            LoadVoxelData();
        }

        // Create mesh if missing
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "voxelMesh";
            GetComponent<MeshFilter>().sharedMesh = mesh;

            UpdateMesh();
            UpdateBoxCollider();
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (flagUpdate == true)
        {
            flagUpdate = false;
            UpdateMesh();
            UpdateBoxCollider();
        }
	}

    /// <summary>
    /// Draw a front facing quad (-z direction).
    /// </summary>
    /// <param name="position">Position of the top left corner of the quad.</param>
    void QuadFront (Vector3 position)
    {
        QuadFront(position.x, position.y, position.z);
    }

    /// <summary>
    /// Draw a front facing quad (-z direction).
    /// </summary>
    /// <param name="x">x-coordinate of the top left corner of the quad.</param>
    /// <param name="y">y-coordinate of the top left corner of the quad.</param>
    /// <param name="z">z-coordinate of the top left corner of the quad.</param>
    void QuadFront(float x, float y, float z)
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

    /// <summary>
    /// Draw a back facing quad.
    /// </summary>
    /// <param name="x">x-coordinate of the top left corner of the quad.</param>
    /// <param name="y">y-coordinate of the top left corner of the quad.</param>
    /// <param name="z">z-coordinate of the top left corner of the quad.</param>
    void QuadBack(float x, float y, float z)
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
    void QuadTop(float x, float y, float z)
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

    void QuadBottom(float x, float y, float z)
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
