#if NET_4_6 || NET_STANDARD_2_0
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class AsyncTools
{
	public static int MainThreadId { get; private set; }
	public static SynchronizationContext MainThreadContext { get; private set; }

	private static Awaiter mainThreadAwaiter;
	private static Awaiter threadPoolAwaiter;
	private static Awaiter nextFrameAwaiter;

	[RuntimeInitializeOnLoadMethod]
	public static void Initialize()
	{
		MainThreadId = Thread.CurrentThread.ManagedThreadId;
		MainThreadContext = SynchronizationContext.Current;

		mainThreadAwaiter = new SynchronizationContextAwaiter(MainThreadContext);
		threadPoolAwaiter = new ThreadPoolContextAwaiter();
		nextFrameAwaiter = new NextFrameAwaiter();
	}

	public static void WhereAmI(string text)
	{
		if (IsMainThread())
		{
			Debug.Log($"{text}: main thread, frame: {Time.frameCount}");
		}
		else
		{
			Debug.Log($"{text}: background thread, id: {Thread.CurrentThread.ManagedThreadId}");
		}
	}

	/// <summary>
	/// Returns true if called from the Unity's main thread, and false otherwise.
	/// </summary>
	public static bool IsMainThread() => Thread.CurrentThread.ManagedThreadId == MainThreadId;

	/// <summary>
	/// Switches execution to a background thread.
	/// </summary>
	public static Awaiter ToThreadPool() => threadPoolAwaiter;

	/// <summary>
	/// Switches execution to the Update context of the main thread.
	/// </summary>
	public static Awaiter ToMainThread() => mainThreadAwaiter;

	/// <summary>
	/// Downloads a file as an array of bytes.
	/// </summary>
	/// <param name="address">File URL</param>
	/// <param name="cancellationToken">Optional cancellation token</param>
	public static Task<byte[]> DownloadAsBytesAsync(string address, CancellationToken cancellationToken = new CancellationToken())
	{
		return Task.Run(() =>
						{
							using (var webClient = new WebClient())
							{
								return webClient.DownloadData(address);
							}
						}, cancellationToken);
	}

	/// <summary>
	/// Downloads a file as a string.
	/// </summary>
	/// <param name="address">File URL</param>
	/// <param name="cancellationToken">Optional cancellation token</param>
	public static Task<string> DownloadAsStringAsync(string address, CancellationToken cancellationToken = new CancellationToken())
	{
		return Task.Run(() =>
						{
							using (var webClient = new WebClient())
							{
								return webClient.DownloadString(address);
							}
						}, cancellationToken);
	}

	/// <summary>
	/// Waits for specified number of seconds or until next frame.
	/// 
	/// If the argument is zero or negative, and if called from the main thread from Update or LateUpdate context,
	/// waits until next rendering frame.
	/// 
	/// If the argument is zero or negative, and if called from the main thread from FixedUpdate context,
	/// waits until next physics frame.
	/// </summary>
	/// <param name="seconds">If positive, number of seconds to wait</param>
	public static Awaiter GetAwaiter(this float seconds)
	{
		var context = SynchronizationContext.Current;
		if (seconds <= 0f && context != null)
		{
			return nextFrameAwaiter;
		}

		return new DelayAwaiter(seconds);
	}

	/// <summary>
	/// Waits for specified number of seconds or until next frame.
	/// 
	/// If the argument is zero or negative, and if called from the main thread from Update or LateUpdate context,
	/// waits until next rendering frame.
	/// 
	/// If the argument is zero or negative, and if called from the main thread from FixedUpdate context,
	/// waits until next physics frame.
	/// </summary>
	/// <param name="seconds">If positive, number of seconds to wait</param>
	public static Awaiter GetAwaiter(this int seconds) => GetAwaiter((float)seconds);

	/// <summary>
	/// Waits until all the tasks are completed.
	/// </summary>
	public static TaskAwaiter GetAwaiter(this IEnumerable<Task> tasks) => Task.WhenAll(tasks).GetAwaiter();

	public static Awaiter GetAwaiter(this SynchronizationContext context) => new SynchronizationContextAwaiter(context);

	/// <summary>
	/// Waits until the process exits.
	/// </summary>
	public static TaskAwaiter<int> GetAwaiter(this Process process)
	{
		var tcs = new TaskCompletionSource<int>();
		process.EnableRaisingEvents = true;
		process.Exited += (sender, eventArgs) => tcs.TrySetResult(process.ExitCode);
		if (process.HasExited)
		{
			tcs.TrySetResult(process.ExitCode);
		}
		return tcs.Task.GetAwaiter();
	}

	/// <summary>
	/// Waits for AsyncOperation completion
	/// </summary>
	public static Awaiter GetAwaiter(this AsyncOperation asyncOp) => new AsyncOperationAwaiter(asyncOp);

	#region Various awaiters

	public abstract class Awaiter : INotifyCompletion
	{
		public abstract bool IsCompleted { get; }
		public abstract void OnCompleted(Action action);
		public Awaiter GetAwaiter() => this;

		public void GetResult()
		{
		}
	}

	private class DelayAwaiter : Awaiter
	{
		private readonly SynchronizationContext context;
		private readonly float seconds;

		public DelayAwaiter(float seconds)
		{
			context = SynchronizationContext.Current;
			this.seconds = seconds;
		}

		public override bool IsCompleted => (seconds <= 0f);

		public override void OnCompleted(Action action)
		{
			Task.Delay((int)(seconds * 1000)).ContinueWith(prevTask =>
							   {
								   if (context != null)
								   {
									   context.Post(state => action(), null);
								   }
								   else
								   {
									   action();
								   }
							   });
		}
	}

	private class SynchronizationContextAwaiter : Awaiter
	{
		private readonly SynchronizationContext context;

		public SynchronizationContextAwaiter(SynchronizationContext context)
		{
			this.context = context;
		}

		public override bool IsCompleted => context == null || context == SynchronizationContext.Current;
		public override void OnCompleted(Action action) => context.Post(state => action(), null);
	}

	private class ThreadPoolContextAwaiter : Awaiter
	{
		public override bool IsCompleted => IsMainThread() == false;
		public override void OnCompleted(Action action) => ThreadPool.QueueUserWorkItem(state => action(), null);
	}

	private class NextFrameAwaiter : Awaiter
	{
		public override bool IsCompleted => false;
		public override void OnCompleted(Action action) => NextFrameHelper.Enqueue(action);
	}

	private class AsyncOperationAwaiter : Awaiter
	{
		private readonly AsyncOperation asyncOp;
		public AsyncOperationAwaiter(AsyncOperation asyncOp) => this.asyncOp = asyncOp;
		public override bool IsCompleted => asyncOp.isDone;
		public override void OnCompleted(Action action) => asyncOp.completed += _ => action();
	}
	#endregion
}
#endif