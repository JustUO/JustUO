#region Header
//   Vorspire    _,-'/-'/  Ring3D.cs
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
#endregion

namespace VitaNex.Geometry
{
	public class Ring3D : Shape3D
	{
		private int _RadiusMin;
		private int _RadiusMax;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int RadiusMin
		{
			get { return _RadiusMin; }
			set
			{
				if (_RadiusMin == value)
				{
					return;
				}

				_RadiusMin = value;
				Render();
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int RadiusMax
		{
			get { return _RadiusMax; }
			set
			{
				if (_RadiusMax == value)
				{
					return;
				}

				_RadiusMax = value;
				Render();
			}
		}

		public Ring3D(int radius)
			: this(radius, radius)
		{ }

		public Ring3D(int radiusMin, int radiusMax)
			: this(Point3D.Zero, radiusMin, radiusMax)
		{ }

		public Ring3D(IPoint3D center, int radius)
			: this(center, radius, radius)
		{ }

		public Ring3D(IPoint3D center, int radiusMin, int radiusMax)
			: base(center)
		{
			_RadiusMin = Math.Min(radiusMin, radiusMax);
			_RadiusMax = Math.Max(radiusMin, radiusMax);
		}

		public Ring3D(GenericReader reader)
			: base(reader)
		{ }

		protected override void OnRender()
		{
			int min = Math.Min(RadiusMin, RadiusMax);
			int max = Math.Max(RadiusMin, RadiusMax);

			const int h = 5;

			for (int x = -max; x <= max; x++)
			{
				for (int y = -max; y <= max; y++)
				{
					int dist = (int)Math.Sqrt(x * x + y * y);

					if (dist >= min && dist <= max)
					{
						Add(new Block3D(Center.Clone3D(x, y), h));
					}
				}
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);

			writer.Write(_RadiusMin);
			writer.Write(_RadiusMax);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();

			_RadiusMin = reader.ReadInt();
			_RadiusMax = reader.ReadInt();
		}
	}
}