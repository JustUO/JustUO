using System;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
    public class VialofArmorEssence : Item
    {
        [Constructable]
        public VialofArmorEssence()
            : base(0x5722)
        {
            Used = false;
        }

        public VialofArmorEssence(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1113018; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Used { get; set; }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            list.Add(1113213);
            list.Add(1113219);
            list.Add(1070722, "Duration: 10 Min");
            list.Add(1042971, "Cooldown: 2 Hours");
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!Used)
            {
                from.SendMessage("Which animal you want to Targhet ?");

                from.Target = new InternalTarget(this);
            }
            else
            {
                from.SendMessage("You must wait until the Effect of Vial of Armor Essence wears off !");
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
            private readonly VialofArmorEssence m_Tasty;

            public InternalTarget(VialofArmorEssence tasty)
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

                    if ((creature.Controlled || creature.Summoned) && (from == creature.ControlMaster) &&
                        !(creature.Asleep))
                    {
                        creature.FixedParticles(0x373A, 10, 15, 5018, EffectLayer.Waist);
                        creature.PlaySound(0x1EA);

                        var mod = new ResistanceMod(ResistanceType.Physical, +15);
                        var mod1 = new ResistanceMod(ResistanceType.Fire, +10);
                        var mod2 = new ResistanceMod(ResistanceType.Cold, +10);
                        var mod3 = new ResistanceMod(ResistanceType.Poison, +10);
                        var mod4 = new ResistanceMod(ResistanceType.Energy, +10);
                        creature.AddResistanceMod(mod);
                        creature.AddResistanceMod(mod1);
                        creature.AddResistanceMod(mod2);
                        creature.AddResistanceMod(mod3);
                        creature.AddResistanceMod(mod4);

                        from.SendMessage("You have increased the Damage Absorption of your pet by 10% for 10 Minutes !!");
                        m_Tasty.Used = true;
                        creature.Asleep = true;

                        Timer.DelayCall(TimeSpan.FromMinutes(10.0), delegate
                        {
                            var mod5 = new ResistanceMod(ResistanceType.Physical, -15);
                            var mod6 = new ResistanceMod(ResistanceType.Fire, -10);
                            var mod7 = new ResistanceMod(ResistanceType.Cold, -10);
                            var mod8 = new ResistanceMod(ResistanceType.Poison, -10);
                            var mod9 = new ResistanceMod(ResistanceType.Energy, -10);
                            creature.AddResistanceMod(mod5);
                            creature.AddResistanceMod(mod6);
                            creature.AddResistanceMod(mod7);
                            creature.AddResistanceMod(mod8);
                            creature.AddResistanceMod(mod9);
                            creature.PlaySound(0x1EB);

                            this.m_Tasty.Used = true;
                            creature.Asleep = false;
                            from.SendMessage("The effect of Vial of Armor Essence is finish !");

                            Timer.DelayCall(TimeSpan.FromMinutes(120.0), delegate { this.m_Tasty.Used = false; });
                        });
                    }
                    else if ((creature.Controlled || creature.Summoned) && (from == creature.ControlMaster) &&
                             (creature.Asleep))
                    {
                        from.SendMessage("Pet already under the influence of Vial of Armor Essence !");
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