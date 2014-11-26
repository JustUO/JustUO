#region Header
//   Vorspire    _,-'/-'/  CollectionExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Server;
#endregion

namespace System
{
	public static class CollectionExtUtility
	{
		private static readonly object[] _Empty = new object[0];

		public static IEnumerable<T> ToEnumerable<T>(this T obj)
		{
			if (obj != null)
			{
				yield return obj;
			}
		}

		public static Queue<T> ToQueue<T>(this IEnumerable<T> list)
		{
			return new Queue<T>(list);
		}

		public static IEnumerator<T> GetEnumerator<T>(this T[] arr)
		{
			return Ensure(arr).GetEnumerator();
		}

		public static IEnumerable<T> Ensure<T>(this IEnumerable<T> list)
		{
			if (list == null)
			{
				yield break;
			}

			foreach (var o in list)
			{
				yield return o;
			}
		}

		public static IEnumerable<T> With<T>(this IEnumerable<T> list, params T[] include)
		{
			return With(list, include.AsEnumerable());
		}

		public static IEnumerable<T> With<T>(this IEnumerable<T> list, params IEnumerable<T>[] include)
		{
			if (list == null || include == null)
			{
				yield break;
			}

			foreach (var o in list)
			{
				yield return o;
			}

			foreach (var o in include.Where(o => o != null).SelectMany(o => o))
			{
				yield return o;
			}
		}

		public static IEnumerable<T> Without<T>(this IEnumerable<T> list, params T[] exclude)
		{
			return With(list, exclude.AsEnumerable());
		}

		public static IEnumerable<T> Without<T>(this IEnumerable<T> list, params IEnumerable<T>[] exclude)
		{
			if (list == null || exclude == null)
			{
				yield break;
			}

			foreach (var o in list.Where(o => o != null && exclude.All(e => !e.Contains(o))))
			{
				yield return o;
			}
		}

		/// <summary>
		///     Trim the given number of entries from the start of a collection.
		/// </summary>
		public static void TrimStart<T>(this List<T> source, int count)
		{
			if (source == null || source.Count == 0 || count <= 0)
			{
				return;
			}

			while (--count >= 0 && source.Count > 0)
			{
				source.RemoveAt(0);
			}
		}

		/// <summary>
		///     Trim the given number of entries from the end of a collection.
		/// </summary>
		public static void TrimEnd<T>(this List<T> source, int count)
		{
			if (source == null || source.Count == 0 || count <= 0)
			{
				return;
			}

			while (--count >= 0 && source.Count > 0)
			{
				source.RemoveAt(source.Count - 1);
			}
		}

		/// <summary>
		///     Trim the entries from the start of a collection until the given count is reached.
		/// </summary>
		public static void TrimStartTo<T>(this List<T> source, int count)
		{
			if (source == null || count < source.Count)
			{
				return;
			}

			while (source.Count > count)
			{
				source.RemoveAt(0);
			}
		}

		/// <summary>
		///     Trim the entries from the end of a collection until the given count is reached.
		/// </summary>
		public static void TrimEndTo<T>(this List<T> source, int count)
		{
			if (source == null || count < source.Count)
			{
				return;
			}

			while (source.Count > count)
			{
				source.RemoveAt(source.Count - 1);
			}
		}

		public static bool AddOrReplace<T>(this List<T> source, T obj)
		{
			return AddOrReplace(source, obj, obj);
		}

		public static bool AddOrReplace<T>(this List<T> source, T search, T replace)
		{
			if (source == null)
			{
				return false;
			}

			int index = source.IndexOf(search);

			if (!InBounds(source, index))
			{
				source.Add(replace);
			}
			else
			{
				source[index] = replace;
			}

			return true;
		}

		public static bool AddOrReplace<TKey, TVal>(this IDictionary<TKey, TVal> source, TKey key, TVal val)
		{
			if (source == null || key == null)
			{
				return false;
			}

			if (!source.ContainsKey(key))
			{
				source.Add(key, val);
			}
			else
			{
				source[key] = val;
			}

			return true;
		}

		public static bool AddOrReplace<TKey, TVal>(this IDictionary<TKey, TVal> source, TKey key, Func<TVal, TVal> resolver)
		{
			if (source == null || key == null || resolver == null)
			{
				return false;
			}

			if (!source.ContainsKey(key))
			{
				source.Add(key, resolver(default(TVal)));
			}
			else
			{
				source[key] = resolver(source[key]);
			}

			return true;
		}

