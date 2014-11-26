#region Header
//   Vorspire    _,-'/-'/  Times.cs
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

namespace VitaNex.Schedules
{
	public class ScheduleTimes : IEnumerable<TimeSpan>
	{
		private static ScheduleTimes _None,
									 _Noon,
									 _Midnight,
									 _EveryHour,
									 _EveryHalfHour,
									 _EveryQuarterHour,
									 _EveryTenMinutes,
									 _EveryFiveMinutes,
									 _EveryMinute,
									 _FourTwenty;

		private List<TimeSpan> _List = new List<TimeSpan>();

		public ScheduleTimes(ScheduleTimes times)
		{
			Add(times);
		}

		public ScheduleTimes(IEnumerable<TimeSpan> times)
		{
			Add(times);
		}

		public ScheduleTimes(params TimeSpan[] times)
		{
			Add(times);
		}

		public ScheduleTimes(GenericReader reader)
		{
			Deserialize(reader);
		}

		public static ScheduleTimes None { get { return new ScheduleTimes(_None); } }
		public static ScheduleTimes Noon { get { return new ScheduleTimes(_Noon); } }
		public static ScheduleTimes Midnight { get { return new ScheduleTimes(_Midnight); } }
		public static ScheduleTimes EveryHour { get { return new ScheduleTimes(_EveryHour); } }
		public static ScheduleTimes EveryHalfHour { get { return new ScheduleTimes(_EveryHalfHour); } }
		public static ScheduleTimes EveryQuarterHour { get { return new ScheduleTimes(_EveryQuarterHour); } }
		public static ScheduleTimes EveryTenMinutes { get { return new ScheduleTimes(_EveryTenMinutes); } }
		public static ScheduleTimes EveryFiveMinutes { get { return new ScheduleTimes(_EveryFiveMinutes); } }
		public static ScheduleTimes EveryMinute { get { return new ScheduleTimes(_EveryMinute); } }
		public static ScheduleTimes FourTwenty { get { return new ScheduleTimes(_FourTwenty); } }

		public int Count { get { return _List.Count; } }

		public TimeSpan? this[int index]
		{
			get { return index < 0 || index >= _List.Count ? (TimeSpan?)null : _List[index]; }
			set
			{
				if (index < 0 || index >= _List.Count)
				{
					return;
				}

				if (value == null)
				{
					_List.RemoveAt(index);
				}
				else
				{
					_List[index] = (TimeSpan)value;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _List.GetEnumerator();
		}

		public IEnumerator<TimeSpan> GetEnumerator()
		{
			return _List.GetEnumerator();
		}

		public static void Config()
		{
			_None = new ScheduleTimes();
			_Noon = new ScheduleTimes(TimeSpan.FromHours(12));
			_Midnight = new ScheduleTimes(TimeSpan.Zero);
			_EveryHour = new ScheduleTimes();
			_EveryHalfHour = new ScheduleTimes();
			_EveryQuarterHour = new ScheduleTimes();
			_EveryTenMinutes = new ScheduleTimes();
			_EveryFiveMinutes = new ScheduleTimes();
			_EveryMinute = new ScheduleTimes();

			for (int hours = 0; hours < 24; hours++)
			{
				_EveryHour.Add(new TimeSpan(hours, 0, 0));

				for (int minutes = 0; minutes < 60; minutes++)
				{
					_EveryMinute.Add(new TimeSpan(hours, minutes, 0));

					if (minutes % 5 == 0)
					{
						_EveryFiveMinutes.Add(new TimeSpan(hours, minutes, 0));
					}

					if (minutes % 10 == 0)
					{
						_EveryTenMinutes.Add(new TimeSpan(hours, minutes, 0));
					}

					if (minutes % 15 == 0)
					{
						_EveryQuarterHour.Add(new TimeSpan(hours, minutes, 0));
					}

					if (minutes % 30 == 0)
					{
						_EveryHalfHour.Add(new TimeSpan(hours, minutes, 0));
					}
				}
			}

			_FourTwenty = new ScheduleTimes(new[] {new TimeSpan(4, 20, 0), new TimeSpan(16, 20, 0)});
		}

		private void Validate(ref TimeSpan time)
		{
			time = new TimeSpan(0, time.Hours, time.Minutes, 0, 0);
		}

		public bool Contains(TimeSpan time, bool validate = true)
		{
			if (validate)
			{
				Validate(ref time);
			}

			return _List.Contains(time);
		}

		public void Add(ScheduleTimes times)
		{
			foreach (TimeSpan time in times)
			{
				InternalAdd(time);
			}

			_List.Sort();
		}

		public void Add(IEnumerable<TimeSpan> times)
		{
			foreach (TimeSpan time in times)
			{
				InternalAdd(time);
			}

			_List.Sort();
		}

		public void Add(params TimeSpan[] times)
		{
			foreach (TimeSpan time in times)
			{
				InternalAdd(time);
			}

			_List.Sort();
		}

		private void InternalAdd(TimeSpan time)
		{
			Validate(ref time);

			if (Contains(time, false))
			{
				return;
			}

			_List.Add(time);
		}

		public void Remove(ScheduleTimes times)
		{
			foreach (TimeSpan time in times)
			{
				InternalRemove(time);
			}

			_List.TrimExcess();
			_List.Sort();
		}

		public void Remove(IEnumerable<TimeSpan> times)
		{
			foreach (TimeSpan time in times)
			{
				InternalRemove(time);
			}

			_List.TrimExcess();
			_List.Sort();
		}

		public void Remove(params TimeSpan[] times)
		{
			foreach (TimeSpan time in times)
			{
				InternalRemove(time);
			}

			_List.TrimExcess();
			_List.Sort();
		}

		private void InternalRemove(TimeSpan time)
		{
			Validate(ref time);

			if (!Contains(time, false))
			{
				return;
			}

			_List.Remove(time);
		}

		public void Clear()
		{
			_List.Clear();
			_List.TrimExcess();
		}

		public TimeSpan[] ToArray()
		{
			return _List.ToArray();
		}

		public override string ToString()
		{
			var times = new string[_List.Count];

			for (int i = 0; i < times.Length; i++)
			{
				times[i] = Schedules.FormatTime(_List[i]);
			}

			return String.Join(", ", times);
		}

		public virtual void Serialize(GenericWriter writer)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					writer.WriteList(_List, writer.Write);
					break;
			}
		}

		public virtual void Deserialize(GenericReader reader)
		{
			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					_List = reader.ReadList(reader.ReadTimeSpan);
					break;
			}
		}
	}
}