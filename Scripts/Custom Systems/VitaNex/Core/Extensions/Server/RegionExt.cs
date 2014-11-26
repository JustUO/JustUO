#region Header
//   Vorspire    _,-'/-'/  RegionExt.cs
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
using System.Reflection;
using System.Threading.Tasks;

using VitaNex;
using VitaNex.Crypto;
using VitaNex.FX;
using VitaNex.Network;
#endregion

namespace Server
{
	public static class RegionExtUtility
	{
		public class RegionSerial : CryptoHashCode
		{
			public override string Value { get { return base.Value.Replace("-", String.Empty); } }

			public RegionSerial(Region r)
				: base(
					CryptoHashType.MD5,
					r.GetType().FullName + r.Map.Name + r.Name + r.Area.GetBoundsHashCode() + TimeStamp.UtcNow.Stamp +
					Utility.RandomDouble())
			{ }

			public RegionSerial(GenericReader reader)
				: base(reader)
			{ }
		}

		public class PreviewRegion : Region, IEquatable<Region>
		{
			private static ulong _UID;

			public RegionSerial Serial { get; private set; }

			public PollTimer Timer { get; private set; }
			public EffectInfo[] Effects { get; private set; }

			public DateTime Expire { get; set; }
			public int EffectID { get; set; }
			public int EffectHue { get; set; }
			public EffectRender EffectRender { get; set; }

			public PreviewRegion(Region r)
				: this(r.Name, r.Map, r.Area)
			{
				Serial = GetSerial(r);
			}

			public PreviewRegion(string name, Map map, params Rectangle2D[] area)
				: base(name + " " + _UID++, map, null, area)
			{
				Serial = new RegionSerial(this);
				EnsureDefaults();
			}

			public PreviewRegion(string name, Map map, params Rectangle3D[] area)
				: base(name + " " + _UID++, map, null, area)
			{
				Serial = new RegionSerial(this);
				EnsureDefaults();
			}

			public void EnsureDefaults()
			{
				Expire = DateTime.UtcNow.AddSeconds(300.0);

				EffectID = 1801;
				EffectHue = 85;
				EffectRender = EffectRender.ShadowOutline;
			}

			public void Refresh()
			{
				Expire = DateTime.UtcNow.AddSeconds(300.0);

				Register();
			}

			public override void OnRegister()
			{
				base.OnRegister();

				Expire = DateTime.UtcNow.AddSeconds(300.0);

				if (Effects == null)
				{
					var effects = new EffectInfo[Area.Length][,];

					effects.SetAll(i => new EffectInfo[Area[i].Width,Area[i].Height]);

					for (int index = 0; index < Area.Length; index++)
					{
						var b = Area[index];

						int xSpacing = Math.Max(1, Math.Min(16, b.Width / 8));
						int ySpacing = Math.Max(1, Math.Min(16, b.Height / 8));

						int minX = Math.Min(b.Start.X, b.End.X);
						int maxX = Math.Max(b.Start.X, b.End.X);

						int minY = Math.Min(b.Start.Y, b.End.Y);
						int maxY = Math.Max(b.Start.Y, b.End.Y);

						Parallel.For(
							minX,
							maxX,
							x => Parallel.For(
								minY,
								maxY,
								y =>
								{
									if (x != b.Start.X && x != b.End.X - 1 && x % xSpacing != 0 //
										&& y != b.Start.Y && y != b.End.Y - 1 && y % ySpacing != 0)
									{
										return;
									}

									int idxX = x - minX;
									int idxY = y - minY;

									effects[index][idxX, idxY] = new EffectInfo(
										new Point3D(x, y, 0), Map, EffectID, EffectHue, 1, 25, EffectRender);
								}));
					}

					Effects = effects.AsParallel().SelectMany(list => list.OfType<EffectInfo>()).ToArray();

					foreach (var e in Effects)
					{
						e.SetSource(e.Source.GetWorldTop(e.Map));
					}
				}

				if (Timer == null)
				{
					Timer = PollTimer.FromSeconds(
						1.0,
						() =>
						{
							if (DateTime.UtcNow > Expire)
							{
								Unregister();
								return;
							}

							foreach (var e in Effects)
							{
								e.Send();
							}
						},
						() => Registered);
				}
				else
				{
					Timer.Running = true;
				}

				_Previews.AddOrReplace(Serial, this);
			}

