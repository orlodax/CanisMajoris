using Canis.Utility;
using Canis.ViewModels.SubAttivita;
using Canis.ViewModels.Templates;
using Canis.Views.Templates;
using Windows.UI.Xaml.Controls;

namespace Canis.ViewModels
{
    public class AttivitaPageVM : NavigationMenuVM
    {
        public AttivitaPageVM()
        {
            ItemInvokedCommand = new RelayCommand(NavigationTo);

            #region Menu Entries
            //repeat for each menu entry
            MenuItems.Add(new MenuItem()
            {
                MenuEntry = "Clienti",
                Glyph = (new SymbolIconSource() { Symbol = Symbol.Contact }).Symbol,
            }) ;
            //--------------------------
            //repeat for each menu entry
            MenuItems.Add(new MenuItem()
            {
                MenuEntry = "Fornitori",
                Glyph = (new SymbolIconSource() { Symbol = Symbol.Manage }).Symbol,
            });
            //--------------------------
            //repeat for each menu entry
            MenuItems.Add(new MenuItem()
            {
                MenuEntry = "Prodotti",
                Glyph = (new SymbolIconSource() { Symbol = Symbol.Document }).Symbol,
            });
            //--------------------------
            //repeat for each menu entry
            MenuItems.Add(new MenuItem()
            {
                MenuEntry = "Strutture",
                Glyph = (new SymbolIconSource() { Symbol = Symbol.Street }).Symbol,
            });
            //--------------------------
            //repeat for each menu entry
            MenuItems.Add(new MenuItem()
            {
                MenuEntry = "Competitor",
                Glyph = (new SymbolIconSource() { Symbol = Symbol.BlockContact }).Symbol,
            });
            //--------------------------
            //repeat for each menu entry
            MenuItems.Add(new MenuItem()
            {
                MenuEntry = "Conto Vendita",
                Glyph = (new SymbolIconSource() { Symbol = Symbol.TwoPage }).Symbol,
            });
            //--------------------------
            //repeat for each menu entry
            MenuItems.Add(new MenuItem()
            {
                MenuEntry = "Obiettivi",
                Glyph = (new SymbolIconSource() { Symbol = Symbol.WebCam }).Symbol,
            });
            //--------------------------
            #endregion


            void NavigationTo(object parameter)
            {
                NavigationViewItemInvokedEventArgs args = parameter as NavigationViewItemInvokedEventArgs;
                
                switch (args.InvokedItem.ToString())
                {
                    case "Clienti":
                        SelectedPage = new VerticalMenu() { DataContext = new MenuClientiVM() };
                        break;
                    case "Fornitori":
                        SelectedPage = new VerticalMenu() { DataContext = new MenuClientiVM() };
                        break;
                    case "Prodotti":
                        SelectedPage = new VerticalMenu() { DataContext = new MenuClientiVM() };
                        break;
                    case "Strutture":
                        SelectedPage = new VerticalMenu() { DataContext = new MenuClientiVM() };
                        break;
                    case "Competitor":
                        SelectedPage = new VerticalMenu() { DataContext = new MenuClientiVM() };
                        break;
                    case "Conto Vendita":
                        SelectedPage = new VerticalMenu() { DataContext = new MenuClientiVM() };
                        break;
                    case "Obiettivi":
                        SelectedPage = new VerticalMenu() { DataContext = new MenuClientiVM() };
                        break;
                }

            }


        }


    }
}