		public static void Free<T>(this List<T> list, bool clear)
		{
			if (list == null)
			{
				return;
			}

			if (clear)
			{
				list.Clear();
			}

			// Override default TrimeExcess behaviour by requiring a lower Count to Capacity percent.
			// By default, trimExcess will only resize the collection if the count is below 90 percent.
			// 80 percent should help balance out RAM versus CPU usage in the context of RunUO.
			//if (list.Count <= list.Capacity * 0.80)
			//{
			list.TrimExcess();
			//}
		}

		public static void Free<T>(this IEnumerable<List<T>> list, bool clear)
		{
			foreach (var l in list)
			{
				Free(l, clear);
			}
		}

		public static void Free<T>(this HashSet<T> list, bool clear)
		{
			if (list == null)
			{
				return;
			}

			if (clear)
			{
				list.Clear();
			}

			list.TrimExcess();
		}

		public static void Free<T>(this IEnumerable<HashSet<T>> list, bool clear)
		{
			foreach (var l in list)
			{
				Free(l, clear);
			}
		}

		public static void Free<T>(this Queue<T> list, bool clear)
		{
			if (list == null)
			{
				return;
			}

			if (clear)
			{
				list.Clear();
			}

			list.TrimExcess();
		}

		public static void Free<T>(this IEnumerable<Queue<T>> list, bool clear)
		{
			foreach (var l in list)
			{
				Free(l, clear);
			}
		}

		public static bool InBounds<T>(this T[] source, int index)
		{
			return source != null && index >= 0 && index < source.Length;
		}

		public static bool InBounds<T>(this ICollection<T> source, int index)
		{
			return source != null && index >= 0 && index < source.Count;
		}

		public static bool InBounds<T>(this IEnumerable<T> source, int index)
		{
			return source != null && index >= 0 && index < source.Count();
		}

		/// <summary>
		///     Creates a multi-dimensional array from a multi-dimensional collection.
		/// </summary>
		public static T[][] ToMultiArray<T>(this IEnumerable<IEnumerable<T>> source)
		{
			if (source == null)
			{
				return new T[0][];
			}

			var ma = source.Select(e => e.ToArray()).ToArray();

			SetAll(ma, (i, e) => e ?? new T[0]);

			return ma;
		}

		public static void RemoveRange<T>(this List<T> source, Func<T, bool> predicate)
		{
			if (source != null && predicate != null)
			{
				RemoveRange(source, source.Where(predicate));
			}
		}

		public static void RemoveRange<T>(this List<T> source, params T[] entries)
		{
			if (source != null && entries != null && entries.Length > 0)
			{
				RemoveRange(source, entries.AsEnumerable());
			}
		}

		public static void RemoveRange<T>(this List<T> source, IEnumerable<T> entries)
		{
			if (source != null && entries != null)
			{
				ForEach(entries.Where(e => e != null), e => source.Remove(e));
			}
		}

		public static void RemoveRange<TKey, TVal>(this IDictionary<TKey, TVal> source, Func<TKey, TVal, bool> predicate)
		{
			if (source != null && predicate != null)
			{
				RemoveRange(source, kv => predicate(kv.Key, kv.Value));
			}
		}

		public static void RemoveRange<TKey, TVal>(
			this IDictionary<TKey, TVal> source, Func<KeyValuePair<TKey, TVal>, bool> predicate)
		{
			if (source != null && predicate != null)
			{
				RemoveRange(source, source.Where(predicate));
			}
		}

		public static void RemoveRange<TKey, TVal>(
			this IDictionary<TKey, TVal> source, params KeyValuePair<TKey, TVal>[] entries)
		{
			if (source != null && entries != null && entries.Length > 0)
			{
				RemoveRange(source, entries.AsEnumerable());
			}
		}

		public static void RemoveRange<TKey, TVal>(
			this IDictionary<TKey, TVal> source, IEnumerable<KeyValuePair<TKey, TVal>> entries)
		{
			if (source != null && entries != null)
			{
				ForEach(entries.Select(kv => kv.Key).Where(k => k != null), k => source.Remove(k));
			}
		}

