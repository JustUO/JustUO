#region Header
//   Vorspire    _,-'/-'/  Http_Init.cs
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
using System.Net;
#endregion

namespace VitaNex.Http
{
	[CoreService("Http", "1.0.0.0", TaskPriority.Highest)]
	public static partial class HttpService
	{
		static HttpService()
		{
			Requests = new Dictionary<HttpInfo, List<HttpWebRequest>>();
		}
	}
}