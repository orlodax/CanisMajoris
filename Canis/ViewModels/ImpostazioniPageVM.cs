using Canis.Utility;
using Canis.Views;
using CanisModels;
using System.Windows.Input;
using Unity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Canis.ViewModels
{
    public class ImpostazioniPageVM : ObservableObject
    {
        string indirizzoIp;
        string dbName;
        string dbPort;
        string username;
        string password;
        public string IndirizzoIp { get => indirizzoIp; set => SetField(ref indirizzoIp, value); }
        public string DbName { get => dbName; set => SetField(ref dbName, value); }
        public string DbPort { get => dbPort; set => SetField(ref dbPort, value); }
        public string Username { get => username; set => SetField(ref username, value); }
        public string Password { get => password; set => SetField(ref password, value); }

        Windows.Storage.ApplicationDataContainer localSettings;

        public ICommand SaveLoginCMD { get; internal set; }

        public ImpostazioniPageVM()
        {
            SaveLoginCMD = new RelayCommand(SaveLogin);

            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values["dbIPAddress"] != null)
                IndirizzoIp = localSettings.Values["dbIPAddress"].ToString();
            if (localSettings.Values["dbName"] != null)
                DbName = localSettings.Values["dbName"].ToString();
            if (localSettings.Values["dbPort"] != null)
                DbPort = localSettings.Values["dbPort"].ToString();
            if (localSettings.Values["dbUsername"] != null)
                Username = localSettings.Values["dbUsername"].ToString();
            if (localSettings.Values["dbPassword"] != null)
                Password = localSettings.Values["dbPassword"].ToString();
        }

        private void SaveLogin(object parameter)
        {
            localSettings.Values["dbIPAddress"] = IndirizzoIp;
            localSettings.Values["dbName"] = DbName;
            localSettings.Values["dbPort"] = DbPort;
            localSettings.Values["dbUsername"] = Username;
            localSettings.Values["dbPassword"] = Password;

            Frame rootFrame = Window.Current.Content as Frame;

            if (CompositionRoot.Execute())
                rootFrame.Navigate(typeof(MainPage));
            else
            { }
        }
    }
}
