#region Header
//   Vorspire    _,-'/-'/  Pair.cs
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

using Server;
#endregion

namespace VitaNex
{
	public static class Pair
	{
		public static Pair<T1, T2> Create<T1, T2>(T1 left, T2 right)
		{
			return Pair<T1, T2>.Create(left, right);
		}
	}

	/// <summary>
	///     It's kinda like a Tuple, but it's a Pair.
	/// </summary>
	[PropertyObject]
	public struct Pair<TLeft, TRight> : IEquatable<KeyValuePair<TLeft, TRight>>, IEquatable<Pair<TLeft, TRight>>
	{
		public static Pair<TLeft, TRight> Create(TLeft left, TRight right)
		{
			return new Pair<TLeft, TRight>(left, right);
		}

		public TLeft Left { get; private set; }
		public TRight Right { get; private set; }

		public Pair(Pair<TLeft, TRight> p)
			: this(p.Left, p.Right)
		{ }

		public Pair(KeyValuePair<TLeft, TRight> kvp)
			: this(kvp.Key, kvp.Value)
		{ }

		public Pair(TLeft left, TRight right)
			: this()
		{
			Left = left;
			Right = right;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Left != null ? Left.GetHashCode() : 0) * 397) ^ (Right != null ? Right.GetHashCode() : 0);
			}
		}

		public override bool Equals(object obj)
		{
			return !ReferenceEquals(null, obj) &&
				   ((obj is KeyValueString && Equals((KeyValueString)obj)) ||
					(obj is KeyValuePair<TLeft, TRight> && Equals((KeyValuePair<TLeft, TRight>)obj)));
		}

		public bool Equals(Pair<TLeft, TRight> other)
		{
			return Equals(Left, other.Left) && Equals(Right, other.Right);
		}

		public bool Equals(KeyValuePair<TLeft, TRight> other)
		{
			return Equals(Left, other.Key) && Equals(Right, other.Value);
		}

		public static bool operator ==(Pair<TLeft, TRight> l, Pair<TLeft, TRight> r)
		{
			return Equals(l, r);
		}

		public static bool operator !=(Pair<TLeft, TRight> l, Pair<TLeft, TRight> r)
		{
			return !Equals(l, r);
		}

		public static implicit operator KeyValuePair<TLeft, TRight>(Pair<TLeft, TRight> p)
		{
			return new KeyValuePair<TLeft, TRight>(p.Left, p.Right);
		}

		public static implicit operator Pair<TLeft, TRight>(KeyValuePair<TLeft, TRight> p)
		{
			return new Pair<TLeft, TRight>(p.Key, p.Value);
		}
	}
}