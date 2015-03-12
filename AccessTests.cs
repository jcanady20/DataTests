using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTests
{
	public partial class Program
	{
		static void dtFirst()
		{
			using(var dta = new DataTableAccess())
			{
				dta.GetFirstAddress();
			}
		}

		static void dtAll()
		{
			using (var dta = new DataTableAccess())
			{
				dta.GetAllAddresses();
			}
		}

		static void sqFirst()
		{
			using(var sq = new SequentialAccess())
			{
				sq.GetFirstAddress();
			}
		}

		static void sqAll()
		{
			using (var sq = new SequentialAccess())
			{
				sq.GetAllAddresses();
			}
		}
	}
}
