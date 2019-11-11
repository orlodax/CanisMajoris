using CanisDAL.Utilities;
using CanisModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace CanisDAL
{
    public class SyncAgent
    {
        private bool isOnline;
        public bool IsOnline { get => isOnline; set { isOnline = value; if (value) ActiveDB = RemoteDB; else ActiveDB = LocalDB; } }
        private DateTime? LastTimeOnline { get; set; } = null;
        private DateTime? TimeWentOffline { get; set; } = null;
        public Database RemoteDB { get; set; }
        public Database LocalDB { get; set; }
        public Database ActiveDB { get; set; }

        Database.LastModifies LocalModifies = new Database.LastModifies();
        Database.LastModifies RemoteModifies = new Database.LastModifies();

        #region Constructor and Connectivity Watcher

        public event EventHandler<bool> InternetStatusChanged;

        public SyncAgent(Database remoteDatabase, Database localDatabase)
        {
            RemoteDB = remoteDatabase;
            LocalDB = localDatabase;

            IsOnline = GetIsInternetAccessAvailable();

            // register for network status change notifications
            NetworkStatusChangedEventHandler NetworkStatusCallback = new NetworkStatusChangedEventHandler(OnNetworkStatusChange);
            //WHATTT-----------------------------------------------SYSTEM RUNTIME INTEROPSERVICES BE DOOMED----------------------------
            NetworkInformation.NetworkStatusChanged += NetworkStatusCallback;

            CheckUpdatesOnStartUp();
        }
        
        //event to communicate to mainwindow when connection status changes
        void OnInternetStatusChanged(bool isonline)
        {
            InternetStatusChanged?.Invoke(this, isonline);
        }
        //actually checks connection status and rings this last event here
        public bool GetIsInternetAccessAvailable()
        {
            switch (NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel())
            {
                case NetworkConnectivityLevel.InternetAccess:
                    OnInternetStatusChanged(true);
                    return true;
                default:
                    OnInternetStatusChanged(false);
                    return false;
            }
        }
        #endregion

        //registered in constructor, this event writes connection on-off status in bool property
        void OnNetworkStatusChange(object sender)
        {
            IsOnline = GetIsInternetAccessAvailable();

            if (IsOnline)
            {
                //check local changes
                CheckLocalChanges();
                //store last online access
                LastTimeOnline = DateTime.Now;
            }
            else
            {
                TimeWentOffline = DateTime.Now;
            }
        }

        async void CheckUpdatesOnStartUp()
        {
            LocalModifies = await LocalDB.CheckUpdates();
            RemoteModifies = await RemoteDB.CheckUpdates();

            foreach (string table in LocalDB.Tables)
                if (LocalModifies.List.ContainsKey(table))
                    if (LocalModifies.List[table] < RemoteModifies.List[table]) //SE IL LOCALE È RIMASTO INDIETRO
                    {
                        #region nuovi inserts
                        DataTable dt = await RemoteDB.GetData(String.Format("SELECT * FROM {0} WHERE created > '{1}' AND created = lastmodify ;", table, LocalModifies.List[table]));
                        if (dt.Rows.Count > 0)
                        {
                            DataColumnCollection dataColumns = dt.Columns;
                            foreach (DataRow row in dt.Rows)
                            {
                                uint idr = Convert.ToUInt32(row[0]);
                                LocalDB.SetData(String.Format("INSERT INTO {0} (idr) VALUES ({1});", table, idr));

                                foreach (DataColumn col in dataColumns)
                                    if (col.ColumnName == "id" || col.ColumnName == "rowStart" || col.ColumnName == "rowEnd")
                                        break;
                                    else
                                        LocalDB.SetData(String.Format("UPDATE {0} SET {1}='{2}' WHERE idr={3};", table, col.ColumnName, row[col.ColumnName], idr));
                            }
                        }
                        #endregion
                        #region nuovi updates
                            DataTable dtu = await RemoteDB.GetData(String.Format("SELECT * FROM {0} WHERE lastmodify > '{1}' AND created < lastmodify ;", table, LocalModifies.List[table]));
                            if (dtu.Rows.Count > 0)
                            {
                                DataColumnCollection dataColumns = dtu.Columns;
                                foreach (DataRow row in dtu.Rows)
                                {
                                    uint idr = Convert.ToUInt32(row[0]);

                                    foreach (DataColumn col in dataColumns)
                                        if (col.ColumnName == "id" || col.ColumnName == "rowStart" || col.ColumnName == "rowEnd")
                                            break;
                                        else
                                            LocalDB.SetData(String.Format("UPDATE {0} SET {1}='{2}' WHERE idr={3};", table, col.ColumnName, row[col.ColumnName], idr));
                                }
                            }
                        #endregion
                    }
        }

        void CheckLocalChanges()
        { 
            if (LastTimeOnline != null && TimeWentOffline!=null) //if something happened that made LastTimeOnline not null, we have to check for local updates to upload
                foreach (string table in LocalDB.Tables)
                    UpdateRemoteTable(table);
        }

/// <summary>
/// DA CORREGGERE...............................
/// </summary>
/// <param name="table"></param>
        async void UpdateRemoteTable(string table) //sends data to remote table if finds updates
        {
            DataTable newInserts = await LocalDB.GetData(String.Format("SELECT * FROM {0} WHERE created > {1} AND lastmodify = NULL AND idr = 0;", table, TimeWentOffline.Value.ToString("yyyy-MM-dd HH:mm:ss")));
            DataTable newUpdates = await LocalDB.GetData(String.Format("SELECT * FROM {0} WHERE lastmodify > {1};", table, TimeWentOffline.Value.ToString("yyyy-MM-dd HH:mm:ss")));

            if (newInserts.Rows.Count > 0)
            {
                DataColumnCollection dataColumns = newInserts.Columns;
                dataColumns.Remove("id");
                dataColumns.Remove("idr");
                dataColumns.Remove("created");
                dataColumns.Remove("lastmodify");

                foreach (DataRow row in newInserts.Rows)
                {
                    RemoteDB.SetData(String.Format("INSERT INTO {0} (nome) VALUES ('uploading');", table));

                    DataTable dataTable = await RemoteDB.GetData(String.Format("SELECT id FROM {0} ORDER BY ID DESC LIMIT 1;", table));
                    uint idr = Convert.ToUInt32(dataTable.Rows[0][0]);

                    for (int j = 0; j <= dataColumns.Count; j++)
                    {
                        DataColumn col = dataColumns[j];
                        //Type type = row[col.ColumnName].GetType(); IS IT necessary to check TYPES?

                        //var cell = row[col.ColumnName];

                        //if (type == typeof(string))
                        //    cell = row[col.ColumnName].ToString();

                        RemoteDB.SetData(String.Format("UPDATE {0} SET {1}='{2}' WHERE id={3};", table, col.ColumnName, row[col.ColumnName], idr));
                    }   
                }
            }

            if (newUpdates.Rows.Count > 0)
            {
                DataColumnCollection dataColumns = newUpdates.Columns;
                dataColumns.Remove("id");
                //dataColumns.Remove("idr");
                dataColumns.Remove("created");
                dataColumns.Remove("lastmodify");

                foreach (DataRow row in newInserts.Rows)
                {
                    uint idr = Convert.ToUInt32(row["idr"]);

                    DataTable remoteEntry = await RemoteDB.GetData(String.Format("SELECT lastmodify FROM {0} WHERE id={1};", table, idr));
                    DateTime dateModify = Convert.ToDateTime(remoteEntry.Rows[0][0]);

                    if (dateModify < TimeWentOffline)
                        for (int j = 0; j <= dataColumns.Count; j++)
                        {
                            DataColumn col = dataColumns[j];
                            if (col.ColumnName != "idr")
                                RemoteDB.SetData(String.Format("UPDATE {0} SET {1}='{2}' WHERE id={3};", table, col.ColumnName, row[col.ColumnName], idr));
                        }
                    else
                        for (int j = 0; j <= dataColumns.Count; j++)
                        {
                            RemoteDB.SetData(String.Format("INSERT INTO {0} (nome) VALUES ('uploading');", table));
                            DataTable dataTable = await RemoteDB.GetData(String.Format("SELECT id FROM {0} ORDER BY ID DESC LIMIT 1;", table));
                            uint idremoto = Convert.ToUInt32(dataTable.Rows[0][0]);

                            DataColumn col = dataColumns[j];
                            if (col.ColumnName != "idr")
                                RemoteDB.SetData(String.Format("UPDATE {0} SET {1}='{2}' WHERE id={3};", table, col.ColumnName, row[col.ColumnName], idremoto));
                        }
                }
            }
        }
    }
}
