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
//  [2014] IAccount.cs
// ************************************/
#endregion

#region References
using System;
#endregion

namespace Server.Accounting
{
	public interface IAccount : IComparable<IAccount>, IEquatable<IAccount>
	{
		[CommandProperty(AccessLevel.Administrator, true)]
		string Username { get; set; }

		[CommandProperty(AccessLevel.Administrator)]
		AccessLevel AccessLevel { get; set; }

		[CommandProperty(AccessLevel.Administrator)]
		int Length { get; }

		[CommandProperty(AccessLevel.Administrator)]
		int Limit { get; }

		[CommandProperty(AccessLevel.Administrator)]
		int Count { get; }

		Mobile this[int index] { get; set; }

		void Delete();
		void SetPassword(string password);
		bool CheckPassword(string password);
	}
}