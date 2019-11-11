using Canis.Utility;
using CanisModels;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace Canis.ViewModels.Templates
{
    public class ListFrameVM : ObservableObject
    {
        #region Pane Section
        private int paneLenght;
        public int PaneLenght { get => paneLenght; set => SetField(ref paneLenght, value); }

        private bool isPaneOpen = false;
        public bool IsPaneOpen { get => isPaneOpen; set => SetField(ref isPaneOpen, value); }

        private Page paneContent = new Page();
        public Page PaneContent { get => paneContent; set => SetField(ref paneContent, value); }
        #endregion

        //list that will hold columns of the datagrid, specifieds inside derived Lists VMs
        public ObservableCollection<DataGridColumn> ColumnCollection { get; private set; } = new ObservableCollection<DataGridColumn>();

        public ICommand CreateCMD { get; internal set; }
        public ICommand DeleteCMD { get; internal set; }
        public ICommand EditCMD { get; internal set; }
        public ICommand ClosePaneCMD { get; internal set; }

        //common confirmation dialog
        public ContentDialog DeleteConfirmDialog;

        //generic object to hold selections from datagrid. It will be casted as matching model inside derived Lists VMs 
        private object selectedItem;
        public object SelectedItem { get => selectedItem; set => SetField(ref selectedItem, value); }


        public ListFrameVM()
        {
            EditCMD = new RelayCommand(Edit);
            ClosePaneCMD = new RelayCommand(ClosePane);
            DeleteCMD = new RelayCommand(Delete);

            DeleteConfirmDialog = new ContentDialog() { Title = "Attenzione", Content = "Eliminare l'elemento selezionato?", PrimaryButtonText = "Conferma", SecondaryButtonText = "Annulla" }; //popino eliminazione
        }
        private async void Delete(object parameter)
        {
            if (!DeleteConfirmDialog.IsLoaded)
                await DeleteConfirmDialog.ShowAsync();
        }
        private void Edit(object parameter)
        {
            if (SelectedItem != null)
                IsPaneOpen = true;
        }
        private void ClosePane(object parameter)
        {
            IsPaneOpen = false;
        }
    }
}
