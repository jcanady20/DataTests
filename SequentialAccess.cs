using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DataTests
{
	public class SequentialAccess : CommonAccess
	{
		public override string OnGetFirstAddress(System.Data.IDbCommand cmd)
		{
			return GetAddresses(cmd).First();
		}

		public override IList<string> OnGetAllAddresses(System.Data.IDbCommand cmd)
		{
			return GetAddresses(cmd).ToList();
		}

		private IEnumerable<string> GetAddresses(IDbCommand cmd)
		{
			using(var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
			{
				while(reader.Read())
				{
					yield return reader.GetString(0);
				}
			}
		}

	}
}
