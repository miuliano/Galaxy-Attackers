using UnityEngine;
using System.Collections;

class Utility {

	public static void Shuffle<T>(T[] list)
	{
		for (int i = list.Length - 1; i >= 1; i--)
		{
			int j = Random.Range(0, i + 1);
			T temp = list[i];
			list[i] = list[j];
			list[j] = temp;
		}
	}
}