		public static void RemoveKeyRange<TKey, TVal>(this IDictionary<TKey, TVal> source, Func<TKey, bool> predicate)
		{
			if (source != null && predicate != null)
			{
				RemoveKeyRange(source, source.Keys.Where(predicate));
			}
		}

		public static void RemoveKeyRange<TKey, TVal>(this IDictionary<TKey, TVal> source, params TKey[] keys)
		{
			if (source != null && keys != null && keys.Length > 0)
			{
				RemoveKeyRange(source, keys.AsEnumerable());
			}
		}

		public static void RemoveKeyRange<TKey, TVal>(this IDictionary<TKey, TVal> source, IEnumerable<TKey> keys)
		{
			if (source != null && keys != null)
			{
				ForEach(keys.Where(k => k != null), k => source.Remove(k));
			}
		}

		public static void RemoveValueRange<TKey, TVal>(this IDictionary<TKey, TVal> source, Func<TVal, bool> predicate)
		{
			if (source != null && predicate != null)
			{
				RemoveValueRange(source, source.Values.Where(predicate));
			}
		}

		public static void RemoveValueRange<TKey, TVal>(this IDictionary<TKey, TVal> source, params TVal[] values)
		{
			if (source != null && values != null && values.Length > 0)
			{
				RemoveValueRange(source, values.AsEnumerable());
			}
		}

		public static void RemoveValueRange<TKey, TVal>(this IDictionary<TKey, TVal> source, IEnumerable<TVal> values)
		{
			if (source != null && values != null)
			{
				ForEach(values.SelectMany(v => source.Where(kv => Equals(kv.Value, v)).Select(kv => kv.Key)), k => source.Remove(k));
			}
		}

		/// <summary>
		///     Combines a multi-dimensional collection into a single-dimension collection.
		/// </summary>
		public static TEntry[] Combine<TEntry>(this IEnumerable<IEnumerable<TEntry>> source)
		{
			return source != null ? source.SelectMany(e => e != null ? e.ToArray() : new TEntry[0]).ToArray() : new TEntry[0];
		}

		public static void SetAll<TEntry>(this TEntry[] source, TEntry entry)
		{
			SetAll(source, i => entry);
		}

		public static void SetAll<TEntry>(this TEntry[] source, Func<TEntry> instantiate)
		{
			if (source == null)
			{
				return;
			}

			for (int i = 0; i < source.Length; i++)
			{
				source[i] = instantiate != null ? instantiate() : source[i];
			}
		}

		public static void SetAll<TEntry>(this TEntry[] source, Func<int, TEntry> instantiate)
		{
			if (source == null)
			{
				return;
			}

			for (int i = 0; i < source.Length; i++)
			{
				source[i] = instantiate != null ? instantiate(i) : source[i];
			}
		}

		public static void SetAll<TEntry>(this TEntry[] source, Func<int, TEntry, TEntry> instantiate)
		{
			if (source == null)
			{
				return;
			}

			for (int i = 0; i < source.Length; i++)
			{
				source[i] = instantiate != null ? instantiate(i, source[i]) : source[i];
			}
		}

		public static void SetAll<TEntry>(this List<TEntry> source, TEntry entry)
		{
			SetAll(source, i => entry);
		}

		public static void SetAll<TEntry>(this List<TEntry> source, Func<TEntry> instantiate)
		{
			if (source == null)
			{
				return;
			}

			while (source.Count < source.Capacity)
			{
				source.Add(default(TEntry));
			}

			for (int i = 0; i < source.Count; i++)
			{
				source[i] = instantiate != null ? instantiate() : source[i];
			}
		}

		public static void SetAll<TEntry>(this List<TEntry> source, Func<int, TEntry> instantiate)
		{
			if (source == null)
			{
				return;
			}

			while (source.Count < source.Capacity)
			{
				source.Add(default(TEntry));
			}

			for (int i = 0; i < source.Count; i++)
			{
				source[i] = instantiate != null ? instantiate(i) : source[i];
			}
		}

		public static void SetAll<TEntry>(this List<TEntry> source, Func<int, TEntry, TEntry> instantiate)
		{
			if (source == null)
			{
				return;
			}

			while (source.Count < source.Capacity)
			{
				source.Add(default(TEntry));
			}

			for (int i = 0; i < source.Count; i++)
			{
				source[i] = instantiate != null ? instantiate(i, source[i]) : source[i];
			}
		}

