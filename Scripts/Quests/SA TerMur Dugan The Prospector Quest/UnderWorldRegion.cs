using System;
using System.Xml;
using Server.Spells.Chivalry;
using Server.Spells.Fourth;
using Server.Spells.Seventh;
using Server.Spells.Sixth;

namespace Server.Regions
{
    public class UnderWorldRegion : BaseRegion
    {
        public UnderWorldRegion(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        { 
        }

        public override void OnEnter(Mobile m)
        {

            m.SendLocalizedMessage(1094954); // You observe the remains of four humans here.  As you observe the tragic scene, 
            //Effects.PlaySound(m.Location, m.Map, 0x0F7);//you are reminded that you promised to bring evidence to Elder Dugan of their fate.

            base.OnEnter(m);
        }

        public override void OnExit(Mobile m)
        {

            //m.SendMessage("The intense energy dissipates.");
            //Effects.PlaySound(m.Location, m.Map, 0x0FB);
            base.OnExit(m);

        }

        public override bool AllowHousing(Mobile from, Point3D p)
        {
            if (from.AccessLevel == AccessLevel.Player)
                return false;
            else
                return base.AllowHousing(from, p);
        }

        public override bool OnBeginSpellCast(Mobile m, ISpell s)
        {
            if ((s is RecallSpell || s is MarkSpell || s is GateTravelSpell || s is SacredJourneySpell) && m.AccessLevel == AccessLevel.Player)
            {
                m.SendMessage("You can cast that spell here.");
                return true;
            }
            else
            {
                return base.OnBeginSpellCast(m, s);
            }
        }
    }
}

