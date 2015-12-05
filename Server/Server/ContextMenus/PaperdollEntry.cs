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
//  [2014] PaperdollEntry.cs
// ************************************/
#endregion

namespace Server.ContextMenus
{
	public class PaperdollEntry : ContextMenuEntry
	{
		private readonly Mobile m_Mobile;

		public PaperdollEntry(Mobile m)
			: base(6123, Core.GlobalUpdateRange)
		{
			m_Mobile = m;
		}

		public override void OnClick()
		{
			if (m_Mobile.CanPaperdollBeOpenedBy(Owner.From))
			{
				m_Mobile.DisplayPaperdollTo(Owner.From);
			}
		}
	}
}