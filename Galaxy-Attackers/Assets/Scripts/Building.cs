using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour {

    public Transform buildingModel;

    private BoxCollider boxCollider;

    [ContextMenu("Preview")]
    void Preview()
    {
        buildingModel.GetComponent<VoxelModel>().Initialize();
    }

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();

        Bounds bounds = buildingModel.GetComponent<VoxelModel>().GetBounds();
        boxCollider.center = bounds.center;
        boxCollider.size = bounds.size;
    }
}
