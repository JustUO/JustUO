#region Header
// **************************************\
//     _  _   _   __  ___  _   _   ___   |
//    |# |#  |#  |## |### |#  |#  |###   |
//    |# |#  |# |#    |#  |#  |# |#  |#  |
//    |# |#  |#  |#   |#  |#  |# |#  |#  |
//   _|# |#__|#  _|#  |#  |#__|# |#__|#  |
//  |##   |##   |##   |#   |##    |###   |
//        [http://www.playuo.org]        |
// **************************************/
//  [2014] Party.cs
// ************************************/
#endregion

namespace Server
{
	public abstract class PartyCommands
	{
		public static PartyCommands Handler { get; set; }

		public abstract void OnAdd(Mobile from);
		public abstract void OnRemove(Mobile from, Mobile target);
		public abstract void OnPrivateMessage(Mobile from, Mobile target, string text);
		public abstract void OnPublicMessage(Mobile from, string text);
		public abstract void OnSetCanLoot(Mobile from, bool canLoot);
		public abstract void OnAccept(Mobile from, Mobile leader);
		public abstract void OnDecline(Mobile from, Mobile leader);
	}
}