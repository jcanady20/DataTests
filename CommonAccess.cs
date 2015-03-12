using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DataTests
{
	public abstract class CommonAccess : IDisposable
	{
		private readonly string m_connString = @"Server=.\SqlExpress; Database=DataTests; Trusted_Connection=True;";
		private readonly string m_quryString = @"SELECT [EmailAddress] FROM [dbo].[Accounts]";
		private readonly string m_providerName = "System.Data.SqlClient";
		private IDbConnection m_sqlConnection;
		private IDbCommand m_sqlCommand;

		private Stopwatch m_stopWatch;
		internal CommonAccess()
		{
			m_sqlConnection = this.CreateConnection();
			m_sqlCommand = this.CreateCommand(m_sqlConnection);
		}

		public void GetFirstAddress()
		{
			Console.WriteLine("Started:: Getting First Address");
			startTimer();
			var result = this.OnGetFirstAddress(m_sqlCommand);
			var el = stopTimer();
			reportElapsed(el, 1);
			Console.WriteLine("Completed:: Getting First Address");
		}

		public void GetAllAddresses()
		{
			Console.WriteLine("Started:: Getting All Addresses");
			startTimer();
			var result = this.OnGetAllAddresses(m_sqlCommand);
			var el = stopTimer();
			reportElapsed(el, result.Count());
			Console.WriteLine("Completed:: Getting All Addresses");
		}

		public abstract string OnGetFirstAddress(IDbCommand cmd);
		public abstract IList<string> OnGetAllAddresses(IDbCommand cmd);

		private IDbConnection CreateConnection()
		{
			var factory = DbProviderFactories.GetFactory(m_providerName);
			var conn = factory.CreateConnection();
			conn.ConnectionString = m_connString;
			conn.Open();
			return conn;
		}

		private IDbCommand CreateCommand(IDbConnection connection)
		{
			if(connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			var cmd = connection.CreateCommand();
			cmd.CommandText = m_quryString;
			cmd.CommandType = CommandType.Text;

			return cmd;
		}

		private void startTimer()
		{
			if(m_stopWatch != null)
			{
				return;
			}

			m_stopWatch = new Stopwatch();
			m_stopWatch.Start();

		}

		private TimeSpan stopTimer()
		{
			if(m_stopWatch == null)
			{
				return new TimeSpan();
			}
			m_stopWatch.Stop();
			var el = m_stopWatch.Elapsed;
			m_stopWatch = null;
			return el;
		}
	
		private void reportElapsed(TimeSpan elapsed, int totalRecords)
		{
			Trace.WriteLine(String.Format("Retrieved {4} records in {0}:{1}:{2}.{3}", elapsed.Hours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds, totalRecords));
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if(disposing)
			{
				if (m_sqlCommand != null)
				{
					m_sqlCommand.Dispose();
					m_sqlCommand = null;
				}
				if(m_sqlConnection != null)
				{
					m_sqlConnection.Dispose();
					m_sqlConnection = null;
				}
			}
		}
	}
}