			public override void OnUnregister()
			{
				base.OnUnregister();

				if (Timer != null)
				{
					Timer.Running = false;
					Timer = null;
				}

				if (Effects != null)
				{
					foreach (var e in Effects)
					{
						e.Dispose();
					}

					Effects.SetAll(i => null);
					Effects = null;
				}

				Expire = DateTime.UtcNow;

				_Previews.Remove(Serial);
			}

			public override bool AcceptsSpawnsFrom(Region region)
			{
				if (Parent != null)
				{
					return Parent.AcceptsSpawnsFrom(region);
				}

				return base.AcceptsSpawnsFrom(region);
			}

			public override bool AllowBeneficial(Mobile m, Mobile target)
			{
				if (Parent != null)
				{
					return Parent.AllowBeneficial(m, target);
				}

				return base.AllowBeneficial(m, target);
			}

			public override bool AllowHarmful(Mobile m, Mobile target)
			{
				if (Parent != null)
				{
					return Parent.AllowHarmful(m, target);
				}

				return base.AllowHarmful(m, target);
			}

			public override bool AllowHousing(Mobile m, Point3D p)
			{
				if (Parent != null)
				{
					return Parent.AllowHousing(m, p);
				}

				return base.AllowHousing(m, p);
			}

			public override bool AllowSpawn()
			{
				if (Parent != null)
				{
					return Parent.AllowSpawn();
				}

				return base.AllowSpawn();
			}

			public override void AlterLightLevel(Mobile m, ref int global, ref int personal)
			{
				if (Parent != null)
				{
					Parent.AlterLightLevel(m, ref global, ref personal);
					return;
				}

				base.AlterLightLevel(m, ref global, ref personal);
			}

			public override bool CanUseStuckMenu(Mobile m)
			{
				if (Parent != null)
				{
					return Parent.CanUseStuckMenu(m);
				}

				return base.CanUseStuckMenu(m);
			}

			public override bool CheckAccessibility(Item item, Mobile m)
			{
				if (Parent != null)
				{
					return Parent.CheckAccessibility(item, m);
				}

				return base.CheckAccessibility(item, m);
			}

			public override TimeSpan GetLogoutDelay(Mobile m)
			{
				if (Parent != null)
				{
					return Parent.GetLogoutDelay(m);
				}

				return base.GetLogoutDelay(m);
			}

			public override Type GetResource(Type type)
			{
				if (Parent != null)
				{
					return Parent.GetResource(type);
				}

				return base.GetResource(type);
			}

			public override void OnAggressed(Mobile aggressor, Mobile aggressed, bool criminal)
			{
				if (Parent != null)
				{
					Parent.OnAggressed(aggressor, aggressed, criminal);
					return;
				}

				base.OnAggressed(aggressor, aggressed, criminal);
			}

			public override bool OnBeforeDeath(Mobile m)
			{
				if (Parent != null)
				{
					return Parent.OnBeforeDeath(m);
				}

				return base.OnBeforeDeath(m);
			}

			public override bool OnBeginSpellCast(Mobile m, ISpell s)
			{
				if (Parent != null)
				{
					return Parent.OnBeginSpellCast(m, s);
				}

				return base.OnBeginSpellCast(m, s);
			}

			public override void OnBeneficialAction(Mobile helper, Mobile target)
			{
				if (Parent != null)
				{
					Parent.OnBeneficialAction(helper, target);
					return;
				}

				base.OnBeneficialAction(helper, target);
			}

			public override void OnChildAdded(Region child)
			{
				if (Parent != null)
				{
					Parent.OnChildAdded(child);
					return;
				}

				base.OnChildAdded(child);
			}

			public override void OnChildRemoved(Region child)
			{
				if (Parent != null)
				{
					Parent.OnChildRemoved(child);
					return;
				}

				base.OnChildRemoved(child);
			}

			public override bool OnCombatantChange(Mobile m, Mobile Old, Mobile New)
			{
				if (Parent != null)
				{
					return Parent.OnCombatantChange(m, Old, New);
				}

				return base.OnCombatantChange(m, Old, New);
			}

