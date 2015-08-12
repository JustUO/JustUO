namespace Server.Items
{
    [Flipable(0x1541, 0x1542)]
    public class SashOfMight : BodySash
    {
        //private SkillMod m_SkillMod0;
        // private SkillMod m_SkillMod1;

        [Constructable]
        public SashOfMight() : base(0x1541)
        {
            //Name = "The Sash of the Tamer";
            Hue = 0x481;

            //DefineMods();
        }

        public SashOfMight(Serial serial) : base(serial)
        {
            //DefineMods();

            //if ( Parent != null && this.Parent is Mobile ) 
            //	SetMods( (Mobile)Parent );
        }

        public override int LabelNumber
        {
            get { return 1075412; }
        } // Sash of Might
        /*private void DefineMods()
		{
            m_SkillMod0 = new DefaultSkillMod(SkillName.AnimalTaming, true, 10);
            m_SkillMod1 = new DefaultSkillMod(SkillName.AnimalLore, true, 10);
           
        }

		private void SetMods( Mobile wearer )
		{
            wearer.AddSkillMod(m_SkillMod0);
            wearer.AddSkillMod(m_SkillMod1);

		}

		public override bool OnEquip( Mobile from ) 
		{ 
			SetMods( from );
			return true;  
		} 

		public override void OnRemoved( object parent ) 
		{ 
			if ( parent is Mobile ) 
			{ 
				Mobile m = (Mobile)parent;
				m.RemoveStatMod( "SashoftheTamer" );

                if (m_SkillMod0 != null)
                    m_SkillMod0.Remove();

                if (m_SkillMod1 != null)
                    m_SkillMod1.Remove();


			} 
		} */

        public override void OnSingleClick(Mobile from)
        {
            LabelTo(from, Name);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();
        }
    }
}