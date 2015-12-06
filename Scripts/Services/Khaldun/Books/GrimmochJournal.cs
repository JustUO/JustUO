using System;
using Server;

namespace Server.Items
{
	public class GrimmochJournal1 : BaseBook
	{
		[Constructable]
		public GrimmochJournal1()
			: base( Utility.Random( 0xFF1, 2 ), "The daily journal of Grimmoch Drummel", "Grimmoch", 5, false )
		{
			Pages[0].Lines = new string[] { "Day One :", "", "'Tis a grand sight, this", "primeval tomb, I agree", "with Tavara on that.", "And we've a good crew", "here, they've strong", "backs and a good" };

			Pages[1].Lines = new string[] { "attitude.  I'm a bit", "concerned by those", "that worked as guides", "for us, however.  All", "seemed well enough", "until we revealed the", "immense stone doors", "of the tomb structure" };

			Pages[2].Lines = new string[] { "itself.  Seemed to send", "a shiver up their", "spines and get them all", "stirred up with", "whispering.  I'll", "watch the lot of them", "with a close eye, but", "I'm confident we won't" };

			Pages[3].Lines = new string[] { "have any real", "problems on the dig.", "I'm especially proud to", "see Thomas standing", "out - he was a good", "hire, despite the", "warnings from his", "previous employers." };

			Pages[4].Lines = new string[] { "He's drummed up the", "workers into a", "furious pace - we've", "nearly halved the", "estimate on the", "timeline for", "excavating the Tomb's", "entrance." };
		}

