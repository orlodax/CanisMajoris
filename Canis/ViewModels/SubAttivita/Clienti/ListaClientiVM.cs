using Canis.Utility;
using Canis.ViewModels.Templates;
using Canis.Views.SubAttivita.Clienti.SubFrames;
using CanisBLL;
using CanisBLL.Utilities;
using CanisModels;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.ObjectModel;
using Unity;
using Windows.UI.Xaml;

namespace Canis.ViewModels.SubAttivita.Clienti
{
    public class ListaClientiVM : ListFrameVM
    {
        BLClienti BLClienti;

        private Cliente selectedCliente = new Cliente();
        public Cliente SelectedCliente { get { return (SelectedItem != null) ? SelectedItem as Cliente : new Cliente(); } set => SetField(ref selectedCliente, value); }

        private NotifyTaskCompletion<ObservableCollection<Cliente>> collection;
        public NotifyTaskCompletion<ObservableCollection<Cliente>> Collection { get => collection; set => SetField(ref collection, value); }


        public ListaClientiVM()
        {
            BLClienti = Globals.Container.Resolve<BLClienti>();

            CreateCMD = new RelayCommand(Create);

            DeleteConfirmDialog.PrimaryButtonCommand = new RelayCommand(DeleteCliente);

            ColumnCollection.Add(new DataGridTextColumn() { Header = "Nome", IsReadOnly = true, Binding = new Windows.UI.Xaml.Data.Binding() { Path = new PropertyPath("Nome") } });
            ColumnCollection.Add(new DataGridTextColumn() { Header = "Data Creazione", IsReadOnly = true, Binding = new Windows.UI.Xaml.Data.Binding() { Path = new PropertyPath("CreatoIl") } });
            ColumnCollection.Add(new DataGridTextColumn() { Header = "Ultima Modifica", IsReadOnly = true, Binding = new Windows.UI.Xaml.Data.Binding() { Path = new PropertyPath("UltimaModifica") } });

            Collection = new NotifyTaskCompletion<ObservableCollection<Cliente>>(BLClienti.GetClienti());

            PaneLenght = 500;
            PaneContent = new DettagliCliente() { DataContext = SelectedCliente };
        }

        private async void Create(object parameter)
        {
            Collection.Result.Add(await BLClienti.CreateCliente());
        }

        private async void DeleteCliente(object parameter)
        {
            if (SelectedCliente.ID > 0)
                Collection.Result.Remove(await BLClienti.DeleteCliente(SelectedCliente));
        }
    }
}
