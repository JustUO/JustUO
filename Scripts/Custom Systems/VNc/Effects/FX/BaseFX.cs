#region Header
//   Vorspire    _,-'/-'/  BaseFX.cs
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
using System.Collections.Generic;
using System.Threading.Tasks;

using Server;
#endregion

namespace VitaNex.FX
{
	public abstract class BaseEffect<TQueue, TEffectInfo> : List<TQueue>
		where TQueue : EffectQueue<TEffectInfo>
		where TEffectInfo : EffectInfo
	{
		public int CurProcess { get; protected set; }
		public bool Processing { get; protected set; }
		public bool Sending { get; protected set; }

		public virtual IPoint3D Start { get; set; }
		public virtual Map Map { get; set; }
		public virtual int Repeat { get; set; }
		public virtual TimeSpan Interval { get; set; }
		public virtual Action<TEffectInfo> EffectHandler { get; set; }
		public virtual Action Callback { get; set; }

		public virtual bool EnableMutate { get; set; }
		public virtual bool Reversed { get; set; }

		public abstract TEffectInfo[] Effects { get; }

		public BaseEffect(
			IPoint3D start,
			Map map,
			int repeat = 0,
			TimeSpan? interval = null,
			Action<TEffectInfo> effectHandler = null,
			Action callback = null)
		{
			Start = start;
			Map = map;
			Repeat = Math.Max(0, repeat);
			Interval = interval ?? TimeSpan.FromMilliseconds(100);
			EffectHandler = effectHandler;
			Callback = callback;
		}

		public abstract TQueue CreateEffectQueue(IEnumerable<TEffectInfo> queue);
		public abstract TEffectInfo CloneEffectInfo(TEffectInfo src);

		public void Update()
		{
			Clear();

			if (Effects == null || Effects.Length == 0)
			{
				return;
			}

			var points = GetTargetPoints(CurProcess);

			if (points == null || points.Length == 0)
			{
				return;
			}

			Capacity = points.Length;

			this.SetAll(i => null);

			points.For(
				(i, list) =>
				{
					if (list == null || list.Length == 0)
					{
						return;
					}

					var fx = new TEffectInfo[list.Length][];

					fx.SetAll(fxi => new TEffectInfo[Effects.Length]);

					Parallel.For(
						0,
						list.Length,
						index =>
						{
							Point3D p = list[index];

							Effects.For(
								(eindex, effect) =>
								{
									TEffectInfo e = CloneEffectInfo(effect);

									if (e == null)
									{
										return;
									}

									e.Source = new Entity(Serial.Zero, p, Map);
									e.Map = Map;

									if (EnableMutate)
									{
										MutateEffect(e);
									}

									fx[index][eindex] = e;
								});
						});

					TQueue q = CreateEffectQueue(fx.Combine());

					if (q.Handler == null)
					{
						q.Handler = EffectHandler;
					}

					this[i] = q;
				});

			for (int i = Count - 1; i >= 0; i--)
			{
				if (this[i] == null)
				{
					RemoveAt(i);
				}
			}

			TrimExcess();

			if (Reversed)
			{
				Reverse();
			}

			int idx = 0;

			foreach (TQueue cur in this)
			{
				if (++idx >= Count)
				{
					cur.Callback = () =>
					{
						if (Callback != null)
						{
							Callback();
						}

						Sending = false;

						if (++CurProcess <= Repeat)
						{
							Timer.DelayCall(Interval, Send);
						}
						else
						{
							CurProcess = 0;
							Processing = false;
						}
					};

					break;
				}

				TQueue next = this[idx];

				cur.Callback = () =>
				{
					Processing = true;
					Timer.DelayCall(Interval, next.Process);
				};
			}

			OnUpdated();
		}

		public virtual Point3D[][] GetTargetPoints(int dist)
		{
			return Start == null ? new Point3D[0][] : new[] {new[] {Start.Clone3D()}};
		}

		protected virtual void OnUpdated()
		{ }

		public virtual void MutateEffect(TEffectInfo e)
		{ }

		public void Send()
		{
			if (Sending)
			{
				return;
			}

			Sending = true;

			VitaNexCore.TryCatch(
				() =>
				{
					Update();

					if (Count == 0 || this[0] == null)
					{
						return;
					}

					Processing = true;
					this[0].Process();
					OnSend();
				},
				e => e.ToConsole());
		}

		public virtual void OnSend()
		{ }
	}

	public abstract class BaseRangedEffect<TQueue, TEffectInfo> : BaseEffect<TQueue, TEffectInfo>
		where TQueue : EffectQueue<TEffectInfo>
		where TEffectInfo : EffectInfo
	{
		public virtual int Range { get; set; }
		public virtual bool AverageZ { get; set; }

		public BaseRangedEffect(
			IPoint3D start,
			Map map,
			int range = 5,
			int repeat = 0,
			TimeSpan? interval = null,
			Action<TEffectInfo> effectHandler = null,
			Action callback = null)
			: base(start, map, repeat, interval, effectHandler, callback)
		{
			Range = range;
			AverageZ = true;
		}

		public override Point3D[][] GetTargetPoints(int count)
		{
			return Start.ScanRangeGet(Map, Range, ComputePoint, AverageZ);
		}

		protected virtual bool ExcludePoint(Point3D p, int range, Direction fromCenter)
		{
			return false;
		}

		protected virtual bool ComputePoint(ScanRangeResult r)
		{
			if (!r.Excluded && ExcludePoint(r.Current, r.Distance, Utility.GetDirection(Start, r.Current)))
			{
				r.Exclude();
			}

			return false;
		}
	}

	public abstract class BaseBoundsEffect<TQueue, TEffectInfo> : BaseEffect<TQueue, TEffectInfo>
		where TQueue : EffectQueue<TEffectInfo>
		where TEffectInfo : EffectInfo
	{
		public virtual Rectangle2D Bounds { get; set; }
		public virtual bool AverageZ { get; set; }

		public BaseBoundsEffect(
			IPoint3D start,
			Map map,
			Rectangle2D bounds,
			int repeat = 0,
			TimeSpan? interval = null,
			Action<TEffectInfo> effectHandler = null,
			Action callback = null)
			: base(start, map, repeat, interval, effectHandler, callback)
		{
			Bounds = bounds;
			AverageZ = true;
		}

		public override Point3D[][] GetTargetPoints(int count)
		{
			var points = new List<Point3D>[Math.Max(Bounds.Width, Bounds.Height)];

			Bounds.ForEach(
				p2d =>
				{
					int distance = (int)Math.Floor(Start.GetDistance(p2d));

					points[distance].Add(p2d.ToPoint3D(AverageZ ? p2d.GetAverageZ(Map) : Start.Z));
				});

			return points.ToMultiArray();
		}
	}
}