		public GrimmochJournal1( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class GrimmochJournal2 : BaseBook
	{
		[Constructable]
		public GrimmochJournal2()
			: base( Utility.Random( 0xFF1, 2 ), "The daily journal of Grimmoch Drummel", "Grimmoch", 7, false )
		{
			Pages[0].Lines = new string[] { "Day Two :", "", "We managed to dig out", "the last of the", "remaining rubble", "today, revealing the", "entirety of the giant", "stone doors that sealed" };

			Pages[1].Lines = new string[] { "ol' Khal Ankur and", "his folk up ages ago.", "Actually getting them", "open was another", "matter altogether,", "however.  As the", "workers set to the", "task with picks and" };

			Pages[2].Lines = new string[] { "crowbars, I could have", "sworn I saw Lysander", "Gathenwale fiddling", "with something in that", "musty old tome of his.", " I've no great", "knowledge of things", "magical, but the way" };

			Pages[3].Lines = new string[] { "his hand moved over", "that book, and the look", "of concentration on his", "face as he whispered", "something to himself", "looked like every", "description of an", "incantation I've ever" };

			Pages[4].Lines = new string[] { "heard.  The strange", "thing is, this set of", "doors that an entire", "crew of excavators", "was laboring over for", "hours, right when", "Gathenwale finishes", "with his mumbling..." };

			Pages[5].Lines = new string[] { "well, I swore the doors", "just gave open at the", "exact moment he", "spoke his last bit of", "whisper and shut the", "tome tight in his", "hands.  When he", "looked up, it was" };

			Pages[6].Lines = new string[] { "almost as if he was", "expecting the doors to", "be open, rather than", "shocked that they'd", "finally given way." };
		}

		public GrimmochJournal2( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class GrimmochJournal3 : BaseBook
	{
		[Constructable]
		public GrimmochJournal3()
			: base( Utility.Random( 0xFF1, 2 ), "The daily journal of Grimmoch Drummel", "Grimmoch", 7, false )
		{
			Pages[0].Lines = new string[] { "Day Three - Day Five:", "", "I might have", "written too hastily in", "my first entry - this", "place doesn't seem too", "bent on giving up any", "secrets.   Though the" };

			Pages[1].Lines = new string[] { "main antechamber is", "open to us, the main", "exit hall is blocked by", "yet another pile of", "rubble.  Doesn't look a", "bit like anything", "caused by a quake or", "instability in the" };

			Pages[2].Lines = new string[] { "stonework... I swear it", "looks as if someone", "actually piled the", "stones up themselves,", "some time after the", "tomb was built.  The", "stones aren't of the", "same set nor quality" };

			Pages[3].Lines = new string[] { "of the carved work", "that surrounds them", "- if anything, they", "resemble the grade of", "common rock we saw", "in great quantities on", "the trip here.  Which", "makes it feel all the" };

			Pages[4].Lines = new string[] { "more like someone", "hauled them in and", "deliberately covered", "this passage.  But then", "why not decorate them", "in the same ornate", "manner as the rest of", "the stone in this" };

			Pages[5].Lines = new string[] { "place?  Lysander", "wouldn't hear a word", "of what I had to say -", "to him, it was a quake", "some time in the", "history of the tomb,", "and that was it, shut", "up and move on.  So I" };

			Pages[6].Lines = new string[] { "shut up, and got back", "to work." };
		}

		public GrimmochJournal3( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class GrimmochJournal6 : BaseBook
	{
		[Constructable]
		public GrimmochJournal6()
			: base( Utility.Random( 0xFF1, 2 ), "The daily journal of Grimmoch Drummel", "Grimmoch", 6, false )
		{
			Pages[0].Lines = new string[] { "Day Six :", "", "The camp was", "attacked last night by", "a pack of, well, I don't", "have a clue.  I've never", "seen the like of these", "beasts anywhere." };

			Pages[1].Lines = new string[] { "Huge things, with", "fangs the size of your", "forefinger, covered in", "hair and with the", "strangest arched back", "I've ever seen.  And so", "many of them.  We", "were forced back into" };

			Pages[2].Lines = new string[] { "the Tomb for the", "night, just to keep our", "hides on us.  And", "today Gathenwale", "practically orders us", "all to move the entire", "exterior camp into the", "Tomb.  Now, I don't" };

			Pages[3].Lines = new string[] { "disagree that we'd be", "well off to use the", "place as a point of", "fortification... but I", "don't like it one bit, in", "any case.  I don't like", "the look of this place,", "nor the sound of it." };

			Pages[4].Lines = new string[] { "The way the wind", "gets into the", "passageways,", "whistling up the", "strangest noises.", "Deep, sustained echoes", "of the wind, not so", "much flute-like as..." };

			Pages[5].Lines = new string[] { "well, it sounds", "ridiculous.  In any", "case, we've set to work", "moving the bulk of the", "exterior camp into the", "main antechamber, so", "there's no use moaning", "about it now." };
		}

		public GrimmochJournal6( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class GrimmochJournal7 : BaseBook
	{
		[Constructable]
		public GrimmochJournal7()
			: base( Utility.Random( 0xFF1, 2 ), "The daily journal of Grimmoch Drummel", "Grimmoch", 5, false )
		{
			Pages[0].Lines = new string[] { "Day Seven - Day Ten:", "", "I cannot stand this", "place, I cannot bear it.", "I've got to get out.", "Something evil lurks", "in this ancient place,", "something best left" };

			Pages[1].Lines = new string[] { "alone.  I hear them,", "yet none of the others", "do.  And yet they", "must.  Hands, claws,", "scratching at stone,", "the awful scratching", "and the piteous cries", "that sound almost like" };

			Pages[2].Lines = new string[] { "laughter.  I can hear", "them above even the", "cracks of the", "workmen's picks, and", "at night they are all I", "can hear.  And yet the", "others hear nothing.", "We must leave this" };

			Pages[3].Lines = new string[] { "place, we must.", "Three workers have", "gone missing - Tavara", "expects they've", "abandoned us - and I", "count them lucky if", "they have.  I don't care", "what the others say," };

			Pages[4].Lines = new string[] { "we must leave this", "place.  We must do as", "those before and pile", "up the stones, block all", "access to this primeval", "crypt, seal it up again", "for all eternity." };
		}

		public GrimmochJournal7( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class GrimmochJournal11 : BaseBook
	{
		[Constructable]
		public GrimmochJournal11()
			: base( Utility.Random( 0xFF1, 2 ), "The daily journal of Grimmoch Drummel", "Grimmoch", 4, false )
		{
			Pages[0].Lines = new string[] { "Day Eleven - Day", "Thirteen :", "", "Lysander is gone, and", "two more workers", "with him.  Good", "riddance to the first.", "He knows something." };

			Pages[1].Lines = new string[] { "He heard them too, I", "know he did - and yet", "he scowled at me", "when I mentioned", "them.  I cannot stop", "the noise in my head,", "the scratching, the", "clawing tears at my" };

			Pages[2].Lines = new string[] { "senses.  What is it?", "What does Lysander", "seek that I can only", "turn from?  Where", "has he gone?  The", "only answer to my", "questions comes as", "laughter from behind" };

			Pages[3].Lines = new string[] { "the stones." };
		}

		public GrimmochJournal11( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class GrimmochJournal14 : BaseBook
	{
		[Constructable]
		public GrimmochJournal14()
			: base( Utility.Random( 0xFF1, 2 ), "The daily journal of Grimmoch Drummel", "Grimmoch", 5, false )
		{
			Pages[0].Lines = new string[] { "Day Fourteen - Day", "Sixteen :", "", "We are lost... we are", "lost... all is lost.  The", "dead are piled up at", "my feet.  Bergen and I", "somehow managed in" };

			Pages[1].Lines = new string[] { "the madness to piece", "together a barricade,", "barring access to the", "camp antechamber.", "He knows as well as I", "that we cannot hold it", "forever.  The dead", "come.  They took" };

			Pages[2].Lines = new string[] { "Lysander before our", "eyes.  I pity the soul", "of even such a", "madman - no one", "should die in such a", "manner.  And yet so", "many have.  We're", "trapped here in this" };

			Pages[3].Lines = new string[] { "horror.  So many have", "died, and for what?", "What curse have we", "stumbled upon?  I", "cannot bear it, the", "moaning, wailing cries", "of the dead.  Poor", "Thomas, cut to pieces" };

			Pages[4].Lines = new string[] { "by their blades.  We", "had only an hour to", "properly bury those", "we could, before the", "undead legions struck", "again.  I cannot go on...", "I cannot go on." };
		}

		public GrimmochJournal14( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class GrimmochJournal17 : BaseBook
	{
		[Constructable]
		public GrimmochJournal17()
			: base( Utility.Random( 0xFF1, 2 ), "The daily journal of Grimmoch Drummel", "Grimmoch", 6, false )
		{
			Pages[0].Lines = new string[] { "Day Seventeen - Day", "Twenty-Two :", "", "The fighting never", "ceases... the blood", "never stops flowing,", "like a river through", "the bloated corpses of" };

			Pages[1].Lines = new string[] { "the dead.  And yet", "there are still more.", "Always more, with", "the red fire gleaming", "in their eyes.  My", "arm aches, I've taken", "to the sword as my", "bow seems to do little" };

			Pages[2].Lines = new string[] { "good... the dull ache in", "my arm... so many", "swings, cleaving a", "mountain of decaying", "flesh.  And Thomas...", "he was there, in the", "thick of it... Thomas", "was beside me..." };

			Pages[3].Lines = new string[] { "his face cleaved in", "twain - and yet beside", "me, fighting with us", "against the horde until", "he was cut down once", "again.  And I swear I", "see him even now,", "there in the dark" };

			Pages[4].Lines = new string[] { "corner of the", "antechamber, his eyes", "flickering in the last", "dying embers of the", "fire... and he stares at", "me, and a scream fills", "the vault - whether", "his or mine, I can no" };

			Pages[5].Lines = new string[] { "longer tell." };
		}

		public GrimmochJournal17( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class GrimmochJournal23 : BaseBook
	{
		[Constructable]
		public GrimmochJournal23()
			: base( Utility.Random( 0xFF1, 2 ), "The daily journal of Grimmoch Drummel", "Grimmoch", 1, false )
		{
			Pages[0].Lines = new string[] { "Day Twenty-Three :", "", "We no longer bury the", "dead." };
		}

		public GrimmochJournal23( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}
}
