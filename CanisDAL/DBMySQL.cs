using CanisModels;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace CanisDAL
{
    public class DBMySQL : Database
    {
        public DBMySQL(string server, string port, string databaseName, string username, string password, string dbtype, string apikey)
        {
            Server = server;
            Port = port;
            DatabaseName = databaseName;
            Username = username;
            Password = password;
            DbType = dbtype;
            ApiKey = apikey;
            ConnectionString = String.Format
                ("Server = {0}; Port = {1}; Database = {2}; SslMode = none; Uid = {3}; Pwd = {4}; Convert Zero Datetime=True;", Server, Port, DatabaseName, Username, Password);
        }

        #region Basic Functions
        public DateTime lastUpdate = new DateTime();//Data dell'ultima sincronizzazione del db
        public List<DateTime> lastDateUpdates = new List<DateTime>();
        

        //Execute query
        public async override void SetData(string query)
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                MySqlCommand cmd = new MySqlCommand(query, conn);
                try
                {
                    conn.Open();
                    await cmd.ExecuteNonQueryAsync();
                    conn.Close();
                }
                catch (MySqlException e)
                {
                    OnMySqlError(new MySqlErrorEventArgs(e));
                    conn.Close();
                }
            }
        }

        //Execute read query no async
        public DataTable GetDataNoAsync(string query)

        {
            DataTable DbImportato = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                MySqlCommand cmd = new MySqlCommand(query, conn);
                try
                {
                    conn.Open();
                    DbDataReader reader;
                    reader = cmd.ExecuteReader();

                    DbImportato.Load(reader);
                    conn.Close();
                }
                catch (MySqlException e)
                {
                    OnMySqlError(new MySqlErrorEventArgs(e));
                    conn.Close();
                }
            }
            //if the SQL connection succeeds, this method returns values from remote database storing them into a DataTable type variable called "DbImportato"
            return DbImportato;
        }
        //Execute read query
        public override async Task<DataTable> GetData(string query)

        {
            DataTable DbImportato = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                MySqlCommand cmd = new MySqlCommand(query, conn);
                try
                {
                    conn.Open();
                    DbDataReader reader;
                    reader = await cmd.ExecuteReaderAsync();

                    DbImportato.Load(reader);
                    conn.Close();
                }
                catch (MySqlException e)
                {
                    OnMySqlError(new MySqlErrorEventArgs(e));
                    conn.Close();
                }
            }
            //if the SQL connection succeeds, this method returns values from remote database storing them into a DataTable type variable called "DbImportato"
            return DbImportato;
        }

        //Test parameters for connection
        public override Exception Test()
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();
                    conn.Close();
                    return null;
                }
                catch (Exception e)
                {
                    // throw;
                    return e;
                }
            }
        }

        public override async void InitTables()
        {
            try
            {
                DataTable lastupdates = await GetData(String.Format("SELECT UPDATE_TIME, TABLE_NAME FROM information_schema.tables WHERE TABLE_SCHEMA = '{0}'; ", DatabaseName));
                foreach (DataRow row in lastupdates.Rows)
                {
                    lastDateUpdates.Add(new DateTime());
                }
            }
            catch (MySqlException e)
            {
                OnMySqlError(new MySqlErrorEventArgs(e));
            }
        }

        public override async Task<LastModifies> CheckUpdates()
        {
            LastModifies LastModifies = new LastModifies();

            DataTable dt = await GetData(String.Format("SELECT UPDATE_TIME, TABLE_NAME FROM information_schema.tables WHERE TABLE_SCHEMA = '{0}'; ", DatabaseName));
            foreach (DataRow row in dt.Rows)
                if (row[0].GetType() != typeof(DBNull))
                    LastModifies.List.Add(row[1].ToString(), Convert.ToDateTime(row[0]));
                else
                    LastModifies.List.Add(row[1].ToString(), new DateTime(0));

            return LastModifies;
        }

        //Check for updates on db
        public override async Task<List<string>> CheckUpdate()
        {
            int i = 0;
            List<string> AlteredTables = new List<string>();
            try
            {
                DataTable lastupdates = await GetData(String.Format("SELECT UNIX_TIMESTAMP(UPDATE_TIME), TABLE_NAME FROM information_schema.tables WHERE TABLE_SCHEMA = '{0}'; ", DatabaseName));
                foreach (DataRow row in lastupdates.Rows)
                {
                    DateTime data;
                    if (row[0].GetType() != typeof(DBNull))
                        data = UnixToDate(Convert.ToInt64(row[0]));
                    else
                        data = new DateTime(0);

                    if (data.CompareTo(lastDateUpdates[i]) >= 0)
                    {
                        lastDateUpdates[i] = DateTime.Now;
                        AlteredTables.Add(row[1].ToString());
                    }
                    i++;
                }
                if (AlteredTables.Count > 0)
                    return AlteredTables;
                else return new List<string>();
            }
            catch (MySqlException e)
            {
                OnMySqlError(new MySqlErrorEventArgs(e));
                return new List<string>();
            }
        }

        public override List<string> GetTables()
        {
            return new List<string>();
        }
        #endregion

        #region Clienti
        public ObservableCollection<Cliente> ConvertDataTableToClienti(DataTable dataTable)
        {
            ObservableCollection<Cliente> clienti = new ObservableCollection<Cliente>();

            if (dataTable.Rows.Count > 0)
                foreach (DataRow row in dataTable.Rows)
                    clienti.Add(new Cliente
                    {
                        ID = Convert.ToUInt32(row["id"]),
                        Created = Convert.ToDateTime(row["created"]),
                        LastModify = Convert.ToDateTime(row["lastmodify"]),
                        Nome = row["nome"].ToString(),
                    });

            return clienti;
        }
        public override async Task<ObservableCollection<Cliente>> GetClienti()
        {
            return ConvertDataTableToClienti(await GetData("SELECT * FROM clienti;"));
        }
        public override async Task<Cliente> CreateCliente()
        {
            SetData("INSERT INTO clienti (nome) VALUES ('Nuovo cliente');");
            return ConvertDataTableToClienti(await GetData("SELECT * FROM clienti ORDER BY ID DESC LIMIT 1;"))[0];
        }
        public override async Task<Cliente> DeleteCliente(Cliente c)
        {
            SetData(String.Format("DELETE FROM clienti WHERE id={0};", c.ID));
            return c;
        }


        #endregion
    }
}