		/// <summary>
		///     Iterates through the elements in a clone of the given collection.
		///     This method is safe, the list can be manipulated by the action without causing "out of range", or "collection modified" exceptions.
		/// </summary>
		public static void For<TObj>(this IEnumerable<TObj> list, Action<int> action)
		{
			if (list == null || action == null)
			{
				return;
			}

			var arr = list.ToList();

			for (int i = 0; i < arr.Count; i++)
			{
				action(i);
			}

			Free(arr, true);
		}

		/// <summary>
		///     Iterates through the elements in a clone of the given collection.
		///     This method is safe, the list can be manipulated by the action without causing "out of range", or "collection modified" exceptions.
		/// </summary>
		public static void For<TObj>(this IEnumerable<TObj> list, Action<int, TObj> action)
		{
			if (list == null || action == null)
			{
				return;
			}

			var arr = list.ToList();

			for (int i = 0; i < arr.Count; i++)
			{
				action(i, arr[i]);
			}

			Free(arr, true);
		}

		/// <summary>
		///     Iterates through the elements in a clone of the given collection.
		///     This method is safe, the list can be manipulated by the action without causing "out of range", or "collection modified" exceptions.
		/// </summary>
		public static void For<TKey, TValue>(this IDictionary<TKey, TValue> list, Action<int, TKey, TValue> action)
		{
			if (list == null || action == null)
			{
				return;
			}

			var arr = list.ToList();

			for (int i = 0; i < arr.Count; i++)
			{
				action(i, arr[i].Key, arr[i].Value);
			}

			Free(arr, true);
		}

		/// <summary>
		///     Iterates through the elements in a clone of the given collection.
		///     This method is safe, the list can be manipulated by the action without causing "out of range", or "collection modified" exceptions.
		/// </summary>
		public static void ForEach<TObj>(this IEnumerable<TObj> list, Action<TObj> action)
		{
			if (list == null || action == null)
			{
				return;
			}

			var arr = list.ToList();

			foreach (TObj o in arr)
			{
				action(o);
			}

			Free(arr, true);
		}

		/// <summary>
		///     Iterates through a specified range of elements in a clone of the given collection.
		///     This method is safe, the list can be manipulated by the action without causing "out of range", or "collection modified" exceptions.
		/// </summary>
		public static void ForRange<TObj>(this IEnumerable<TObj> list, int offset, Action<int, TObj> action)
		{
			if (list == null || action == null)
			{
				return;
			}

			var arr = list.ToList();

			if (offset < 0)
			{
				arr.Reverse();
			}

			int index = Math.Min(arr.Count, Math.Abs(offset));

			for (int i = index; i < arr.Count; i++)
			{
				action(i, arr[i]);
			}

			Free(arr, true);
		}

		/// <summary>
		///     Iterates through a specified range of elements in a clone of the given collection.
		///     This method is safe, the list can be manipulated by the action without causing "out of range", or "collection modified" exceptions.
		/// </summary>
		public static void ForRange<TObj>(this IEnumerable<TObj> list, int index, int count, Action<int, TObj> action)
		{
			if (list == null || action == null)
			{
				return;
			}

			var arr = list.ToList();

			index = Math.Max(0, Math.Min(arr.Count, index));

			if (count < 0)
			{
				arr.Reverse();
			}

			count = Math.Min(arr.Count, Math.Abs(count));

			for (int i = index; i < index + count && i < arr.Count; i++)
			{
				action(i, arr[i]);
			}

			Free(arr, true);
		}

		/// <summary>
		///     Iterates through the elements in a clone of the given dictionary.
		///     This method is safe, the dictionary can be manipulated by the action without causing "out of range", or "collection modified" exceptions.
		/// </summary>
		public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Action<TKey, TValue> action)
		{
			if (dictionary == null || dictionary.Count == 0 || action == null)
			{
				return;
			}

			var arr = dictionary.ToList();

			foreach (var kvp in arr)
			{
				action(kvp.Key, kvp.Value);
			}

			Free(arr, true);
		}

