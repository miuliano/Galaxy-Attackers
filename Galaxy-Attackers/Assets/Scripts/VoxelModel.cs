using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelModel : MonoBehaviour {

    public TextAsset voxelFile;
    public float voxelSize = 1.0f;

    private int[] voxelData;
    private int volumeWidth;
    private int volumeHeight;
    
    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private int quadCount;

    [ContextMenu ("Generate Mesh")]
    void GenerateMesh()
    {
        // Have to use shared mesh here otherwise meshes leak
        mesh = GetComponent<MeshFilter>().sharedMesh;

        if (mesh == null)
        {            
            mesh = new Mesh();
            mesh.name = "voxelMesh";

            GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        LoadVoxelData();
        UpdateMesh();
    }

    void LoadVoxelData()
    {
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

                Debug.Log("volumeWidth = " + volumeWidth);
            }
            else if (volumeHeight == 0)
            {
                volumeHeight = int.Parse(line);

                Debug.Log("volumeHeight = " + volumeHeight);

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

                        Debug.Log("voxelData[" + index + "] = " + voxelData[index]);

                        index += 1;
                    }
                }
            }
        }

        Debug.Log("voxel data = " + voxelData.ToString());
    }

    public void UpdateMesh()
    {
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

                    // Always draw front
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
    }

    

	// Use this for initialization
	void Start () {
        mesh = GetComponent<MeshFilter>().sharedMesh;
	}
	
	// Update is called once per frame
	void Update () {
	
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
    /// <param name="x">X-coordinate of the top left corner of the quad.</param>
    /// <param name="y">Y-coordinate of the top left corner of the quad.</param>
    /// <param name="z">Z-coordinate of the top left corner of the quad.</param>
    void QuadFront(float x, float y, float z)
    {
        Debug.Log(string.Format("QuadFront x: {0}, y: {1}, z: {2}", x, y, z));

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

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        quadCount += 1;
    }

    void QuadBack(float x, float y, float z)
    {
        Debug.Log(string.Format("QuadBack x: {0}, y: {1}, z: {2}", x, y, z));

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

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        quadCount += 1;
    }

    void QuadTop(float x, float y, float z)
    {
        Debug.Log(string.Format("QuadTop x: {0}, y: {1}, z: {2}", x, y, z));

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

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        quadCount += 1;
    }

    void QuadBottom(float x, float y, float z)
    {
        Debug.Log(string.Format("QuadBottom x: {0}, y: {1}, z: {2}", x, y, z));

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

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        quadCount += 1;
    }

    private void QuadLeft(float x, float y, float z)
    {
        Debug.Log(string.Format("QuadLeft x: {0}, y: {1}, z: {2}", x, y, z));

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

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        quadCount += 1;
    }

    private void QuadRight(float x, float y, float z)
    {
        Debug.Log(string.Format("QuadRight x: {0}, y: {1}, z: {2}", x, y, z));

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

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        quadCount += 1;
    }
}
