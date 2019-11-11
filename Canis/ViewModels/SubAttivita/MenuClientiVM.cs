using Canis.Utility;
using Canis.ViewModels.SubAttivita.Clienti;
using Canis.ViewModels.Templates;
using Canis.Views.SubAttivita.Clienti;
using Canis.Views.Templates;
using Windows.UI.Xaml.Controls;

namespace Canis.ViewModels.SubAttivita
{
    public class MenuClientiVM : NavigationMenuVM
    {
        public MenuClientiVM()
        {
            ItemInvokedCommand = new RelayCommand(NavigationTo);

            MenuItems.Add(new MenuItem()
            {
                MenuEntry = "Lista Clienti",
                Glyph = (new SymbolIconSource() { Symbol = Symbol.ContactInfo }).Symbol,
            });
            MenuItems.Add(new MenuItem()
            {
                MenuEntry = "Geolocalizzazione",
                Glyph = (new SymbolIconSource() { Symbol = Symbol.World }).Symbol,
            });
            MenuItems.Add(new MenuItem()
            {
                MenuEntry = "Recupero Clienti",
                Glyph = (new SymbolIconSource() { Symbol = Symbol.ReportHacked }).Symbol,
            });

            void NavigationTo(object parameter)
            {
                NavigationViewItemInvokedEventArgs args = parameter as NavigationViewItemInvokedEventArgs;

                switch (args.InvokedItem.ToString())
                {
                    case "Lista Clienti":
                        SelectedPage = new ListFrame() { DataContext = new ListaClientiVM() };
                        break;
                    case "Geolocalizzazione":
                        SelectedPage = new GeolocalizzazioneClienti() { DataContext = new GeolocalizzazioneClientiVM() };
                        break;
                    case "Recupero Clienti":
                        SelectedPage = new RecuperoClienti() { DataContext = new RecuperoClientiVM() };
                        break;

                }
            }
        }
    }
}