		/// <summary>
		///     Iterates through the elements in a clone of the given dictionary.
		///     This method is safe, the dictionary can be manipulated by the action without causing "out of range", or "collection modified" exceptions.
		/// </summary>
		public static void ForEach<TKey, TValue>(
			this IDictionary<TKey, TValue> dictionary, Action<KeyValuePair<TKey, TValue>> action)
		{
			if (dictionary == null || dictionary.Count == 0 || action == null)
			{
				return;
			}

			var arr = dictionary.ToList();

			foreach (var kvp in arr)
			{
				action(kvp);
			}

			Free(arr, true);
		}

		/// <summary>
		///     Iterates through a specified range of elements in a clone of the given dictionary.
		///     This method is safe, the dictionary can be manipulated by the action without causing "out of range", or "collection modified" exceptions.
		/// </summary>
		public static void ForRange<TKey, TValue>(
			this IDictionary<TKey, TValue> dictionary, int offset, Action<int, KeyValuePair<TKey, TValue>> action)
		{
			if (dictionary == null || dictionary.Count == 0 || action == null)
			{
				return;
			}

			var arr = dictionary.ToList();

			if (offset < 0)
			{
				arr.Reverse();
			}

			int index = Math.Min(arr.Count, Math.Abs(offset));

			for (int i = index; i < arr.Count; i++)
			{
				action(i, arr[i]);
			}

			Free(arr, true);
		}

		/// <summary>
		///     Iterates through a specified range of elements in a clone of the given dictionary.
		///     This method is safe, the dictionary can be manipulated by the action without causing "out of range", or "collection modified" exceptions.
		/// </summary>
		public static void ForRange<TKey, TValue>(
			this IDictionary<TKey, TValue> dictionary, int index, int count, Action<int, KeyValuePair<TKey, TValue>> action)
		{
			if (dictionary == null || dictionary.Count == 0 || action == null)
			{
				return;
			}

			var arr = dictionary.ToList();

			if (count < 0)
			{
				arr.Reverse();
			}

			count = Math.Min(arr.Count, Math.Abs(count));

			for (int i = index; i < index + count && i < arr.Count; i++)
			{
				action(i, arr[i]);
			}

			Free(arr, true);
		}

		/// <summary>
		///     Iterates through a specified range of elements in a clone of the given dictionary.
		///     This method is safe, the dictionary can be manipulated by the action without causing "out of range", or "collection modified" exceptions.
		/// </summary>
		public static void ForRange<TKey, TValue>(
			this IDictionary<TKey, TValue> dictionary, int offset, Action<TKey, TValue> action)
		{
			if (dictionary != null && dictionary.Count > 0 && action != null)
			{
				ForRange(dictionary, offset, (i, kv) => action(kv.Key, kv.Value));
			}
		}

		/// <summary>
		///     Iterates through a specified range of elements in a clone of the given dictionary.
		///     This method is safe, the dictionary can be manipulated by the action without causing "out of range", or "collection modified" exceptions.
		/// </summary>
		public static void ForRange<TKey, TValue>(
			this IDictionary<TKey, TValue> dictionary, int index, int count, Action<TKey, TValue> action)
		{
			if (dictionary != null && dictionary.Count > 0 && action != null)
			{
				ForRange(dictionary, index, count, (i, kv) => action(kv.Key, kv.Value));
			}
		}

		/// <summary>
		///     Iterates through a specified range of elements in a clone of the given dictionary.
		///     This method is safe, the dictionary can be manipulated by the action without causing "out of range", or "collection modified" exceptions.
		/// </summary>
		public static void ForRange<TKey, TValue>(
			this IDictionary<TKey, TValue> dictionary, int offset, Action<int, TKey, TValue> action)
		{
			if (dictionary != null && dictionary.Count > 0 && action != null)
			{
				ForRange(dictionary, offset, (i, kv) => action(i, kv.Key, kv.Value));
			}
		}

		/// <summary>
		///     Iterates through a specified range of elements in a clone of the given dictionary.
		///     This method is safe, the dictionary can be manipulated by the action without causing "out of range", or "collection modified" exceptions.
		/// </summary>
		public static void ForRange<TKey, TValue>(
			this IDictionary<TKey, TValue> dictionary, int index, int count, Action<int, TKey, TValue> action)
		{
			if (dictionary != null && dictionary.Count > 0 && action != null)
			{
				ForRange(dictionary, index, count, (i, kv) => action(i, kv.Key, kv.Value));
			}
		}

