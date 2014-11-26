#region Header
//   Vorspire    _,-'/-'/  Updates.cs
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
using System.Text;

using Server;
using Server.Mobiles;

using VitaNex.Http;
using VitaNex.Schedules;
#endregion

namespace VitaNex.Updates
{
	public static partial class UpdateService
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		public const string DefaultURL = "http://core.vita-nex.com/svn/VERSION";

		private static Timer _Timeout;
		private static VersionInfo _RemoteVersion;

		public static VersionInfo LocalVersion { get { return VitaNexCore.Version; } }

		public static VersionInfo RemoteVersion
		{
			get { return _RemoteVersion; }
			private set
			{
				_RemoteVersion = value;
				RemoteVersion.Name = "VitaNexCore";
				RemoteVersion.Description = "Represents the remote version value of Vita-Nex: Core";
			}
		}

		public static UpdateServiceOptions CSOptions { get; private set; }

		public static Uri URL { get; set; }

		public static List<PlayerMobile> Staff { get; private set; }

		public static event Action<VersionInfo, VersionInfo> OnVersionResolved;

		private static void OnScheduleTick(Schedule s)
		{
			RequestVersion();
		}

		public static void RequestVersion()
		{
			if (_Timeout != null && _Timeout.Running)
			{
				CSOptions.ToConsole("Previous request has not been handled yet.");
				return;
			}

			CSOptions.ToConsole("Requesting remote version...");

			_Timeout = Timer.DelayCall(
				TimeSpan.FromMilliseconds(CSOptions.Timeout.TotalMilliseconds + 1000),
				() =>
				{
					CSOptions.ToConsole("Request timed-out.");

					NotifyStaff("Update request failed, the connection timed-out.", true, 1.0, 10.0);
				});

			VitaNexCore.TryCatch(
				() =>
				HttpService.SendRequest(
					URL != null ? URL.ToString() : DefaultURL,
					(int)CSOptions.Timeout.TotalMilliseconds,
					(i, send, receive) =>
					{
						if (URL == null)
						{
							URL = i.URL;
						}

						string rcv = String.Join(String.Empty, receive.GetContent());

						OnDataReceived(rcv);

						if (_Timeout == null)
						{
							return;
						}

						_Timeout.Stop();
						_Timeout = null;
					}),
				CSOptions.ToConsole);
		}

		private static void OnDataReceived(string data)
		{
			CSOptions.ToConsole("{0} bytes of data received, parsing...", Encoding.Default.GetByteCount(data));

			VersionInfo version;

			if (!VersionInfo.TryParse(data, out version))
			{
				CSOptions.ToConsole("The remote version could not be resolved.");
				NotifyStaff("Update request failed, the remote version could not be resolved.", true, 1.0, 10.0);
				return;
			}

			RemoteVersion = version;

			CSOptions.ToConsole("Remote version resolved as {0}", RemoteVersion);

			if (LocalVersion >= RemoteVersion)
			{
				NotifyStaff(
					String.Format("No updates are available, your version [b]{0}[/b] is up-to-date.", LocalVersion), true, 1.0, 10.0);
			}
			else
			{
				NotifyStaff(
					String.Format(
						"Updates are available, your version [b]{0}[/b] is out-of-date, the remote version is [b]{1}[/b].",
						LocalVersion,
						RemoteVersion),
					true,
					1.0,
					10.0);
			}

			if (OnVersionResolved != null)
			{
				OnVersionResolved(LocalVersion, RemoteVersion);
			}
		}

		private static void NotifyStaff(string message, bool autoClose = true, double delay = 1.0, double pause = 3.0)
		{
			if (!CSOptions.NotifyStaff)
			{
				return;
			}

			Staff.RemoveRange(m => !m.IsOnline() || m.Account.AccessLevel < CSOptions.NotifyAccess);
			Staff.TrimExcess();

			message = String.Format("[url=http://core.vita-nex.com]Vita-Nex: Core[/url][br]{0}", message);

			Staff.ForEach(m => m.SendNotification<UpdatesNotifyGump>(message, autoClose, delay, pause));
		}
	}
}