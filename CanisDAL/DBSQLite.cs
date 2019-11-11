using CanisModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.Common;
using CanisDAL.Utilities;

namespace CanisDAL
{
    public class DBSQLite : Database
    {
        public DBSQLite(string connectionString)
        {
            ConnectionString = connectionString;

            Tables = GetTables();
        }

        public void Initialize()
        {
            using (SQLiteConnection db = new SQLiteConnection(ConnectionString))
            {
                //db.Open();

                //String tableCommand = "CREATE TABLE IF NOT " +
                //    "EXISTS MyTable (Primary_Key INTEGER PRIMARY KEY, " +
                //    "Text_Entry NVARCHAR(2048) NULL)";

                //SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                //createTable.ExecuteReader();
            }
        }

        public override List<string> GetTables()
        {
            List<string> lista = new List<string>();
            DataTable dt = new DataTable();
            dt = GetDataSync("SELECT name FROM sqlite_master WHERE type = 'table';");
            if (dt.Rows.Count > 0)
                foreach (DataRow r in dt.Rows)
                    lista.Add(r[0].ToString());

            lista.RemoveAll(x => x.Contains("sqlite_sequence"));
            return lista;
        }

        //Check for updates on db
        public override async Task<LastModifies> CheckUpdates()
        {
            LastModifies LastLocalModifies = new LastModifies();

            foreach (string t in Tables)
            {
                DataTable dt = await GetData(String.Format("SELECT MAX(lastmodify) FROM {0};", t));
                if (dt.Rows[0][0].GetType() != typeof(DBNull))
                    LastLocalModifies.List.Add(t, Convert.ToDateTime(dt.Rows[0][0]));
            }

            return LastLocalModifies;
        }

        public override Task<List<string>> CheckUpdate()
        {
            throw new NotImplementedException();
        }

        public override Task<Cliente> CreateCliente()
        {
            SetData("INSERT INTO clienti (nome) VALUES ('Inserzio')");
            return null;
        }

        public override Task<Cliente> DeleteCliente(Cliente c)
        {
            throw new NotImplementedException();
        }

        public override async void SetData(string query)
        {
            using (SQLiteConnection db =
                 new SQLiteConnection(ConnectionString))
            {
                SQLiteCommand cmd = new SQLiteCommand(query, db);
                try
                {
                    db.Open();
                    await cmd.ExecuteNonQueryAsync();
                    db.Close();
                }
                catch (SQLiteException e)
                {
                    OnSQLiteError(new SQLiteErrorEventArgs(e));
                    db.Close();
                }
            }
        }

        public override async Task<DataTable> GetData(string query)
        {
            DataTable dt = new DataTable();
            using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
            {
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                try
                {
                    conn.Open();
                    DbDataReader reader;
                    reader = await cmd.ExecuteReaderAsync();

                    dt.Load(reader);
                    conn.Close();
                }
                catch (SQLiteException e)
                {
                    OnSQLiteError(new SQLiteErrorEventArgs(e));
                    conn.Close();
                }
                return dt;
            }
        }

        public DataTable GetDataSync(string query)
        {
            DataTable dt = new DataTable();
            using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
            {
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                try
                {
                    conn.Open();
                    SQLiteDataReader reader;
                    reader = cmd.ExecuteReader();

                    dt.Load(reader);
                    conn.Close();
                }
                catch (SQLiteException e)
                {
                    OnSQLiteError(new SQLiteErrorEventArgs(e));
                    conn.Close();
                }
                return dt;
            }
        }

        public override Task<ObservableCollection<Cliente>> GetClienti()
        {
            throw new NotImplementedException();
        }

        public override void InitTables()
        {
            throw new NotImplementedException();
        }

        public override Exception Test()
        {
            throw new NotImplementedException();
        }

    }
}
