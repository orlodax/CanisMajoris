using CanisModels;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace Canis.ViewModels.Templates
{
    public class NavigationMenuVM : ObservableObject
    {
        private ObservableCollection<MenuItem> menuItems = new ObservableCollection<MenuItem>();
        public ObservableCollection<MenuItem> MenuItems { get => menuItems; set { menuItems = value; NotifyPropertyChanged(); } }

        private Page selectedPage;
        public Page SelectedPage { get => selectedPage; set => SetField(ref selectedPage, value); }
        public ICommand ItemInvokedCommand { get; internal set; }


        public NavigationMenuVM()
        {

        }

    }
}
