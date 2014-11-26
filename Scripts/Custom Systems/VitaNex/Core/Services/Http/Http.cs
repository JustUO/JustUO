#region Header
//   Vorspire    _,-'/-'/  Http.cs
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
using System.IO;
using System.Net;

using Server;
#endregion

namespace VitaNex.Http
{
	public static partial class HttpService
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		public static Dictionary<HttpInfo, List<HttpWebRequest>> Requests { get; private set; }

		public static event Action<HttpInfo, HttpWebRequest> OnSend;
		public static event Action<HttpInfo, HttpWebRequest, HttpWebResponse> OnReceive;

		public static void AbortAll()
		{
			Requests.Values.ForEach(
				list =>
				{
					list.ForEach(r => r.Abort());
					list.Clear();
				});
			Requests.Clear();
		}

		public static void Register(HttpInfo info, HttpWebRequest request)
		{
			if (info == null || request == null)
			{
				return;
			}

			Requests.AddOrReplace(
				info,
				list =>
				{
					list = list ?? new List<HttpWebRequest>();
					list.AddOrReplace(request);
					return list;
				});
		}

		public static bool Unregister(HttpInfo info, HttpWebRequest request)
		{
			bool retVal = false;

			var list = Requests.GetValue(info);

			if (list != null)
			{
				retVal = list.Remove(request);

				list.Free(false);

				if (list.Count == 0)
				{
					Requests.Remove(info);
				}
			}

			return retVal;
		}

		public static void InvokeSend(HttpInfo info, HttpWebRequest request)
		{
			if (info == null || request == null)
			{
				return;
			}

			if (OnSend != null)
			{
				OnSend(info, request);
			}
		}

		public static void InvokeReceive(HttpInfo info, HttpWebRequest request, HttpWebResponse response)
		{
			if (info == null || request == null || response == null)
			{
				return;
			}

			if (OnReceive != null)
			{
				OnReceive(info, request, response);
			}
		}

		public static void SendRequest(string url, Action<HttpInfo, HttpWebRequest, HttpWebResponse> callback)
		{
			SendRequest(url, (int)HttpInfo.DefaultTimeout.TotalMilliseconds, callback);
		}

		public static void SendRequest(string url, int timeout, Action<HttpInfo, HttpWebRequest, HttpWebResponse> callback)
		{
			var info = HttpInfo.Create(url, timeout);

			if (info != null)
			{
				info.Send(callback);
			}
		}

		public static string[] GetContent(this HttpWebResponse response)
		{
			return VitaNexCore.TryCatchGet(
				() =>
				{
					var lines = new string[0];

					using (var s = response.GetResponseStream())
					{
						if (s == null)
						{
							return lines;
						}

						using (StreamReader sr = new StreamReader(s))
						{
							var cache = new List<string>();

							while (!sr.EndOfStream)
							{
								cache.Add(sr.ReadLine());
							}

							lines = cache.ToArray();

							cache.Free(true);
						}

						s.Close();
					}

					return lines;
				});
		}
	}
}