		public static int IndexOf<TObj>(this IEnumerable<TObj> list, TObj obj)
		{
			return IndexOf(list, obj, null);
		}

		public static int IndexOf<TObj>(this IEnumerable<TObj> list, TObj obj, Func<TObj, bool> match)
		{
			if (list == null)
			{
				return -1;
			}

			int index = -1;
			bool found = false;

			foreach (var o in list)
			{
				++index;

				if (match != null && match(o))
				{
					found = true;
					break;
				}

				if (obj == null || (!ReferenceEquals(o, obj) && !o.Equals(obj)))
				{
					continue;
				}

				found = true;
				break;
			}

			return found ? index : -1;
		}

		public static TObj GetRandom<TObj>(this IEnumerable<TObj> list)
		{
			return GetRandom(list, default(TObj));
		}

		public static TObj GetRandom<TObj>(this IEnumerable<TObj> list, TObj def)
		{
			if (list == null)
			{
				return def;
			}

			if (list is TObj[])
			{
				return GetRandom((TObj[])list, def);
			}

			if (list is IList<TObj>)
			{
				return GetRandom((IList<TObj>)list, def);
			}

			if (list is ISet<TObj>)
			{
				return GetRandom((ISet<TObj>)list, def);
			}

			if (list is Queue<TObj>)
			{
				return GetRandom((Queue<TObj>)list, def);
			}

			var arr = list.ToList();

			var o = arr.Count == 0 ? def : arr[Utility.Random(arr.Count)];

			Free(arr, true);

			return o;
		}

		public static TObj GetRandom<TObj>(this TObj[] list)
		{
			return GetRandom(list, default(TObj));
		}

		public static TObj GetRandom<TObj>(this TObj[] list, TObj def)
		{
			if (list == null)
			{
				return def;
			}

			return list.Length == 0 ? def : list[Utility.Random(list.Length)];
		}

		public static TObj GetRandom<TObj>(this IList<TObj> list)
		{
			return GetRandom(list, default(TObj));
		}

		public static TObj GetRandom<TObj>(this IList<TObj> list, TObj def)
		{
			if (list == null)
			{
				return def;
			}

			return list.Count == 0 ? def : list[Utility.Random(list.Count)];
		}

		public static TObj GetRandom<TObj>(this ISet<TObj> list)
		{
			return GetRandom(list, default(TObj));
		}

		public static TObj GetRandom<TObj>(this ISet<TObj> list, TObj def)
		{
			if (list == null)
			{
				return def;
			}

			return list.Count == 0 ? def : list.ElementAt(Utility.Random(list.Count));
		}

		public static TObj GetRandom<TObj>(this Queue<TObj> list)
		{
			return GetRandom(list, default(TObj));
		}

		public static TObj GetRandom<TObj>(this Queue<TObj> list, TObj def)
		{
			if (list == null)
			{
				return def;
			}

			return list.Count == 0 ? def : list.ElementAt(Utility.Random(list.Count));
		}

		public static TObj Pop<TObj>(this List<TObj> list)
		{
			return Pop(list, default(TObj));
		}

		public static TObj Pop<TObj>(this List<TObj> list, TObj def)
		{
			if (list == null || list.Count == 0)
			{
				return def;
			}

			TObj o;
			list.Remove(o = list[0]);
			return o;
		}

		public static List<TObj> RemoveAllGet<TObj>(this List<TObj> list, Predicate<TObj> match)
		{
			if (list == null)
			{
				return new List<TObj>();
			}

			var removed = new List<TObj>(list.Count);

			if (match != null)
			{
				list.RemoveAll(
					o =>
					{
						if (!match(o))
						{
							return false;
						}

						removed.Add(o);
						return true;
					});
			}
			else
			{
				removed.AddRange(list);
				list.Clear();
			}

			Free(list, false);
			Free(removed, false);

			return removed;
		}

		public static IEnumerable<TObj> Not<TObj>(this IEnumerable<TObj> list, Func<TObj, bool> predicate)
		{
			if (list == null)
			{
				return _Empty.OfType<TObj>();
			}

			return predicate == null ? list : list.Where(o => !predicate(o));
		}

