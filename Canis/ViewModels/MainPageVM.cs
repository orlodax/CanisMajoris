using Canis.Utility;
using Canis.Views;
using Canis.Views.Templates;
using CanisDAL;
using CanisModels;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Windows.Input;
using Unity;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Canis.ViewModels
{
    public class MainPageVM : ObservableObject
    {
        public ICommand NavigationClickCMD { get; private set; }

        private Page pageToNavigate;
        public Page PageToNavigate { get => pageToNavigate; set => SetField(ref pageToNavigate, value); }

        private Button lastButtonPressed = new Button();
        public Button LastButtonPressed { get => lastButtonPressed; set => SetField(ref lastButtonPressed, value); }


        private bool isOnline;
        public bool IsOnline { get => isOnline; set => SetField(ref isOnline, value); }

        SolidColorBrush accento;
        SyncAgent SyncAgent;

        public MainPageVM()
        {
            NavigationClickCMD = new RelayCommand(NavigationClick);

            accento = (SolidColorBrush)Application.Current.Resources["SystemControlForegroundAccentBrush"];
            byte a = ((Color)accento.GetValue(SolidColorBrush.ColorProperty)).A;
            byte g = ((Color)accento.GetValue(SolidColorBrush.ColorProperty)).G;
            byte r = ((Color)accento.GetValue(SolidColorBrush.ColorProperty)).R;
            byte b = ((Color)accento.GetValue(SolidColorBrush.ColorProperty)).B;

            SyncAgent = Globals.Container.Resolve<SyncAgent>();
            SyncAgent.InternetStatusChanged += SyncAgent_InternetStatusChanged;
            IsOnline = SyncAgent.IsOnline;
        }

        private void SyncAgent_InternetStatusChanged(object sender, bool e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    // Dispatch back to the main thread
                    IsOnline = e;
                });
        }

        private void NavigationClick(object sender)
        {
            LastButtonPressed.Foreground = new SolidColorBrush(Colors.White);
            Button button = sender as Button;
            button.Foreground = accento;

            switch (button.Name)
            {
                case "ButtonHome":
                    PageToNavigate = new HomePage() { DataContext = new HomePageVM() };
                    break;
                case "ButtonAttivita":
                    PageToNavigate = new HorizontalMenu() { DataContext = new AttivitaPageVM() };
                    break;
                case "ButtonAgenti":
                    PageToNavigate = new HorizontalMenu() { DataContext = new AgentiPageVM() };
                    break;
                case "ButtonBizIntel":
                    PageToNavigate = new HorizontalMenu() { DataContext = new BizIntelPageVM() };
                    break;
                case "ButtonVendite":
                    PageToNavigate = new HorizontalMenu() { DataContext = new VenditePageVM() };
                    break;
                case "ButtonReport":
                    PageToNavigate = new HorizontalMenu() { DataContext = new ReportPageVM() };
                    break;
                case "ButtonRubrica":
                    PageToNavigate = new RubricaPage() { DataContext = new RubricaPageVM() };
                    break;
                case "ButtonImpostazioni":
                    PageToNavigate = new ImpostazioniPage() { DataContext = new ImpostazioniPageVM() };
                    break;
            }
            LastButtonPressed = button;
        }

    }
}
