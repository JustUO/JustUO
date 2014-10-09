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

using Server;
#endregion

namespace System
{
	public static class CollectionExtUtility
	{
		public static IEnumerator<TObj> GetEnumerator<TObj>(this TObj[] arr)
		{
			return (arr ?? new TObj[0]).AsEnumerable().GetEnumerator();
		}

		public static bool InBounds<T>(this T[] source, int index)
		{
			return source != null && index >= 0 && index < source.Length;
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

			var s = source.ToArray();
			var real = new T[s.Length][];

			s.For((i, p) => real[i] = p != null ? p.ToArray() : new T[0]);

			return real;
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
				ForEach(values, v => ForEach(source.Where(kv => Equals(kv.Value, v)).Select(kv => kv.Key), k => source.Remove(k)));
			}
		}

		/// <summary>
		///     Combines a multi-dimensional collection into a single-dimension collection.
		/// </summary>
		public static TEntry[] Combine<TEntry>(this IEnumerable<IEnumerable<TEntry>> source)
		{
			return source == null ? new TEntry[0] : source.SelectMany(e => e != null ? e.ToArray() : new TEntry[0]).ToArray();
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

			var arr = list.ToArray();

			for (int i = 0; i < arr.Length; i++)
			{
				action(i);
			}
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

			var arr = list.ToArray();

			for (int i = 0; i < arr.Length; i++)
			{
				action(i, arr[i]);
			}
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

			var arr = list.ToArray();

			for (int i = 0; i < arr.Length; i++)
			{
				action(i, arr[i].Key, arr[i].Value);
			}
		}