		public static bool TrueForAll<TObj>(this IEnumerable<TObj> list, Predicate<TObj> match)
		{
			return list == null || match == null || list.All(o => match(o));
		}

		public static bool Contains<TObj>(this TObj[] list, TObj obj)
		{
			return IndexOf(list, obj) != -1;
		}

		public static void Shuffle<TObj>(this IList<TObj> list)
		{
			if (list == null || list.Count == 0)
			{
				return;
			}

			int n = list.Count;

			while (n > 1)
			{
				n--;

				int k = Utility.Random(n + 1);
				TObj value = list[k];

				list[k] = list[n];
				list[n] = value;
			}
		}

		public static void Reverse<TObj>(this TObj[] list)
		{
			var buffer = list.ToList();

			buffer.Reverse();

			SetAll(list, i => buffer[i]);

			Free(buffer, true);
		}

		public static TObj[] Dupe<TObj>(this TObj[] list)
		{
			return list != null ? list.ToArray() : new TObj[0];
		}

		public static TObj[] Dupe<TObj>(this TObj[] list, Func<TObj, TObj> action)
		{
			if (list == null || list.Length == 0 || action == null)
			{
				return Dupe(list);
			}

			return list.Select(action).ToArray();
		}

		public static List<TObj> Merge<TObj>(this ICollection<TObj> list, List<TObj> list2, params List<TObj>[] lists)
		{
			var merged = new List<TObj>(list.Count + list2.Count + lists.Sum(l => l.Count));

			merged.AddRange(list);
			merged.AddRange(list2);
			merged.AddRange(lists.SelectMany(l => l));

			Free(merged, false);

			return merged;
		}

		public static TObj[] Merge<TObj>(this TObj[] list, TObj[] list2, params TObj[][] lists)
		{
			var merged = new List<TObj>(list.Length + list2.Length + lists.Sum(l => l.Length));

			merged.AddRange(list);
			merged.AddRange(list2);
			merged.AddRange(lists.SelectMany(l => l));

			var arr = merged.ToArray();

			Free(merged, true);

			return arr;
		}

		public static List<TObj> ChainSort<TObj>(this List<TObj> list)
		{
			if (list == null || list.Count == 0)
			{
				return list ?? new List<TObj>();
			}

			list.Sort();

			return list;
		}

		public static List<TObj> ChainSort<TObj>(this List<TObj> list, Comparison<TObj> compare)
		{
			if (compare == null)
			{
				return ChainSort(list);
			}

			if (list == null || list.Count == 0)
			{
				return list ?? new List<TObj>();
			}

			list.Sort(compare);

			return list;
		}

		public static KeyValuePair<TKey, TValue> Pop<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
		{
			var kvp = default(KeyValuePair<TKey, TValue>);

			if (dictionary == null || dictionary.Count == 0)
			{
				return kvp;
			}

			kvp = dictionary.FirstOrDefault();

			if (kvp.Key != null)
			{
				dictionary.Remove(kvp.Key);
			}

			return kvp;
		}

		public static void Push<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
		{
			if (dictionary == null || key == null)
			{
				return;
			}

			var d = new Dictionary<TKey, TValue>(dictionary);

			d.Remove(key);

			dictionary.Clear();
			dictionary.Add(key, value);

			foreach (var kv in d)
			{
				dictionary.Add(kv.Key, kv.Value);
			}

			d.Clear();
		}

