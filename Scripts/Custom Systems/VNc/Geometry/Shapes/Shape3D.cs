#region Header
//   Vorspire    _,-'/-'/  Shape3D.cs
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
using Server.Commands;
using Server.Items;
using Server.Mobiles;

using VitaNex.Targets;
#endregion

namespace VitaNex.Geometry
{
	public static class Shapes
	{
		private static bool _Configured;

		public static Type[] Types { get; private set; }

		static Shapes()
		{
			Types = typeof(Shape3D).GetConstructableChildren();
		}

		public static TShape CreateInstance<TShape>(IPoint3D center, params object[] args)
		{
			var a = new object[1 + args.Length];

			a[0] = center;

			if (args.Length > 0)
			{
				args.CopyTo(a, 1);
			}

			args = a;

			return typeof(TShape).CreateInstanceSafe<TShape>(args);
		}

		public static void Configure()
		{
			if (_Configured)
			{
				return;
			}

			CommandUtility.Register("AddShape3D", AccessLevel.GameMaster, OnCommand);

			_Configured = true;
		}

		private static void OnCommand(CommandEventArgs e)
		{
			if (e == null || e.Mobile == null || !(e.Mobile is PlayerMobile))
			{
				return;
			}

			PlayerMobile m = (PlayerMobile)e.Mobile;

			if (e.Arguments == null || e.Arguments.Length == 0)
			{
				return;
			}

			string shapeName = e.Arguments[0].Trim().ToLower();
			int val1, val2;

			if (e.Arguments.Length < 2 || !Int32.TryParse(e.Arguments[1].Trim(), out val1))
			{
				val1 = 5;
			}

			if (e.Arguments.Length < 3 || !Int32.TryParse(e.Arguments[2].Trim(), out val2))
			{
				val2 = 5;
			}

			val1 = Math.Max(0, Math.Min(10, val1));
			val2 = e.Arguments.Length < 3 ? val1 : Math.Max(0, Math.Min(10, val2));

			GenericSelectTarget<IPoint3D>.Begin(
				m,
				(mob, targ) =>
				{
					Point3D loc = targ.Clone3D(0, 0, Math.Max(val1, val2) * 5);

					Shape3D shape;

					switch (shapeName)
					{
						case "sphere":
							shape = new Sphere3D(loc, val1, false);
							break;
						case "ring":
							shape = new Ring3D(loc, val1, val2);
							break;
						case "disc":
							shape = new Disc3D(loc, val1, false);
							break;
						case "cylendar":
							shape = new Cylendar3D(loc, val1, false);
							break;
						case "cube":
							shape = new Cube3D(loc, val1, false);
							break;
						default:
							return;
					}

					shape.ForEach(
						b =>
						{
							Static block = new Static(1801);

							block.MoveToWorld(b, m.Map);
						});
				},
				mob => { },
				-1,
				true);
		}
	}

	[PropertyObject]
	public abstract class Shape3D : DynamicWireframe, IPoint3D
	{
		private bool _InitialRender;

		public override List<Block3D> Blocks
		{
			get
			{
				if (!_InitialRender)
				{
					Render();
				}

				return base.Blocks;
			}
			set { base.Blocks = value; }
		}

		public override int Volume
		{
			get
			{
				if (!_InitialRender)
				{
					Render();
				}

				return base.Volume;
			}
		}

		protected Point3D _Center;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Point3D Center
		{
			get { return _Center; }
			set
			{
				if (_Center == value)
				{
					return;
				}

				_Center = value;
				Render();
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int X
		{
			get { return _Center.X; }
			set
			{
				if (_Center.X == value)
				{
					return;
				}

				_Center.X = value;
				Render();
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Y
		{
			get { return _Center.Y; }
			set
			{
				if (_Center.Z == value)
				{
					return;
				}

				_Center.Y = value;
				Render();
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Z
		{
			get { return _Center.Z; }
			set
			{
				if (_Center.Z == value)
				{
					return;
				}

				_Center.Z = value;
				Render();
			}
		}

		public Shape3D()
			: this(Point3D.Zero)
		{ }

		public Shape3D(IPoint3D center)
		{
			_Center = center.Clone3D();
		}

		public Shape3D(GenericReader reader)
			: base(reader)
		{ }

		public void Render()
		{
			if (Rendering)
			{
				return;
			}

			_InitialRender = Rendering = true;

			Clear();
			OnRender();

			Rendering = false;
		}

		protected abstract void OnRender();

		public override void Add(Block3D item)
		{
			if (!_InitialRender)
			{
				Render();
			}

			base.Add(item);
		}

		public override void AddRange(IEnumerable<Block3D> collection)
		{
			if (!_InitialRender)
			{
				Render();
			}

			base.AddRange(collection);
		}

		public override bool Remove(Block3D item)
		{
			if (!_InitialRender)
			{
				Render();
			}

			return base.Remove(item);
		}

		public override int RemoveAll(Predicate<Block3D> match)
		{
			if (!_InitialRender)
			{
				Render();
			}

			return base.RemoveAll(match);
		}

		public override void RemoveAt(int index)
		{
			if (!_InitialRender)
			{
				Render();
			}

			base.RemoveAt(index);
		}

		public override void RemoveRange(int index, int count)
		{
			if (!_InitialRender)
			{
				Render();
			}

			base.RemoveRange(index, count);
		}

		public override void ForEach(Action<Block3D> action)
		{
			if (!_InitialRender)
			{
				Render();
			}

			base.ForEach(action);
		}

		public override IEnumerator<Block3D> GetEnumerator()
		{
			if (!_InitialRender)
			{
				Render();
			}

			return base.GetEnumerator();
		}

		public override Block3D this[int index]
		{
			get
			{
				if (!_InitialRender)
				{
					Render();
				}

				return base[index];
			}
			set
			{
				if (!_InitialRender)
				{
					Render();
				}

				base[index] = value;
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			if (!_InitialRender)
			{
				Render();
			}

			base.Serialize(writer);

			writer.SetVersion(0);

			writer.Write(_Center);
		}

		public override void Deserialize(GenericReader reader)
		{
			_InitialRender = true;

			base.Deserialize(reader);

			reader.GetVersion();

			_Center = reader.ReadPoint3D();
		}
	}
}