			public override void OnCriminalAction(Mobile m, bool message)
			{
				if (Parent != null)
				{
					Parent.OnCriminalAction(m, message);
					return;
				}

				base.OnCriminalAction(m, message);
			}

			public override bool OnDamage(Mobile m, ref int damage)
			{
				if (Parent != null)
				{
					return Parent.OnDamage(m, ref damage);
				}

				return base.OnDamage(m, ref damage);
			}

			public override void OnDeath(Mobile m)
			{
				if (Parent != null)
				{
					Parent.OnDeath(m);
					return;
				}

				base.OnDeath(m);
			}

			public override bool OnDecay(Item item)
			{
				if (Parent != null)
				{
					return Parent.OnDecay(item);
				}

				return base.OnDecay(item);
			}

			public override void OnDidHarmful(Mobile harmer, Mobile harmed)
			{
				if (Parent != null)
				{
					Parent.OnDidHarmful(harmer, harmed);
					return;
				}

				base.OnDidHarmful(harmer, harmed);
			}

			public override bool OnDoubleClick(Mobile m, object o)
			{
				if (Parent != null)
				{
					return Parent.OnDoubleClick(m, o);
				}

				return base.OnDoubleClick(m, o);
			}

			public override void OnEnter(Mobile m)
			{
				if (Parent != null)
				{
					Parent.OnEnter(m);
					return;
				}

				base.OnEnter(m);
			}

			public override void OnExit(Mobile m)
			{
				if (Parent != null)
				{
					Parent.OnExit(m);
					return;
				}

				base.OnExit(m);
			}

			public override void OnGotBeneficialAction(Mobile helper, Mobile target)
			{
				if (Parent != null)
				{
					Parent.OnGotBeneficialAction(helper, target);
					return;
				}

				base.OnGotBeneficialAction(helper, target);
			}

			public override void OnGotHarmful(Mobile harmer, Mobile harmed)
			{
				if (Parent != null)
				{
					Parent.OnGotHarmful(harmer, harmed);
					return;
				}

				base.OnGotHarmful(harmer, harmed);
			}

			public override bool OnHeal(Mobile m, ref int heal)
			{
				if (Parent != null)
				{
					return Parent.OnHeal(m, ref heal);
				}

				return base.OnHeal(m, ref heal);
			}

			public override void OnLocationChanged(Mobile m, Point3D oldLocation)
			{
				if (Parent != null)
				{
					Parent.OnLocationChanged(m, oldLocation);
					return;
				}

				base.OnLocationChanged(m, oldLocation);
			}

			public override bool OnMoveInto(Mobile m, Direction d, Point3D newLocation, Point3D oldLocation)
			{
				if (Parent != null)
				{
					return Parent.OnMoveInto(m, d, newLocation, oldLocation);
				}

				return base.OnMoveInto(m, d, newLocation, oldLocation);
			}

			public override bool OnResurrect(Mobile m)
			{
				if (Parent != null)
				{
					return Parent.OnResurrect(m);
				}

				return base.OnResurrect(m);
			}

			public override bool OnSingleClick(Mobile m, object o)
			{
				if (Parent != null)
				{
					return Parent.OnSingleClick(m, o);
				}

				return base.OnSingleClick(m, o);
			}

			public override bool OnSkillUse(Mobile m, int skill)
			{
				if (Parent != null)
				{
					return Parent.OnSkillUse(m, skill);
				}

				return base.OnSkillUse(m, skill);
			}

			public override void OnSpeech(SpeechEventArgs args)
			{
				if (Parent != null)
				{
					Parent.OnSpeech(args);
					return;
				}

				base.OnSpeech(args);
			}

			public override void OnSpellCast(Mobile m, ISpell s)
			{
				if (Parent != null)
				{
					Parent.OnSpellCast(m, s);
					return;
				}

				base.OnSpellCast(m, s);
			}

			public override bool OnTarget(Mobile m, Targeting.Target t, object o)
			{
				if (Parent != null)
				{
					return Parent.OnTarget(m, t, o);
				}

				return base.OnTarget(m, t, o);
			}

			public override void SpellDamageScalar(Mobile caster, Mobile target, ref double damage)
			{
				if (Parent != null)
				{
					Parent.SpellDamageScalar(caster, target, ref damage);
					return;
				}

				base.SpellDamageScalar(caster, target, ref damage);
			}

