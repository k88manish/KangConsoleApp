using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletIntegration
{
	public static class Database
	{
		private static string connectionString = @"Data Source=|DataDirectory|\AppDB.db;";

		public static void ExecuteQuery(string query)
		{
			using (SQLiteConnection m_dbConnection = new SQLiteConnection(connectionString))
			{
				try
				{
					m_dbConnection.Open();
					using (SQLiteCommand cmd = new SQLiteCommand(query, m_dbConnection))
					{
						cmd.ExecuteNonQuery();
					}
				}
				catch (Exception ex)
				{
					Util.HandleError(ex);
				}
			}
		}

		public static SQLiteDataReader ExecuteDataReader(string query)
		{
			SQLiteConnection m_dbConnection = new SQLiteConnection(connectionString);
			try
			{
				m_dbConnection.Open();
				using (SQLiteCommand cmd = new SQLiteCommand(query, m_dbConnection))
				{
					return cmd.ExecuteReader();
				}
			}
			catch (Exception ex)
			{
				Util.HandleError(ex);
				throw;
			}
		}
	}
}