using D_Fast_food.Models.MyModels;
using D_Fast_food.MyHelpers;
using D_Fast_food.Views;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;

namespace D_Fast_food
{
    public partial class App : Xamarin.Forms.Application
    {
        private MyHttpClient myHttpClient = new MyHttpClient();
        Command com { get; set; }

        public static Delivery_man deliveryMan = new Delivery_man();

        public App()
        {
			Xamarin.Forms.Application.Current.On<Xamarin.Forms.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
            InitializeComponent();

            com = new Command(async () => await CheckAuthorizationToDeliver());
        }



        protected override async void OnStart()
        {
            NavigationPage np;

            string loggedIn = await MyHelper.GetSavedValueForAppAsync("LoggedIn");

            if (loggedIn != null && loggedIn.Equals("true"))
            {
                try
                {
                    string user_info = await MyHelper.GetSavedValueForAppAsync("user_info");
                    deliveryMan = JsonConvert.DeserializeObject<Delivery_man>(user_info);

                    com.Execute(CheckAuthorizationToDeliver());

                    MainPage = new AppShell();
                }
                catch (Exception e)
                {
                    MyHelper.RemoveAllSavedValuesForApp();
                    np = new NavigationPage(new SignInPage());
                    MainPage = np;
                }
            }
            else
            {
                np = new NavigationPage(new SignInPage());
                MainPage = np;
            }


        }


        private async Task CheckAuthorizationToDeliver()
        {
            Delivery_man d1 = (Delivery_man)await myHttpClient.sendHttpGetAsyncObject<Delivery_man>(Constants.ApiGetDeliveryManById + "?id_delivery_man=" + App.deliveryMan.id_delivery_man);

            if (!d1.authorized)
            {
                MyHelper.RemoveAllSavedValuesForApp();

                NavigationPage np = new NavigationPage(new SignInPage());
                MainPage = np;
            }
        }



        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