		/// <summary>
		///     Iterates through the elements in a clone of the given collection.
		///     This method is safe, the list can be manipulated by the action without causing "out of range", or "collection modified" exceptions.
		/// </summary>
		public static IEnumerable<TObj> ForEachChain<TObj>(this IEnumerable<TObj> list, Action<TObj> action)
		{
			if (list == null || action == null)
			{
				return list;
			}

			var arr = list.ToArray();

			foreach (TObj o in arr)
			{
				action(o);
			}

			return arr;
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

			var arr = list.ToArray();

			foreach (TObj o in arr)
			{
				action(o);
			}
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

			var arr = list.ToArray();

			if (offset < 0)
			{
				arr.Reverse();
			}

			int index = Math.Min(arr.Length, Math.Abs(offset));

			for (int i = index; i < arr.Length; i++)
			{
				action(i, arr[i]);
			}
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

			var arr = list.ToArray();

			index = Math.Max(0, Math.Min(arr.Length, index));

			if (count < 0)
			{
				arr.Reverse();
			}

			count = Math.Min(arr.Length, Math.Abs(count));

			for (int i = index; i < index + count && i < arr.Length; i++)
			{
				action(i, arr[i]);
			}
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

			foreach (var kvp in dictionary.ToArray())
			{
				action(kvp.Key, kvp.Value);
			}
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

			foreach (var kvp in dictionary.ToArray())
			{
				action(kvp);
			}
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

			var arr = dictionary.ToArray();

			if (offset < 0)
			{
				arr.Reverse();
			}

			int index = Math.Min(arr.Length, Math.Abs(offset));

			for (int i = index; i < arr.Length; i++)
			{
				action(i, arr[i]);
			}
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

			var arr = dictionary.ToArray();

			if (count < 0)
			{
				arr.Reverse();
			}

			count = Math.Min(arr.Length, Math.Abs(count));

			for (int i = index; i < index + count && i < arr.Length; i++)
			{
				action(i, arr[i]);
			}
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
			if (list == null || obj == null)
			{
				return -1;
			}

			int index = -1;
			bool found = false;

			foreach (var o in list)
			{
				index++;

				if (!ReferenceEquals(o, obj) && !o.Equals(obj))
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

			var arr = list.ToArray();
			return arr.Length == 0 ? def : arr[Utility.Random(arr.Length)];
		}

		public static IEnumerable<TObj> Not<TObj>(this IEnumerable<TObj> list, Func<TObj, bool> predicate)
		{
			if (list == null)
			{
				return new List<TObj>();
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

		public static void Shuffle<TObj>(this List<TObj> list)
		{
			if (list == null || list.Count == 0)
			{
				return;
			}

			Random rng = new Random();
			int n = list.Count;

			while (n > 1)
			{
				n--;

				int k = rng.Next(n + 1);
				TObj value = list[k];

				list[k] = list[n];
				list[n] = value;
			}
		}

		public static void Reverse<TObj>(this TObj[] list)
		{
			var buffer = new TObj[list.Length];

			list.CopyTo(buffer, 0);

			for (int i = 0, j = buffer.Length - 1; i < buffer.Length; i++, j--)
			{
				list[i] = buffer[j];
			}
		}

		public static TObj[] Dupe<TObj>(this TObj[] list)
		{
			if (list == null || list.Length == 0)
			{
				return new TObj[0];
			}

			var clone = new TObj[list.Length];
			list.CopyTo(clone, 0);
			return clone;
		}

		public static TObj[] Dupe<TObj>(this TObj[] list, Func<TObj, TObj> action)
		{
			if (list == null || list.Length == 0)
			{
				return new TObj[0];
			}

			if (action == null)
			{
				return Dupe(list);
			}

			var clone = new TObj[list.Length];

			for (int i = 0; i < list.Length; i++)
			{
				clone[i] = action(list[i]);
			}

			return clone;
		}

		public static TObj[] Merge<TObj>(this TObj[] list, TObj[] list2, params TObj[][] lists)
		{
			var merged = new List<TObj>(list.Length + list2.Length + lists.Sum(l => l.Length));

			merged.AddRange(list);
			merged.AddRange(list2);
			merged.AddRange(lists.SelectMany(l => l));

			return merged.ToArray();
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

			if (kvp.Key != null && dictionary.ContainsKey(kvp.Key))
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
			dictionary.Clear();
			dictionary.Add(key, value);

			d.ForEach(
				kvp =>
				{
					if (!dictionary.ContainsKey(kvp.Key))
					{
						dictionary.Add(kvp.Key, kvp.Value);
					}
				});
			d.Clear();
		}

		public static TKey GetKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value)
		{
			return dictionary == null || dictionary.Count == 0
					   ? default(TKey)
					   : dictionary.FirstOrDefault(kv => ReferenceEquals(kv.Value, value) || value.Equals(kv.Value)).Key;
		}

		public static TKey GetKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, int index)
		{
			return dictionary == null || dictionary.Count == 0 || index < 0 || index >= dictionary.Count
					   ? default(TKey)
					   : dictionary.Keys.ElementAt(index);
		}

		public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
		{
			return dictionary == null || dictionary.Count == 0
					   ? default(TValue)
					   : dictionary.FirstOrDefault(kv => ReferenceEquals(kv.Key, key) || key.Equals(kv.Key)).Value;
		}

		public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, int index)
		{
			return dictionary == null || dictionary.Count == 0 || index < 0 || index >= dictionary.Count
					   ? default(TValue)
					   : dictionary.Values.ElementAt(index);
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
			sourceN.For((i, v) => arr[i + 2] = v);

			return Merge(arr);
		}

		public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
			this IEnumerable<IDictionary<TKey, TValue>> collection,
			Func<IGrouping<TKey, TValue>, TKey> keySelector = null,
			Func<IGrouping<TKey, TValue>, TValue> elementSelector = null)
		{
			return collection == null
					   ? new Dictionary<TKey, TValue>()
					   : collection.SelectMany(d => d)
								   .ToLookup(kv => kv.Key, kv => kv.Value)
								   .ToDictionary(keySelector ?? (g => g.Key), elementSelector ?? (g => g.First()));
		}
	}
}