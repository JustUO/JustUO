#region Header
//   Vorspire    _,-'/-'/  HttpInfo.cs
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
using System.Net;
#endregion

namespace VitaNex.Http
{
	public sealed class HttpInfo
	{
		public static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10.0);

		public static HttpInfo Create(string url, int? timeout = null)
		{
			return VitaNexCore.TryCatchGet(
				() => new HttpInfo(new Uri(url), timeout ?? (int)DefaultTimeout.TotalMilliseconds), e => e.ToConsole(true, true));
		}

		public Uri URL { get; set; }
		public int Timeout { get; set; }

		public HttpInfo(Uri url, int? timeout = null)
		{
			URL = url;
			Timeout = timeout ?? (int)DefaultTimeout.TotalMilliseconds;
		}

		public void Send(Action<HttpInfo, HttpWebRequest, HttpWebResponse> callback = null)
		{
			Action action = () => VitaNexCore.TryCatch(
				() =>
				{
					var request = (HttpWebRequest)WebRequest.Create(URL);

					request.Timeout = Timeout;
					request.Proxy = null;
					request.Credentials = null;

					HttpService.Register(this, request);
					HttpService.InvokeSend(this, request);

					request.BeginGetResponse(AsyncRequestResult, Tuple.Create(request, callback));
				});

			action.BeginInvoke(action.EndInvoke, null);
		}

		private void AsyncRequestResult(IAsyncResult r)
		{
			VitaNexCore.TryCatch(
				() =>
				{
					var state = (Tuple<HttpWebRequest, Action<HttpInfo, HttpWebRequest, HttpWebResponse>>)r.AsyncState;
					var request = state.Item1;
					var callback = state.Item2;

					var response = (HttpWebResponse)request.EndGetResponse(r);

					if (!HttpService.Unregister(this, request))
					{
						return;
					}

					if (callback != null)
					{
						callback(this, request, response);
					}

					HttpService.InvokeReceive(this, request, response);
				});
		}
	}
}