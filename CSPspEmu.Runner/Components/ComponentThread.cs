﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CSharpUtils.Threading;
using CSPspEmu.Core;

namespace CSPspEmu.Runner.Components
{
	abstract public class ComponentThread : PspEmulatorComponent, IRunnableComponent
	{
		public bool Running = true;

		protected Thread ComponentThreadThread;
		protected AutoResetEvent StopCompleteEvent = new AutoResetEvent(false);
		protected AutoResetEvent PauseEvent = new AutoResetEvent(false);
		protected AutoResetEvent ResumeEvent = new AutoResetEvent(false);

		readonly public TaskQueue ThreadTaskQueue = new TaskQueue();
		abstract protected String ThreadName { get; }

		public void StartSynchronized()
		{
			Console.WriteLine("Component {0} StartSynchronized!", this);
			ComponentThreadThread = new Thread(() =>
			{
				ComponentThreadThread.Name = this.ThreadName;
				Thread.CurrentThread.CurrentCulture = new CultureInfo(PspConfig.CultureName);
				try
				{
					Main();
				}
				finally
				{
					Running = false;
					StopCompleteEvent.Set();
					Console.WriteLine("Component {0} Stopped!", this);
				}
			});
			ComponentThreadThread.Start();
			ThreadTaskQueue.EnqueueAndWaitCompleted(() =>
			{
			});
			Console.WriteLine("Component {0} Started!", this);
		}

		public void StopSynchronized()
		{
			Console.Write("Component {0} StopSynchronized...", this);
			if (Running)
			{
				StopCompleteEvent.Reset();
				{
					Running = false;
				}
				if (!StopCompleteEvent.WaitOne(1000))
				{
					Console.Error.WriteLine("Error stopping {0}", this);
					ComponentThreadThread.Abort();
				}
			}
			Console.WriteLine("Stopped!", this);
		}

		public void PauseSynchronized()
		{
			Console.WriteLine("Component {0} PauseSynchronized!", this);

			ThreadTaskQueue.EnqueueAndWaitStarted(() =>
			{
				while (!PauseEvent.WaitOne(TimeSpan.FromMilliseconds(10)))
				{
					if (!Running) break;
				}
			});
		}

		public void ResumeSynchronized()
		{
			Console.WriteLine("Component {0} ResumeSynchronized!", this);

			PauseEvent.Set();
		}

		abstract protected void Main();
	}
}