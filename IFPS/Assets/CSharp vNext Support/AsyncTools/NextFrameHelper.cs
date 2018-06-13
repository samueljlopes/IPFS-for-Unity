#if NET_4_6 || NET_STANDARD_2_0
using System;
using System.Collections.Generic;
using UnityEngine;

public class NextFrameHelper : MonoBehaviour
{
	private struct Job
	{
		public int frame;
		public Action action;
	}

	private static readonly Queue<Job> queue = new Queue<Job>();

	[RuntimeInitializeOnLoadMethod]
	private static void Initialize()
	{
		var go = new GameObject();
		go.hideFlags = HideFlags.HideAndDontSave;
		go.AddComponent<NextFrameHelper>();
		DontDestroyOnLoad(go);
	}

	public static void Enqueue(Action action) => queue.Enqueue(new Job { frame = Time.frameCount, action = action });

	private void Update()
	{
		int currentFrame = Time.frameCount;
		while (queue.Count > 0)
		{
			var job = queue.Peek();
			if (job.frame == currentFrame)
			{
				break;
			}
			queue.Dequeue();
			job.action();
		}
	}
}
#endif