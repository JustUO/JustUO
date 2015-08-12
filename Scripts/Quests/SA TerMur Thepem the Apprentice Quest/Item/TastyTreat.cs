using System;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
    public class TastyTreat : Item
    {
        [Constructable]
        public TastyTreat()
            : base(0xF7E)
        {
            Hue = 1745;

            Used = false;
        }

        public TastyTreat(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1113003; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Used { get; set; }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            list.Add(1113213);
            list.Add(1113214);
            list.Add(1070722, "Duration: 20 Min");
            list.Add(1042971, "Cooldown: 2 Min");
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!Used)
            {
                from.SendMessage("which animal you want to Targhet ?");

                from.Target = new InternalTarget(this);
            }
            else
            {
                from.SendLocalizedMessage(1113051);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version

            writer.Write(Used);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();

            Used = reader.ReadBool();
        }

        private class InternalTarget : Target
        {
            private int Change1;
            private int Change2;
            private int Change3;
            private int Change4;
            private int Change5;
            private int Change6;
            private readonly TastyTreat m_Tasty;

            public InternalTarget(TastyTreat tasty)
                : base(10, false, TargetFlags.None)
            {
                m_Tasty = tasty;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                var pm = from as PlayerMobile;

                if (m_Tasty.Deleted)
                    return;

                if (targeted is BaseCreature)
                {
                    var creature = (BaseCreature) targeted;

                    Change1 = (int) ((creature.RawStr)*1.05);
                    Change2 = (int) ((creature.RawDex)*1.05);
                    Change3 = (int) ((creature.RawInt)*1.05);

                    Change4 = creature.RawStr;
                    Change5 = creature.RawDex;
                    Change6 = creature.RawInt;

                    if ((creature.Controlled || creature.Summoned) && (from == creature.ControlMaster) &&
                        !(creature.Asleep))
                    {
                        creature.FixedParticles(0x373A, 10, 15, 5018, EffectLayer.Waist);
                        creature.PlaySound(0x1EB);

                        creature.RawStr = Change1;
                        creature.RawDex = Change2;
                        creature.RawInt = Change3;

                        from.SendMessage("You have increased the Stats of your pet by 5% for 20 Minutes!!");
                        m_Tasty.Used = true;
                        creature.Asleep = true;

                        Timer.DelayCall(TimeSpan.FromMinutes(20.0), delegate
                        {
                            creature.RawStr = this.Change4;
                            creature.RawDex = this.Change5;
                            creature.RawInt = this.Change6;
                            creature.PlaySound(0x1DF);

                            this.m_Tasty.Used = true;
                            creature.Asleep = false;
                            from.SendMessage("The effect of Tasty Treat is Finish !");

                            Timer.DelayCall(TimeSpan.FromMinutes(2.0), delegate { this.m_Tasty.Used = false; });
                        });
                    }
                    else if ((creature.Controlled || creature.Summoned) && (from == creature.ControlMaster) &&
                             (creature.Asleep))
                    {
                        from.SendLocalizedMessage(502676);
                    }
                    else
                    {
                        from.SendLocalizedMessage(1113049);
                    }
                }
                else
                {
                    from.SendLocalizedMessage(500329);
                }
            }
        }
    }
}