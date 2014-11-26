#region Header
//   Vorspire    _,-'/-'/  PollTimer.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;

using Server;
#endregion

namespace VitaNex
{
	[CoreService("PollTimer", "1.0.0.0", TaskPriority.Highest)]
	public sealed class PollTimer : Timer, IDisposable
	{
		public static PollTimer FromMilliseconds(
			double interval, Action callback, Func<bool> condition = null, bool autoStart = true)
		{
			return CreateInstance(TimeSpan.FromMilliseconds(interval), callback, condition, autoStart);
		}

		public static PollTimer FromMilliseconds<TObj>(
			double interval, Action<TObj> callback, TObj o, Func<bool> condition = null, bool autoStart = true)
		{
			return CreateInstance(TimeSpan.FromMilliseconds(interval), callback, o, condition, autoStart);
		}

		public static PollTimer FromSeconds(
			double interval, Action callback, Func<bool> condition = null, bool autoStart = true)
		{
			return CreateInstance(TimeSpan.FromSeconds(interval), callback, condition, autoStart);
		}

		public static PollTimer FromSeconds<TObj>(
			double interval, Action<TObj> callback, TObj o, Func<bool> condition = null, bool autoStart = true)
		{
			return CreateInstance(TimeSpan.FromSeconds(interval), callback, o, condition, autoStart);
		}

		public static PollTimer FromMinutes(
			double interval, Action callback, Func<bool> condition = null, bool autoStart = true)
		{
			return CreateInstance(TimeSpan.FromMinutes(interval), callback, condition, autoStart);
		}

		public static PollTimer FromMinutes<TObj>(
			double interval, Action<TObj> callback, TObj o, Func<bool> condition = null, bool autoStart = true)
		{
			return CreateInstance(TimeSpan.FromMinutes(interval), callback, o, condition, autoStart);
		}

		public static PollTimer FromHours(
			double interval, Action callback, Func<bool> condition = null, bool autoStart = true)
		{
			return CreateInstance(TimeSpan.FromHours(interval), callback, condition, autoStart);
		}

		public static PollTimer FromHours<TObj>(
			double interval, Action<TObj> callback, TObj o, Func<bool> condition = null, bool autoStart = true)
		{
			return CreateInstance(TimeSpan.FromHours(interval), callback, o, condition, autoStart);
		}

		public static PollTimer FromDays(double interval, Action callback, Func<bool> condition = null, bool autoStart = true)
		{
			return CreateInstance(TimeSpan.FromDays(interval), callback, condition, autoStart);
		}

		public static PollTimer FromDays<TObj>(
			double interval, Action<TObj> callback, TObj o, Func<bool> condition = null, bool autoStart = true)
		{
			return CreateInstance(TimeSpan.FromDays(interval), callback, o, condition, autoStart);
		}

		public static PollTimer FromTicks(long interval, Action callback, Func<bool> condition = null, bool autoStart = true)
		{
			return CreateInstance(TimeSpan.FromTicks(interval), callback, condition, autoStart);
		}

		public static PollTimer FromTicks<TObj>(
			long interval, Action<TObj> callback, TObj o, Func<bool> condition = null, bool autoStart = true)
		{
			return CreateInstance(TimeSpan.FromTicks(interval), callback, o, condition, autoStart);
		}

		public static PollTimer CreateInstance(
			TimeSpan interval, Action callback, Func<bool> condition = null, bool autoStart = true)
		{
			return new PollTimer(interval, callback, condition, autoStart);
		}

		public static PollTimer CreateInstance<TObj>(
			TimeSpan interval, Action<TObj> callback, TObj o, Func<bool> condition = null, bool autoStart = true)
		{
			return new PollTimer(interval, () => callback(o), condition, autoStart);
		}

		public bool IsDisposed { get; private set; }

		public bool IgnoreWorld { get; set; }

		public Func<bool> Condition { get; set; }
		public Action Callback { get; set; }

		public PollTimer(TimeSpan interval, Action callback, Func<bool> condition = null, bool autoStart = true)
			: base(interval, interval)
		{
			Condition = condition;
			Callback = callback;
			Running = autoStart;
		}

		~PollTimer()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (IsDisposed)
			{
				return;
			}

			IsDisposed = true;

			//GC.SuppressFinalize(this);

			Running = false;
			Condition = null;
			Callback = null;
		}

		/// <summary>
		///     Calls the protected PollTimer.OnTick method to force the PollTimer to tick without affecting its current state.
		/// </summary>
		public void Tick()
		{
			OnTick();
		}

		protected override void OnTick()
		{
			base.OnTick();

			if (Callback != null && (IgnoreWorld || (!World.Loading && !World.Saving)))
			{
				if (Condition != null)
				{
					if (VitaNexCore.TryCatchGet(Condition, VitaNexCore.ToConsole))
					{
						VitaNexCore.TryCatch(Callback, VitaNexCore.ToConsole);
					}
				}
				else
				{
					VitaNexCore.TryCatch(Callback, VitaNexCore.ToConsole);
				}
			}

			if (Interval <= TimeSpan.Zero)
			{
				Running = false;
			}
		}
	}
}