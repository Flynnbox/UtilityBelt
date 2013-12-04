using System;
using System.Diagnostics;
using System.Threading;

namespace UtilityBelt
{
	public static class DebugProfiler
	{
		private static readonly Stopwatch stopwatch = new Stopwatch();
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		// Explicit static constructor to tell C# compiler
		// not to mark type as beforefieldinit
		static DebugProfiler()
		{
		}

		[Conditional("DEBUG")]
		public static void Start()
		{
			stopwatch.Start();
		}

		[Conditional("DEBUG")]
		public static void Stop()
		{
			stopwatch.Stop();
		}

		[Conditional("DEBUG")]
		public static void Reset()
		{
			stopwatch.Reset();
		}

		[Conditional("DEBUG")]
		public static void Log(string message)
		{
			log.DebugFormat(String.Format("{0}: {1}", message, GetElapsedTime())); //Console.WriteLine(String.Format("{0}: {1}", message, GetElapsedTime()));
		}

		[Conditional("DEBUG")]
		public static void Log()
		{
			log.Debug(GetElapsedTime()); //Console.WriteLine(String.Format("{0}", GetElapsedTime()));
		}

		public static long MillisecondsToCompleteRepititions(Action action, int iterations, bool performWarmup = false)
		{
			Stopwatch localStopwatch = new Stopwatch();
			SetProcessorAffinityAndThreadPriority();

			if (performWarmup)
			{
				Warmup(action, stopwatch);
			}

			stopwatch.Reset();
			stopwatch.Start();
			for (int repeat = 0; repeat < iterations; ++repeat)
			{
				localStopwatch.Reset();
				localStopwatch.Start();
				action();
				localStopwatch.Stop();
				Log("Ticks: " + localStopwatch.ElapsedTicks + " mS: " + localStopwatch.ElapsedMilliseconds);
			}
			stopwatch.Stop();
			Log("Total Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);
			return stopwatch.ElapsedMilliseconds;
		}

		public static long RepititionsWithinTimeLimit(Action action, int milliseconds, bool performWarmup = false)
		{
			SetProcessorAffinityAndThreadPriority();

			if (performWarmup)
			{
				Warmup(action, stopwatch);
			}

			long repititions = 0;
			stopwatch.Reset();
			stopwatch.Start();
			while (stopwatch.ElapsedMilliseconds < milliseconds)
			{
				action();
				repititions++;
			}
			stopwatch.Stop();
			Console.WriteLine("Total Repititions: " + repititions);
			return repititions;
		}

		private static void Warmup(Action action, Stopwatch stopwatch)
		{
			Log("Warmup Starts");
			stopwatch.Reset();
			stopwatch.Start();
			while (stopwatch.ElapsedMilliseconds < 1200) // A Warmup of 1000-1500 mS stabilizes the CPU cache and pipeline.
			{
				action();
			}
			stopwatch.Stop();
			Log("Warmup Ends");
		}

		private static void SetProcessorAffinityAndThreadPriority()
		{
			Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(2); // Uses the second Core or Processor for the Test
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High; // Prevents "Normal" processes from interrupting Threads
			Thread.CurrentThread.Priority = ThreadPriority.Highest; // Prevents "Normal" Threads from interrupting this thread
		}

		private static string GetElapsedTime()
		{
			TimeSpan ts = stopwatch.Elapsed;
			return String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
		}
	}
}