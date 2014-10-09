#region Header
//   Vorspire    _,-'/-'/  ATTKTest.cs
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
using Server.Network;
#endregion

namespace VitaNex.Sandbox.Tests
{
	public sealed class ATTKTest : ISandboxTest
	{
		private static readonly string[] _Vars = new[]
		{
			"enjoy rock climbing", "watch paint dry", "poke dead things with a stick", "drink poison", "do archery",
			"paint epic scenes", "pee vee pee", "fletch arrows", "sail the seas", "clean Vorspire's shoes",
			"roll out Kon's red carpet", "play Ultima Online", "collect stamps", "res kill newbs", "grow plants",
			"slay mighty beasts", "dance for the King", "play Skyrim", "be a target", "be a hero", "adventure everywhere",
			"sell live-tock", "beta test", "pillage and burn villages", "drink every day", "fly high", "be good in bed",
			"satisfy your partner", "stare into space", "stare at pixels", "loot houses", "gank newbs", "jump around",
			"laugh out loud", "love April Fools Day", "have a knee", "say 'used to'", "go Faith Hilling", "tramp spotting",
			"kill stealing", "gamble a lot", "bench 200 pounds", "be a millionairre", "come early", "say random things"
		};

		private Timer _Timer;

		public void EntryPoint()
		{
			DateTime now = DateTime.UtcNow;

			if (now.Month != 4 || now.Day != 1)
			{
				return;
			}

			_Timer = Timer.DelayCall(TimeSpan.FromMinutes(2.0), TimeSpan.FromMinutes(Utility.RandomMinMax(5, 20)), OnTimerTick);
		}

		public void OnSuccess()
		{ }

		public void OnException(Exception e)
		{
			Sandbox.CSOptions.ToConsole(e);
		}

		private void OnTimerTick()
		{
			Sandbox.SafeInvoke(
				() =>
				{
					if (_Timer == null)
					{
						return;
					}

					DateTime now = DateTime.UtcNow;

					if (now.Month != 4 || now.Day != 1)
					{
						_Timer.Stop();
						_Timer = null;
						return;
					}

					foreach (NetState state in NetState.Instances.ToArray())
					{
						if (state == null || !state.Running || state.Mobile == null || state.Mobile.Deleted || state.Mobile.Hidden)
						{
							continue;
						}

						Mobile m = state.Mobile;
						m.Say("I used to {0}, then I took an arrow to the knee!", _Vars[Utility.Random(_Vars.Length)]);
					}

					_Timer.Interval = TimeSpan.FromMinutes(Utility.RandomMinMax(5, 20));
				},
				this);
		}
	}
}