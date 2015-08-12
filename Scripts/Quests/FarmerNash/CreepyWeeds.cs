using System;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
    public class CreepyWeeds : Item
    {
        [Constructable]
        public CreepyWeeds()
            : base(0x0CB8)
        {
            Name = "Creepy Weeds";
            Weight = 1;
            Movable = false;

            Timer.DelayCall(TimeSpan.FromMinutes(10.0), delegate { this.Delete(); });
        }

        public CreepyWeeds(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!CheckUse(from))
                return;
            var map = Map;
            var loc = Location;

            if (from.InRange(loc, 1) || from.InLOS(this))
            {
                (this).Delete();

                var snake = new Snake();
                var mongbat = new Mongbat();
                var silverserpent = new SilverSerpent();
                var raptor = new Raptor();
                var ballem = new Ballem();
                var fnpitchfork = new FNPitchfork();

                switch (Utility.Random(18))
                {
                    case 0:
                        snake.MoveToWorld(loc, map);
                        break;
                    case 1:
                        mongbat.MoveToWorld(loc, map);
                        break;
                    case 2:
                        silverserpent.MoveToWorld(loc, map);
                        break;
                    case 3:
                        raptor.MoveToWorld(loc, map);
                        break;
                    case 4:
                        ballem.MoveToWorld(loc, map);
                        break;
                    case 5:
                    case 10:
                    case 15:
                        if (Utility.RandomDouble() < 0.20)
                        {
                            fnpitchfork.MoveToWorld(loc, map);
                            from.SendMessage(
                                "You find Farmer Nash's pitchfork under one of the brambles of weeds. You pick up the pitchfork and put it in your backpack.");
                        }
                        break;
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadEncodedInt();
        }

        protected virtual bool CheckUse(Mobile from)
        {
            var pm = from as PlayerMobile;

            if (Deleted || !IsAccessibleTo(from))
            {
                return false;
            }
            if (@from.Map != Map || !@from.InRange(GetWorldLocation(), 1))
            {
                @from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
                return false;
            }
            return true;
        }
    }
}