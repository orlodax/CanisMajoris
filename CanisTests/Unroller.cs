using CanisDAL;
using CanisModels;
using GenericPopulator;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace CanisTests
{
    public class Unroller
    {
        Database DB;
        string ConnectionString = "YourConnectionString";

        public Unroller()
        {
            DB = new DBMySQL("YourDbURL", "Port", "DBName", "Username", "Password", "MySql", string.Empty);
            DB.InitCheckSystem();
            DB.StartCheckSystem();
        }

       
        private Dictionary<string, object> SerializeRow(IEnumerable<string> cols, DbDataReader reader)
        {
            var result = new Dictionary<string, object>();
            foreach (var col in cols)
                result.Add(col, reader[col]);
            return result;
        }

        public DateTime UnixToDate(long dataUnix)
        {
            if (dataUnix != 0)
            {
                DateTime data = DateTimeOffset.FromUnixTimeSeconds(dataUnix).DateTime.ToLocalTime();
                return data;
            }
            else return new DateTime(0);
        }

        [Fact]
        public async void GetClientiJSON()
        {
            Stopwatch stopwatch = new Stopwatch();

            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM clienti;", conn);
                try
                {
                    conn.Open();
                    DbDataReader reader;
                    reader = await cmd.ExecuteReaderAsync();

                    stopwatch.Start();
                    
                    var results = new List<Dictionary<string, object>>();
                    var cols = new List<string>();
                    for (var i = 0; i < reader.FieldCount; i++)
                        cols.Add(reader.GetName(i));

                    while (reader.Read())
                        results.Add(SerializeRow(cols, reader));

                    var json = JsonConvert.SerializeObject(results, Formatting.Indented);
                    var jObjects = JsonConvert.DeserializeObject<List<Cliente>>(json);

                    stopwatch.Stop();

                    var tempo = stopwatch.ElapsedMilliseconds;

                    conn.Close();
                }
                catch (MySqlException e)
                {
                    conn.Close();
                }
            }
        }

        [Fact]
        public async void GetClientiDataTable()
        {
            Stopwatch stopwatch = new Stopwatch();

            DataTable DbImportato = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM clienti;", conn);
                try
                {
                    conn.Open();
                    DbDataReader reader;
                    reader = await cmd.ExecuteReaderAsync();

                    stopwatch.Start();

                    DbImportato.Load(reader);

                    List<Cliente> clienti = new List<Cliente>();

                    if (DbImportato.Rows.Count > 0)
                        foreach (DataRow row in DbImportato.Rows)
                            clienti.Add(new Cliente
                            {
                                //Id = Convert.ToInt32(row["id"]),
                                //Idcantiere = Convert.ToInt32(row["idcantiere"]),
                                //Iddipendente = Convert.ToInt32(row["iddipendente"]),
                                //Data = UnixToDate(Convert.ToInt64(row["data"])),
                                //Hingresso = UnixToDate(Convert.ToInt64(row["hingresso"])),
                                //Huscita = UnixToDate(Convert.ToInt64(row["huscita"])),
                               // Stato = row["stato"].ToString()
                            });

                    stopwatch.Stop();

                    var tempo = stopwatch.ElapsedMilliseconds;


                    conn.Close();
                }
                catch (MySqlException e)
                {
                 //   OnMySqlError(new MySqlErrorEventArgs(e));
                    conn.Close();
                }
            }
          
        }

      
        [Fact]
        public async void GetClientiREFLECT()
        {
            Stopwatch stopwatch = new Stopwatch();

            DataTable DbImportato = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM clienti;", conn);
                try
                {
                    conn.Open();

                    var reader = await cmd.ExecuteReaderAsync();

                    stopwatch.Start();

                    var lista = new ListHelper<Cliente>().CreateList(reader);

                    stopwatch.Stop();

                    var tempo = stopwatch.ElapsedMilliseconds;

                    conn.Close();
                }
                catch (MySqlException e)
                {
                    //   OnMySqlError(new MySqlErrorEventArgs(e));
                    conn.Close();
                }
            }

        }

    }
}
