using CanisModels;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace Canis.ViewModels.Templates
{
    public class MenuItem : ObservableObject
    {
        public string MenuEntry { get; set; }
        public string Tooltip { get; set; }
        public Symbol Glyph { get; set; }

        public MenuItem()
        {

        }
    }
}
