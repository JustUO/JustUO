#region Header
//   Vorspire    _,-'/-'/  DigitalDisplayTest.cs
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
using System.Globalization;

using Server;
using Server.Commands;
using Server.Mobiles;

using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Sandbox.Tests
{
	public sealed class DigitalDisplayTest : ISandboxTest
	{
		public DigitalDisplayTest()
		{
			Tests = new Dictionary<PlayerMobile, PollTimer>();
		}

		public Dictionary<PlayerMobile, PollTimer> Tests { get; private set; }

		public void EntryPoint()
		{
			CommandSystem.Register(
				"DigitalTest1",
				AccessLevel.GameMaster,
				e => Sandbox.SafeInvoke(
					() =>
					{
						var split = e.ArgString.Trim().Split(new[] {' ', ','}, StringSplitOptions.RemoveEmptyEntries);

						var numbers = new int[split.Length];

						if (numbers.Length == 0)
						{
							numbers = new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
						}
						else
						{
							for (int i = 0; i < split.Length; i++)
							{
								numbers[i] = Math.Max(0, Math.Min(9, Int32.Parse(split[i])));
							}
						}

						SuperGump.Send(new DigitalNumericDisplayGump(e.Mobile as PlayerMobile, numbers: numbers));
					},
					this));

			CommandSystem.Register(
				"DigitalTest2",
				AccessLevel.GameMaster,
				e => Sandbox.SafeInvoke(
					() =>
					{
						var pm = e.Mobile as PlayerMobile;

						if (pm == null)
						{
							return;
						}

						if (Tests.ContainsKey(pm))
						{
							if (Tests[pm] != null)
							{
								Tests[pm].Stop();
							}
						}
						else
						{
							Tests.Add(pm, null);
						}

						int i = 0;
						const int iMax = 100;

						Tests[pm] = PollTimer.CreateInstance(
							TimeSpan.FromMilliseconds(100.0),
							() =>
							{
								string numStr = i.ToString(CultureInfo.InvariantCulture);
								var numbers = new int[numStr.Length];

								for (int j = 0; j < numbers.Length; j++)
								{
									numbers[j] = Math.Max(0, Math.Min(9, Int32.Parse(numStr[j].ToString(CultureInfo.InvariantCulture))));
								}

								var instances = SuperGump.GetInstances<DigitalNumericDisplayGump>(pm);

								if (instances.Length != 0)
								{
									DigitalNumericDisplayGump g = instances[0];

									g.NumericWidth += 1;
									g.NumericHeight += 2;

									g.Numerics = numbers;
									g.Refresh();
								}
								else
								{
									var g = new DigitalNumericDisplayGump(e.Mobile as PlayerMobile, numbers: numbers);
									g.Send();
								}

								i++;

								if (i < iMax || !Tests.ContainsKey(pm))
								{
									return;
								}

								Tests[pm].Stop();
								Tests.Remove(pm);
							},
							() => i <= iMax);
					},
					this));

			CommandSystem.Register(
				"DigitalTest3",
				AccessLevel.GameMaster,
				e => Sandbox.SafeInvoke(
					() =>
					{
						var pm = e.Mobile as PlayerMobile;

						if (pm == null)
						{
							return;
						}

						if (Tests.ContainsKey(pm))
						{
							if (Tests[pm] != null)
							{
								Tests[pm].Stop();
							}
						}
						else
						{
							Tests.Add(pm, null);
						}

						int i = 60;
						const int iMin = 0;

						Tests[pm] = PollTimer.CreateInstance(
							TimeSpan.FromSeconds(1.0),
							() =>
							{
								string numStr = i.ToString(CultureInfo.InvariantCulture);
								var numbers = new int[numStr.Length];

								for (int j = 0; j < numbers.Length; j++)
								{
									numbers[j] = Math.Max(0, Math.Min(9, Int32.Parse(numStr[j].ToString(CultureInfo.InvariantCulture))));
								}

								var instances = SuperGump.GetInstances<DigitalNumericDisplayGump>(pm);

								if (instances.Length != 0)
								{
									DigitalNumericDisplayGump g = instances[0];

									g.NumericWidth -= 1;
									g.NumericHeight -= 2;

									g.Numerics = numbers;
									g.Refresh();
								}
								else
								{
									var g = new DigitalNumericDisplayGump(
										e.Mobile as PlayerMobile, numericWidth: 60, numericHeight: 120, numbers: numbers);
									g.Send();
								}

								i--;

								if (i > 0 || !Tests.ContainsKey(pm))
								{
									return;
								}

								Tests[pm].Stop();
								Tests.Remove(pm);
							},
							() => i >= iMin);
					},
					this));
		}

		public void OnSuccess()
		{ }

		public void OnException(Exception e)
		{
			Sandbox.CSOptions.ToConsole(e);
		}
	}
}