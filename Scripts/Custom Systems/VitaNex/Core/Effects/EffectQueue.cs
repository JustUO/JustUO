#region Header
//   Vorspire    _,-'/-'/  EffectQueue.cs
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
using System.Collections;
using System.Collections.Generic;

using Server;
#endregion

namespace VitaNex.FX
{
	public class EffectQueue<TEffectInfo> : IDisposable, IEnumerable<TEffectInfo>
		where TEffectInfo : EffectInfo
	{
		protected Timer DeferTimer { get; set; }

		public bool IsDisposed { get; private set; }

		public bool Processing { get; protected set; }
		public int Processed { get; protected set; }

		public Queue<TEffectInfo> Queue { get; private set; }

		public virtual Action<TEffectInfo> Handler { get; set; }
		public virtual Action<TEffectInfo> Mutator { get; set; }
		public virtual Action Callback { get; set; }

		public virtual bool Deferred { get; set; }

		public EffectQueue(Action callback = null, Action<TEffectInfo> handler = null, bool deferred = true)
		{
			Queue = new Queue<TEffectInfo>();
			Callback = callback;
			Handler = handler;
			Deferred = deferred;
		}

		public EffectQueue(int capacity, Action callback = null, Action<TEffectInfo> handler = null, bool deferred = true)
		{
			Queue = new Queue<TEffectInfo>(capacity);
			Callback = callback;
			Handler = handler;
			Deferred = deferred;
		}

		public EffectQueue(
			IEnumerable<TEffectInfo> queue, Action callback = null, Action<TEffectInfo> handler = null, bool deferred = true)
		{
			Queue = new Queue<TEffectInfo>(queue);
			Callback = callback;
			Handler = handler;
			Deferred = deferred;
		}

		~EffectQueue()
		{
			Dispose();
		}

		public virtual void Dispose()
		{
			if (IsDisposed)
			{
				return;
			}

			IsDisposed = true;

			//GC.SuppressFinalize(this);

			Queue.Free(true);
			Queue = null;
			Handler = null;
			Callback = null;

			Processed = 0;
			Processing = false;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Queue.GetEnumerator();
		}

		public virtual IEnumerator<TEffectInfo> GetEnumerator()
		{
			return Queue.GetEnumerator();
		}

		public void Add(TEffectInfo info)
		{
			Enqueue(info);
		}

		public virtual void Enqueue(TEffectInfo info)
		{
			if (!IsDisposed)
			{
				Queue.Enqueue(info);
			}
		}

		public virtual TEffectInfo Dequeue()
		{
			return Queue.Dequeue();
		}

		public virtual void Process()
		{
			if (IsDisposed /* || Processing*/)
			{
				return;
			}

			if (Queue.Count == 0)
			{
				OnProcessed();
				return;
			}

			Processing = true;

			TEffectInfo info = Dequeue();

			if (!OnProcess(info))
			{
				++Processed;
				Process();
				return;
			}

			++Processed;

			if (Deferred)
			{
				DeferTimer = Timer.DelayCall(
					GetDeferDelay(info),
					() =>
					{
						if (DeferTimer != null)
						{
							DeferTimer.Stop();
							DeferTimer = null;
						}

						Process();
					});
			}
			else
			{
				Process();
			}
		}

		protected virtual bool OnProcess(TEffectInfo info)
		{
			if (IsDisposed || info == null)
			{
				return false;
			}

			if (Mutator != null)
			{
				Mutator(info);
			}

			info.Send();

			if (Handler != null)
			{
				Handler(info);
			}

			return true;
		}

		protected virtual void OnProcessed()
		{
			if (IsDisposed)
			{
				return;
			}

			if (Callback != null)
			{
				Callback();
			}

			Processed = 0;
			Processing = false;

			Queue.Free(false);
		}

		public virtual TimeSpan GetDeferDelay(TEffectInfo info)
		{
			return !IsDisposed && info != null
					   ? TimeSpan.FromMilliseconds(info.Delay.TotalMilliseconds + (info.Duration * 100.0))
					   : TimeSpan.Zero;
		}
	}

	public class EffectQueue : EffectQueue<EffectInfo>
	{
		public EffectQueue(Action callback = null, Action<EffectInfo> handler = null, bool deferred = true)
			: base(callback, handler, deferred)
		{ }

		public EffectQueue(int capacity, Action callback = null, Action<EffectInfo> handler = null, bool deferred = true)
			: base(capacity, callback, handler, deferred)
		{ }

		public EffectQueue(
			IEnumerable<EffectInfo> queue, Action callback = null, Action<EffectInfo> handler = null, bool deferred = true)
			: base(queue, callback, handler, deferred)
		{ }
	}

	public class MovingEffectQueue : EffectQueue<MovingEffectInfo>
	{
		protected Timer DelayTimer { get; set; }

		public MovingEffectQueue(Action callback = null, Action<MovingEffectInfo> handler = null, bool deferred = true)
			: base(callback, handler, deferred)
		{ }

		public MovingEffectQueue(
			int capacity, Action callback = null, Action<MovingEffectInfo> handler = null, bool deferred = true)
			: base(capacity, callback, handler, deferred)
		{ }

		public MovingEffectQueue(
			IEnumerable<MovingEffectInfo> queue,
			Action callback = null,
			Action<MovingEffectInfo> handler = null,
			bool deferred = true)
			: base(queue, callback, handler, deferred)
		{ }

		protected override bool OnProcess(MovingEffectInfo info)
		{
			if (IsDisposed || info == null)
			{
				return false;
			}

			info.Send();

			if (Handler != null)
			{
				Timer.DelayCall(
					GetDeferDelay(info),
					() =>
					{
						if (Handler != null)
						{
							Handler(info);
						}
					});
			}

			return true;
		}

		public override TimeSpan GetDeferDelay(MovingEffectInfo info)
		{
			return info != null ? info.Delay + info.GetTravelTime() : TimeSpan.Zero;
		}
	}
}