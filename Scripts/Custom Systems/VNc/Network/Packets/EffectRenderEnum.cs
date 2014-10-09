#region Header
//   Vorspire    _,-'/-'/  EffectRenderEnum.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

namespace VitaNex.Network
{
	public enum EffectRender
	{
		Normal = 0x00,
		Multiply = 0x01,
		Lighten = 0x02,
		LightenMore = 0x03,
		Darken = 0x04,
		SemiTransparent = 0x05,
		ShadowOutline = 0x06,
		Transparent = 0x07
	}
}