			public override bool SendInaccessibleMessage(Item item, Mobile m)
			{
				if (Parent != null)
				{
					return Parent.SendInaccessibleMessage(item, m);
				}

				return base.SendInaccessibleMessage(item, m);
			}

			public bool Equals(Region other)
			{
				return !ReferenceEquals(other, null) && Serial.Equals(GetSerial(other));
			}
		}

		private static readonly Dictionary<RegionSerial, Region> _Regions;
		private static readonly Dictionary<RegionSerial, PreviewRegion> _Previews;

		static RegionExtUtility()
		{
			_Regions = new Dictionary<RegionSerial, Region>();
			_Previews = new Dictionary<RegionSerial, PreviewRegion>();
		}

		public static RegionSerial GetSerial(this Region r)
		{
			if (r == null)
			{
				return null;
			}

			if (r is PreviewRegion)
			{
				return ((PreviewRegion)r).Serial;
			}

			lock (_Regions)
			{
				if (_Regions.ContainsValue(r))
				{
					return _Regions.GetKey(r);
				}

				var s = new RegionSerial(r);

				_Regions.AddOrReplace(s, r);

				//Console.WriteLine("Region Serial: ('{0}', '{1}', '{2}') = {3}", r.GetType().Name, r.Map, r.Name, s.ValueHash);

				return s;
			}
		}

		public static PreviewRegion DisplayPreview(
			string name,
			Map map,
			int hue = 85,
			int effect = 1801,
			EffectRender render = EffectRender.SemiTransparent,
			params Rectangle2D[] bounds)
		{
			return DisplayPreview(new PreviewRegion(name, map, bounds), hue, effect, render);
		}

		public static PreviewRegion DisplayPreview(
			string name,
			Map map,
			int hue = 85,
			int effect = 1801,
			EffectRender render = EffectRender.SemiTransparent,
			params Rectangle3D[] bounds)
		{
			return DisplayPreview(new PreviewRegion(name, map, bounds), hue, effect, render);
		}

		public static PreviewRegion DisplayPreview(
			this Region r, int hue = 85, int effect = 1801, EffectRender render = EffectRender.SemiTransparent)
		{
			if (r == null || r.Area == null || r.Area.Length == 0)
			{
				return null;
			}

			if (hue < 0)
			{
				hue = 0;
			}

			if (effect <= 0)
			{
				effect = 1801;
			}

			PreviewRegion pr;

			if (r is PreviewRegion)
			{
				pr = (PreviewRegion)r;
				pr.EffectID = effect;
				pr.EffectHue = hue;
				pr.EffectRender = render;
				pr.Register();

				return pr;
			}

			var s = GetSerial(r);

			if (s == null)
			{
				return null;
			}

			if (_Previews.TryGetValue(s, out pr) && pr != null && pr.Area.GetBoundsHashCode() != r.Area.GetBoundsHashCode())
			{
				pr.Unregister();
				pr = null;
			}

			if (pr == null)
			{
				pr = new PreviewRegion(r)
				{
					EffectHue = hue,
					EffectID = effect,
					EffectRender = render
				};
			}

			pr.Register();

			return pr;
		}

		public static void ClearPreview(this Region r)
		{
			if (r == null)
			{
				return;
			}

			PreviewRegion pr;

			if (r is PreviewRegion)
			{
				pr = (PreviewRegion)r;
				pr.Unregister();

				return;
			}

			var s = GetSerial(r);

			if (s == null)
			{
				return;
			}

			if (_Previews.TryGetValue(s, out pr) && pr != null)
			{
				pr.Unregister();
			}
		}

		public static bool Contains(this Sector s, Point3D p, Map m)
		{
			return s.Owner == m && Contains(s, p);
		}

		public static bool Contains(this Region r, Point3D p, Map m)
		{
			return r.Map == m && Contains(r, p);
		}

		public static bool Contains(this RegionRect r, Point3D p, Map m)
		{
			return r.Region.Map == m && Contains(r, p);
		}

		public static bool Contains(this Sector s, Point3D p)
		{
			return s.RegionRects.Contains(p);
		}

		public static bool Contains(this Region r, Point3D p)
		{
			return r.Area.Contains(p);
		}

