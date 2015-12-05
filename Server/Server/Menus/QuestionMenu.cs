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
//  [2014] QuestionMenu.cs
// ************************************/
#endregion

#region References
using Server.Network;
#endregion

namespace Server.Menus.Questions
{
	public class QuestionMenu : IMenu
	{
		private readonly string[] m_Answers;

		private readonly int m_Serial;
		private static int m_NextSerial;

		int IMenu.Serial { get { return m_Serial; } }

		int IMenu.EntryLength { get { return m_Answers.Length; } }

		public string Question { get; set; }

		public string[] Answers { get { return m_Answers; } }

		public QuestionMenu(string question, string[] answers)
		{
			Question = question;
			m_Answers = answers;

			do
			{
				m_Serial = ++m_NextSerial;
				m_Serial &= 0x7FFFFFFF;
			}
			while (m_Serial == 0);
		}

		public virtual void OnCancel(NetState state)
		{ }

		public virtual void OnResponse(NetState state, int index)
		{ }

		public void SendTo(NetState state)
		{
			state.AddMenu(this);
			state.Send(new DisplayQuestionMenu(this));
		}
	}
}