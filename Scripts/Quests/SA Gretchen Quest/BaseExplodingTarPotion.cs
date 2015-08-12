using System;
using System.Collections;
using System.Collections.Generic;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Targeting;

namespace Server.Items
{
    public abstract class BaseExplodingTarPotion : BasePotion
    {
        private readonly List<Mobile> m_Users = new List<Mobile>();

        public BaseExplodingTarPotion(PotionEffect effect) : base(0xF06, effect)
        {
            Hue = 1109;
        }

        public BaseExplodingTarPotion(Serial serial) : base(serial)
        {
        }

        public abstract int Radius { get; }

        public override bool RequireFreeHand
        {
            get { return false; }
        }

        public override void Drink(Mobile from)
        {
            if (Core.AOS && (from.Paralyzed || from.Frozen || (from.Spell != null && from.Spell.IsCasting)))
            {
                from.SendLocalizedMessage(1062725); // You can not use that potion while paralyzed.
                return;
            }

            var delay = GetDelay(from);

            if (delay > 0)
            {
                from.SendLocalizedMessage(1072529, String.Format("{0}\t{1}", delay, delay > 1 ? "seconds." : "second."));
                    // You cannot use that for another ~1_NUM~ ~2_TIMEUNITS~
                return;
            }

            var targ = from.Target as ThrowTarget;

            if (targ != null && targ.Potion == this)
                return;

            from.RevealingAction();

            if (!m_Users.Contains(from))
                m_Users.Add(from);

            from.Target = new ThrowTarget(this);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }

        public void Explode_Callback(object state)
        {
            var states = (object[]) state;

            Explode((Mobile) states[0], (Point3D) states[1], (Map) states[2]);
        }

        public virtual void Explode(Mobile from, Point3D loc, Map map)
        {
            if (Deleted || map == null)
                return;

            Consume();

            // Check if any other players are using this potion
            for (var i = 0; i < m_Users.Count; i ++)
            {
                var targ = m_Users[i].Target as ThrowTarget;

                if (targ != null && targ.Potion == this)
                    Target.Cancel(from);
            }

            // Effects
            Effects.PlaySound(loc, map, 0x207);

            Geometry.Circle2D(loc, map, Radius, TarEffect, 270, 90);

            Timer.DelayCall(TimeSpan.FromSeconds(1), new TimerStateCallback(CircleEffect2), new object[] {loc, map});

            foreach (var mobile in map.GetMobilesInRange(loc, Radius))
            {
                if (mobile != from)
                {
                    if (mobile is PlayerMobile)
                    {
                        var player = (PlayerMobile) mobile;

                        player.SendLocalizedMessage(1095151);
                    }

                    mobile.Send(SpeedControl.WalkSpeed);

                    Timer.DelayCall(TimeSpan.FromMinutes(1.0), delegate { mobile.Send(SpeedControl.Disable); });
                }
            }
        }

        private class ThrowTarget : Target
        {
            public ThrowTarget(BaseExplodingTarPotion potion) : base(12, true, TargetFlags.None)
            {
                Potion = potion;
            }

            public BaseExplodingTarPotion Potion { get; private set; }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (Potion.Deleted || Potion.Map == Map.Internal)
                    return;

                var p = targeted as IPoint3D;

                if (p == null || from.Map == null)
                    return;

                // Add delay
                AddDelay(from);

                SpellHelper.GetSurfaceTop(ref p);

                from.RevealingAction();

                IEntity to;

                if (p is Mobile)
                    to = (Mobile) p;
                else
                    to = new Entity(Serial.Zero, new Point3D(p), from.Map);

                Effects.SendMovingEffect(from, to, 0xF0D, 7, 0, false, false, Potion.Hue, 0);
                Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerStateCallback(Potion.Explode_Callback),
                    new object[] {from, new Point3D(p), from.Map});
            }
        }

        #region Effects

        public virtual void TarEffect(Point3D p, Map map)
        {
            if (map.CanFit(p, 12, true, false))
                Effects.SendLocationEffect(p, map, 0x376A, 4, 9);
        }

        public void CircleEffect2(object state)
        {
            var states = (object[]) state;

            Geometry.Circle2D((Point3D) states[0], (Map) states[1], Radius, TarEffect, 90, 270);
        }

        #endregion

        #region Delay

        private static readonly Hashtable m_Delay = new Hashtable();

        public static void AddDelay(Mobile m)
        {
            var timer = m_Delay[m] as Timer;

            if (timer != null)
                timer.Stop();

            m_Delay[m] = Timer.DelayCall(TimeSpan.FromSeconds(60), new TimerStateCallback(EndDelay_Callback), m);
        }

        public static int GetDelay(Mobile m)
        {
            var timer = m_Delay[m] as Timer;

            if (timer != null && timer.Next > DateTime.Now)
                return (int) (timer.Next - DateTime.Now).TotalSeconds;

            return 0;
        }

        private static void EndDelay_Callback(object obj)
        {
            if (obj is Mobile)
                EndDelay((Mobile) obj);
        }

        public static void EndDelay(Mobile m)
        {
            var timer = m_Delay[m] as Timer;

            if (timer != null)
            {
                timer.Stop();
                m_Delay.Remove(m);
            }
        }

        #endregion
    }
}