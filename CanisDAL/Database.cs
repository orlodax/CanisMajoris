using CanisDAL.Utilities;
using CanisModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CanisDAL
{
    #region Eventargs
    public class MySqlErrorEventArgs : EventArgs
    {
        public MySqlErrorEventArgs(Exception _exception)
        {
            Exception = _exception;
        }

        public Exception Exception { get; }
    }
    public class SQLiteErrorEventArgs : EventArgs
    {
        public SQLiteErrorEventArgs(Exception _exception)
        {
            Exception = _exception;
        }

        public Exception Exception { get; }
    }
    public class DbUpdatedEventArgs : EventArgs
    {
        private List<string> alteredTables;

        public DbUpdatedEventArgs(List<string> _alteredTables)
        {
            alteredTables = _alteredTables;
        }

        public List<string> AlteredTables { get { return alteredTables; } }
    }
    #endregion

    public abstract class Database
    {
        #region Connection String Data
        public string ConnectionString { get; set; }
        public string Server { get; set; }
        public string Port { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DbType { get; set; }
        public string ApiKey { get; set; }
        #endregion

        public List<string> Tables { get; set; }

        #region Query Functions
        public abstract void SetData(string query);
        public abstract Task<DataTable> GetData(string query);
        #endregion

        #region Utility
        public static long DateToUnix(DateTime data)
        {
            long dataUnix = 0;
            try
            {
                DateTimeOffset dto = new DateTimeOffset(data);

                dataUnix = dto.ToUnixTimeSeconds();
            }
            catch (System.ArgumentOutOfRangeException) { }

            return dataUnix;
        }

        public static DateTime UnixToDate(long dataUnix)
        {
            if (dataUnix != 0)
            {
                DateTime data = DateTimeOffset.FromUnixTimeSeconds(dataUnix).DateTime.ToLocalTime();
                return data;
            }
            else return new DateTime(0);
        }

        public static string StringFix(string str)
        {
            if (str != null)
                return Regex.Replace(str, @"[\x00'""\b\n\r\t\cZ\\%_]",
                    delegate (Match match)
                    {
                        string v = match.Value;
                        switch (v)
                        {
                            case "\x00":            // ASCII NUL (0x00) character
                            return "\\0";
                            case "\b":              // BACKSPACE character
                            return "\\b";
                            case "\n":              // NEWLINE (linefeed) character
                            return "\\n";
                            case "\r":              // CARRIAGE RETURN character
                            return "\\r";
                            case "\t":              // TAB
                            return "\\t";
                            case "\u001A":          // Ctrl-Z
                            return "\\Z";
                            default:
                                return "\\" + v;
                        }
                    });
            return null;
        }
        #endregion

        #region Check System Functions
        public abstract Exception Test();
        public event EventHandler<MySqlErrorEventArgs> MySqlError;
        public event EventHandler<SQLiteErrorEventArgs> SQLiteError;
        public event EventHandler<DbUpdatedEventArgs> DbUpdated;
        private System.Timers.Timer TimerDatabase;//Timer che controlla la presenza di nuovi dati nel db
        private BackgroundWorker workerDatabase; //Worker che aggiorna la copia del db
        public abstract void InitTables();

        public void InitCheckSystem()
        {
            //Inizializzazione oggetti
            workerDatabase = new BackgroundWorker();
            TimerDatabase = new System.Timers.Timer(10000);
            InitTables();
            //Sottoscrizione eventi
            workerDatabase.DoWork += WorkerDatabase_DoWork;
            TimerDatabase.Elapsed += DbTimer_Tick;
        }

        public void StartCheckSystem()
        {
            TimerDatabase.Start();
        }

        public void StopCheckSystem()
        {
            if (TimerDatabase != null)
                TimerDatabase.Stop();
        }

        public virtual void OnMySqlError(MySqlErrorEventArgs e)
        {
            MySqlError?.Invoke(this, e);
        }
        public virtual void OnSQLiteError(SQLiteErrorEventArgs e)
        {
            SQLiteError?.Invoke(this, e);
        }
        public virtual void OnDbUpdated(DbUpdatedEventArgs e)
        {
            DbUpdated?.Invoke(this, e);
        }
        #endregion

        #region Funzioni eventi oggetti
        //----------------------------------Funzioni eventi oggetti----------------------------------
        //Funzione che controlla la presenza di nuovi dati sul db

        public abstract Task<List<string>> CheckUpdate();
        public abstract List<string> GetTables();
        private object timerLock = new object();//Oggetto per la non esecuzione continua del timer
        //---------------TO construct table/last update association----------------------------------
        public class LastModifies
        {
            public Dictionary<string, DateTime> List { get; set; } = new Dictionary<string, DateTime>();
        }
        public abstract Task<LastModifies> CheckUpdates();

        private async void WorkerDatabase_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> tables = await CheckUpdate();
            if (tables.Count > 0)
                OnDbUpdated(new DbUpdatedEventArgs(tables));
        }

        private void DbTimer_Tick(object sender, EventArgs e)
        {
            //Metodo per far si che la funzione non venga richiamata se è già in esecuzione
            int tid = Thread.CurrentThread.ManagedThreadId;
            bool lockTaken = false;
            try
            {
                lockTaken = Monitor.TryEnter(timerLock);
                if (lockTaken)
                {
                    //-----------------code here----------------
                    if (!workerDatabase.IsBusy) workerDatabase.RunWorkerAsync();
                    //
                }
            }
            finally
            {
                if (lockTaken) Monitor.Exit(timerLock);
            }
        }
        #endregion

        #region Funzioni

        #region Clienti
        public abstract Task<ObservableCollection<Cliente>> GetClienti();
        public abstract Task<Cliente> CreateCliente();
        public abstract Task<Cliente> DeleteCliente(Cliente c);
        #endregion

        #endregion
    }
}
