#region Header
//   Vorspire    _,-'/-'/  SuperGumpLayout.cs
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
using System.Linq;
#endregion

namespace VitaNex.SuperGumps
{
	public class SuperGumpLayout : Dictionary<string, Action<string>>
	{
		public SuperGumpLayout()
			: this(null)
		{ }

		public SuperGumpLayout(IEnumerable<KeyValuePair<string, Action<string>>> entries)
		{
			if (entries != null)
			{
				entries.ForEach(kv => AddReplace(kv.Key, kv.Value));
			}
		}

		public void Replace(string xpath, Action value)
		{
			if (String.IsNullOrWhiteSpace(xpath) || !ContainsKey(xpath))
			{
				return;
			}

			if (value == null)
			{
				Remove(xpath);
				return;
			}

			this[xpath] = x => value();
		}

		public void Replace(string xpath, Action<string> value)
		{
			if (String.IsNullOrWhiteSpace(xpath) || !ContainsKey(xpath))
			{
				return;
			}

			if (value == null)
			{
				Remove(xpath);
				return;
			}

			this[xpath] = value;
		}

		public void AddReplace(string xpath, Action value)
		{
			if (String.IsNullOrWhiteSpace(xpath))
			{
				return;
			}

			if (value == null)
			{
				if (ContainsKey(xpath))
				{
					Remove(xpath);
				}

				return;
			}

			if (!ContainsKey(xpath))
			{
				Add(xpath, x => value());
			}
			else
			{
				this[xpath] = x => value();
			}
		}

		public void AddReplace(string xpath, Action<string> value)
		{
			if (!ContainsKey(xpath))
			{
				Add(xpath, value);
			}
			else
			{
				this[xpath] = value;
			}
		}

		public void Add(string xpath, Action value)
		{
			Add(
				xpath,
				x =>
				{
					if (value != null)
					{
						value();
					}
				});
		}

		public void AddBefore(string search, string xpath, Action value)
		{
			AddBefore(
				search,
				xpath,
				x =>
				{
					if (value != null)
					{
						value();
					}
				});
		}

		public void AddBefore(string search, string xpath, Action<string> value)
		{
			int index = Keys.IndexOf(search);

			if (index != -1)
			{
				var tmp = new Dictionary<string, Action<string>>();

				this.For(
					(i, k, v) =>
					{
						if (i == index)
						{
							tmp.Add(xpath, value);
						}

						tmp.Add(k, v);
					});

				Clear();

				foreach (var kv in tmp)
				{
					Add(kv.Key, kv.Value);
				}

				tmp.Clear();
			}
			else
			{
				Add(xpath, value);
			}
		}

		public void AddAfter(string search, string xpath, Action value)
		{
			AddAfter(
				search,
				xpath,
				x =>
				{
					if (value != null)
					{
						value();
					}
				});
		}

		public void AddAfter(string search, string xpath, Action<string> value)
		{
			int index = Keys.IndexOf(search);

			if (index != -1)
			{
				var tmp = new Dictionary<string, Action<string>>();

				this.For(
					(i, k, v) =>
					{
						if (i == index + 1)
						{
							tmp.Add(xpath, value);
						}

						tmp.Add(k, v);
					});

				Clear();

				foreach (var kv in tmp)
				{
					Add(kv.Key, kv.Value);
				}

				tmp.Clear();
			}
			else
			{
				Add(xpath, value);
			}
		}

		public virtual void ApplyTo(SuperGump gump)
		{
			foreach (var kvp in this.Where(kvp => !String.IsNullOrEmpty(kvp.Key) && kvp.Value != null))
			{
				kvp.Value(kvp.Key);
			}
		}
	}
}