		public static bool Contains(this RegionRect r, Point3D p)
		{
			return r.Rect.Contains(p);
		}

		public static TRegion Create<TRegion>(params object[] args) where TRegion : Region
		{
			return Create(typeof(TRegion), args) as TRegion;
		}

		public static Region Create(Type type, params object[] args)
		{
			return type.CreateInstanceSafe<Region>(args);
		}

		public static TRegion Clone<TRegion>(this TRegion region, params object[] args) where TRegion : Region
		{
			if (region == null)
			{
				return null;
			}

			var fields = region.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

			fields.Remove("m_Serial");
			fields.Remove("m_Name");
			fields.Remove("m_Map");
			fields.Remove("m_Parent");
			fields.Remove("m_Area");
			fields.Remove("m_Sectors");
			fields.Remove("m_ChildLevel");
			fields.Remove("m_Registered");

			#region Remove Fields that apply to global standards (if any)
			fields.Remove("_Serial");
			fields.Remove("_Name");
			fields.Remove("_Map");
			fields.Remove("_Parent");
			fields.Remove("_Area");
			fields.Remove("_Sectors");
			fields.Remove("_ChildLevel");
			fields.Remove("_Registered");
			#endregion

			region.Unregister();

			var reg = Create(region.GetType(), args) as TRegion;

			if (reg != null)
			{
				fields.Serialize(reg);
				reg.Register();
			}

			return reg;
		}

		public static bool IsPartOf<TRegion>(this Region region) where TRegion : Region
		{
			return region != null && region.IsPartOf(typeof(TRegion));
		}

		public static TRegion GetRegion<TRegion>(this Region region) where TRegion : Region
		{
			return region != null ? region.GetRegion(typeof(TRegion)) as TRegion : null;
		}

		public static TRegion GetRegion<TRegion>(this Mobile m) where TRegion : Region
		{
			return m != null ? GetRegion<TRegion>(m.Region) : null;
		}

		public static Region GetRegion(this Mobile m, Type type)
		{
			return m != null && m.Region != null ? m.Region.GetRegion(type) : null;
		}

		public static Region GetRegion(this Mobile m, string name)
		{
			return m != null && m.Region != null ? m.Region.GetRegion(name) : null;
		}

		public static bool InRegion<TRegion>(this Mobile m) where TRegion : Region
		{
			return m != null && GetRegion<TRegion>(m.Region) != null;
		}

		public static bool InRegion(this Mobile m, Type type)
		{
			return m != null && GetRegion(m, type) != null;
		}

		public static bool InRegion(this Mobile m, string name)
		{
			return m != null && GetRegion(m, name) != null;
		}

		public static bool InRegion(this Mobile m, Region r)
		{
			return m != null && m.Region != null && m.Region.IsPartOf(r);
		}

		public static TRegion GetRegion<TRegion>(this Item i) where TRegion : Region
		{
			if (i == null)
			{
				return null;
			}

			var reg = Region.Find(i.GetWorldLocation(), i.Map);

			return reg != null ? GetRegion<TRegion>(reg) : null;
		}

		public static Region GetRegion(this Item i, string name)
		{
			if (i == null)
			{
				return null;
			}

			var reg = Region.Find(i.GetWorldLocation(), i.Map);

			return reg != null ? reg.GetRegion(name) : null;
		}

		public static Region GetRegion(this Item i, Type type)
		{
			if (i == null)
			{
				return null;
			}

			var reg = Region.Find(i.GetWorldLocation(), i.Map);

			return reg != null ? reg.GetRegion(type) : null;
		}

		public static bool InRegion<TRegion>(this Item i) where TRegion : Region
		{
			if (i == null)
			{
				return false;
			}

			var reg = Region.Find(i.GetWorldLocation(), i.Map);

			return reg != null && GetRegion<TRegion>(reg) != null;
		}

		public static bool InRegion(this Item i, Type type)
		{
			return i != null && GetRegion(i, type) != null;
		}

		public static bool InRegion(this Item i, string name)
		{
			return i != null && GetRegion(i, name) != null;
		}

		public static bool InRegion(this Item i, Region r)
		{
			if (i == null)
			{
				return false;
			}

			var reg = Region.Find(i.GetWorldLocation(), i.Map);

			return reg != null && reg.IsPartOf(r);
		}
	}
}