using CanisDAL;
using CanisModels;
using System;

namespace CanisBLL
{
    abstract public class BLBase
    {
        public SyncAgent SA { get; set; }
        public event EventHandler DataUpdated;

        public string Table {get; set;}

        public BLBase(SyncAgent sa)
        {
            SA = sa;
            SA.RemoteDB.DbUpdated += Db_DbUpdated;
        }
        //private void Db_DbUpdated(object sender, DbUpdatedEventArgs e)
        //{
        //    if (e.AlteredTables.Contains(Table))
        //        OnDataUpdated();
        //}
        internal abstract void Db_DbUpdated(object sender, DbUpdatedEventArgs e);
        private void OnDataUpdated()
        {
            DataUpdated?.Invoke(this, new EventArgs());
        }
    }
}