		public static TKey GetKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value)
		{
			return dictionary != null && dictionary.Count != 0
					   ? dictionary.FirstOrDefault(kv => ReferenceEquals(kv.Value, value) || Equals(value, kv.Value)).Key
					   : default(TKey);
		}

		public static TKey GetKeyAt<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, int index)
		{
			return dictionary != null && dictionary.Count != 0 && index >= 0 && index < dictionary.Count
					   ? dictionary.Keys.ElementAt(index)
					   : default(TKey);
		}

		public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
		{
			return dictionary != null && dictionary.Count != 0
					   ? dictionary.FirstOrDefault(kv => ReferenceEquals(kv.Key, key) || Equals(key, kv.Key)).Value
					   : default(TValue);
		}

		public static TValue GetValueAt<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, int index)
		{
			return dictionary != null && dictionary.Count != 0 && index >= 0 && index < dictionary.Count
					   ? dictionary.Values.ElementAt(index)
					   : default(TValue);
		}

		public static IEnumerable<KeyValuePair<TKey, TValue>> GetRange<TKey, TValue>(
			this IDictionary<TKey, TValue> dictionary, int index, int count)
		{
			index = Math.Max(0, Math.Min(dictionary.Count, index));
			count = Math.Min(dictionary.Count, count);

			if (count >= 0)
			{
				for (int i = index; i < index + count && i < dictionary.Count; i++)
				{
					yield return dictionary.ElementAt(i);
				}
			}
			else
			{
				for (int i = index; i > index + count && i > 0; i--)
				{
					yield return dictionary.ElementAt(i);
				}
			}
		}

		public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
			this IDictionary<TKey, TValue> source1, IDictionary<TKey, TValue> source2, params IDictionary<TKey, TValue>[] sourceN)
		{
			var arr = new IDictionary<TKey, TValue>[sourceN.Length + 2];

			arr[0] = source1;
			arr[1] = source2;

			SetAll(arr, (i, d) => i < 2 ? d : sourceN[i - 2]);

			return Merge(arr);
		}

		public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
			this IEnumerable<IDictionary<TKey, TValue>> collection,
			Func<IGrouping<TKey, TValue>, TKey> keySelector = null,
			Func<IGrouping<TKey, TValue>, TValue> elementSelector = null)
		{
			if (collection == null)
			{
				return new Dictionary<TKey, TValue>();
			}

			return collection.SelectMany(d => d)
							 .ToLookup(kv => kv.Key, kv => kv.Value)
							 .ToDictionary(keySelector ?? (g => g.Key), elementSelector ?? (g => g.First()));
		}

		public static void EnqueueRange<T>(this Queue<T> queue, params T[] buffer)
		{
			if (queue == null || buffer == null || buffer.Length == 0)
			{
				return;
			}

			foreach (var o in buffer)
			{
				queue.Enqueue(o);
			}
		}

		public static void DequeueRange<T>(this Queue<T> queue, ref T[] buffer)
		{
			if (queue == null || buffer == null || buffer.Length == 0)
			{
				return;
			}

			if (buffer.Length > queue.Count)
			{
				buffer = new T[queue.Count];
			}

			SetAll(buffer, queue.Dequeue);
		}

		private static readonly Regex _NaturalOrderExpr = new Regex(@"\d+", RegexOptions.Compiled);

		public static IEnumerable<TSource> OrderByNatural<TSource, TKey>(
			this IEnumerable<TSource> source, Func<TSource, TKey> selector)
		{
			int max = 0;

			var selection = source.Select(
				o =>
				{
					var v = selector(o);
					var s = v != null ? v.ToString() : String.Empty;

					if (!String.IsNullOrWhiteSpace(s))
					{
						var mc = _NaturalOrderExpr.Matches(s);

						if (mc.Count > 0)
						{
							max = Math.Max(max, mc.Cast<Match>().Max(m => m.Value.Length));
						}
					}

					return new
					{
						Key = o,
						Value = s
					};
				}).ToList();

			return
				selection.OrderBy(
					o =>
					String.IsNullOrWhiteSpace(o.Value) ? o.Value : _NaturalOrderExpr.Replace(o.Value, m => m.Value.PadLeft(max, '0')))
						 .Select(o => o.Key);
		}

		public static IEnumerable<TSource> OrderByDescendingNatural<TSource, TKey>(
			this IEnumerable<TSource> source, Func<TSource, TKey> selector)
		{
			int max = 0;

			var selection = source.Select(
				o =>
				{
					var v = selector(o);
					var s = v != null ? v.ToString() : String.Empty;

					if (!String.IsNullOrWhiteSpace(s))
					{
						var mc = _NaturalOrderExpr.Matches(s);

						if (mc.Count > 0)
						{
							max = Math.Max(max, mc.Cast<Match>().Max(m => m.Value.Length));
						}
					}

					return new
					{
						Key = o,
						Value = s
					};
				}).ToList();

			return
				selection.OrderByDescending(
					o =>
					String.IsNullOrWhiteSpace(o.Value) ? o.Value : _NaturalOrderExpr.Replace(o.Value, m => m.Value.PadLeft(max, '0')))
						 .Select(o => o.Key);
		}
	}
}