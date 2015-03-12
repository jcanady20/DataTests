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
	public class DataTableAccess : CommonAccess
	{
		public override string OnGetFirstAddress(IDbCommand cmd)
		{
			var dt = GetAddresses(cmd);
			return dt.Rows[0][0].ToString();
		}

		public override IList<string> OnGetAllAddresses(IDbCommand cmd)
		{
			var addresses = new List<string>();
			var dt = GetAddresses(cmd);
			foreach(DataRow dr in dt.Rows)
			{
				addresses.Add(dr[0].ToString());
			}
			return addresses;
		}

		private DataTable GetAddresses(IDbCommand cmd)
		{
			var dt = new DataTable();
			using(var reader = cmd.ExecuteReader())
			{
				dt.Load(reader);
			}
			return dt;
		}

	}
}
