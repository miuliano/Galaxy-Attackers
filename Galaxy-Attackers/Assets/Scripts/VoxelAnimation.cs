using UnityEngine;
using System.Collections;

public class VoxelAnimation : MonoBehaviour {

	public delegate void VoxelAnimationHandler(VoxelAnimation animation);

	/// <summary>
	/// Models to use for each frame of the animation.
	/// </summary>
	public Transform[] frameModels;

	/// <summary>
	/// Time delay between voxel animation frames.
	/// </summary>
	public float frameDelay = 0.5f;
	
	/// <summary>
	/// Voxel model instances for the frames.
	/// </summary>
	public VoxelModel[] Frames
	{
		get
		{
			return frames;
		}
	}

	/// <summary>
	/// Gets the current frame of the animation.
	/// </summary>
	/// <value>The voxel model of the current frame.</value>
	public VoxelModel CurrentFrame
	{
		get
		{
			return frames[frameIndex];
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="VoxelAnimation"/> is hidden.
	/// </summary>
	/// <value><c>true</c> if hidden; otherwise, <c>false</c>.</value>
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
				// Hide all frames
				foreach(VoxelModel voxelModel in frames)
				{
					voxelModel.Hidden = value;
				}
			}
			else
			{
				// Show current frame
				CurrentFrame.Hidden = false;
			}

			isHidden = value;
		}
	}

	/// <summary>
	/// Occurs when the animation frame changes.
	/// </summary>
	public event VoxelAnimationHandler OnFrameChange;

	private VoxelModel[] frames;
	private int frameIndex;
	private float nextFrame;
	private bool isHidden;

	void Awake () {
		frameIndex = 0;
		nextFrame = frameDelay;
		isHidden = false;
		
		frames = new VoxelModel[frameModels.Length];

		for (int i = 0; i < frames.Length; i++)
		{
			GameObject frame = Instantiate(frameModels[i].gameObject) as GameObject;
			frame.transform.parent = transform;
			frame.transform.localPosition = Vector3.zero;
			
			frames[i] = frame.GetComponent<VoxelModel>();

			// Hide all but frame 0
			if (i != frameIndex)
				frames[i].Hidden = true;
			else
				frames[i].Hidden = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
		float frameTime = Time.time;
		
		if (frameTime > nextFrame)
		{
			// Only toggle frames if we aren't hidden
			if (isHidden == false && frames.Length > 1) {
				frames[frameIndex].Hidden = false;
				frames[(frameIndex - 1 >= 0 ? frameIndex - 1 : frames.Length - 1)].Hidden = true; 
			}

			frameIndex = (frameIndex + 1 <= frames.Length - 1 ? frameIndex + 1 : 0);
			
			nextFrame = frameTime + frameDelay;

			if (OnFrameChange != null)
			{
				OnFrameChange(this);
			}
		}
	}
}
