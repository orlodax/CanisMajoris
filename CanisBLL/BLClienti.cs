using CanisBLL.Utilities;
using CanisDAL;
using CanisModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CanisBLL
{
    public class BLClienti : BLBase
    {
        public BLClienti(SyncAgent sa) : base(sa)
        {
            SA = sa;
            Table = "clienti";
         
        }
        internal override void Db_DbUpdated(object sender, DbUpdatedEventArgs e)
        {
           
        }

        public async Task<ObservableCollection<Cliente>> GetClienti() //Always read from local
        {
            return await SA.ActiveDB.GetClienti();
        }
        public async Task<Cliente> CreateCliente()
        {
            return await SA.ActiveDB.CreateCliente();
        }
        public async Task<Cliente> DeleteCliente(Cliente c)
        {
            return await SA.ActiveDB.DeleteCliente(c);
        }
    }
}
