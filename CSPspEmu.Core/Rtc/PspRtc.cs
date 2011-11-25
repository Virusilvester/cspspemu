﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Rtc
{
	public class PspRtc : PspEmulatorComponent
	{
		protected List<Tuple<DateTime, Action>> Timers = new List<Tuple<DateTime, Action>>();
		public DateTime StartDateTime;
		public DateTime CurrentDateTime;
		public TimeSpan Elapsed
		{
			get {
				return CurrentDateTime - StartDateTime;
			}
		}

		public DateTime UpdatedCurrentDateTime
		{
			get
			{
				Update();
				return CurrentDateTime;
			}
		}

		public uint UnixTimeStamp
		{
			get
			{
				return (uint)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
			}
		}

		public PspRtc(PspEmulatorContext PspEmulatorContext) : base (PspEmulatorContext)
		{
			Start();
		}

		public void Start()
		{
			this.StartDateTime = DateTime.UtcNow;
		}

		public void Update()
		{
			this.CurrentDateTime = DateTime.UtcNow;

			lock (Timers)
			{
			RetryLoop:
				foreach (var Timer in Timers)
				{
					if (this.CurrentDateTime >= Timer.Item1)
					{
						Timer.Item2();
						Timers.Remove(Timer);
						goto RetryLoop;
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="TimeSpan"></param>
		/// <param name="Callback"></param>
		public void RegisterTimerInOnce(TimeSpan TimeSpan, Action Callback)
		{
			RegisterTimerAtOnce(DateTime.UtcNow + TimeSpan, Callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DateTime"></param>
		/// <param name="Callback"></param>
		public void RegisterTimerAtOnce(DateTime DateTime, Action Callback)
		{
			lock (Timers)
			{
				//Console.WriteLine("RegisterTimerAtOnce:" + DateTime);
				Timers.Add(new Tuple<DateTime, Action>(DateTime, Callback));
			}
		}
	}
}