using D_Fast_food.Models.MyModels;
using D_Fast_food.MyHelpers;
using D_Fast_food.Views;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace D_Fast_food.ViewModels
{
    public class ProfilViewModel : BaseViewModel
    {
        private MyHttpClient myHttpClient = new MyHttpClient();

        private string _firstname;
        public string firstname { get => _firstname; set => SetProperty(ref _firstname, value); }

        private string _lastname;
        public string lastname { get => _lastname; set => SetProperty(ref _lastname, value); }

        private string _gender;
        public string gender { get => _gender; set => SetProperty(ref _gender, value); }

        private string _email;
        public string email { get => _email; set => SetProperty(ref _email, value); }

        private string _password;
        public string password { get => _password; set => SetProperty(ref _password, value); }

        private string _phone;
        public string phone { get => _phone; set => SetProperty(ref _phone, value); }

        private string _photo;
        public string photo { get => _photo; set => SetProperty(ref _photo, value); }


        private bool _IsMale;
        public bool IsMale { get => _IsMale; set => SetProperty(ref _IsMale, value); }

        private bool _IsFemale;
        public bool IsFemale { get => _IsFemale; set => SetProperty(ref _IsFemale, value); }

        private string _confirmation_code;
        public string confirmation_code { get => _confirmation_code; set => SetProperty(ref _confirmation_code, value); }


        private string _password1;
        public string password1 { get => _password1; set => SetProperty(ref _password1, value); }

        private string _password2;
        public string password2 { get => _password2; set => SetProperty(ref _password2, value); }


        private bool _IsCar;
        public bool IsCar { get => _IsCar; set => SetProperty(ref _IsCar, value); }

        private bool _IsMoto;
        public bool IsMoto { get => _IsMoto; set => SetProperty(ref _IsMoto, value); }

        private string _transport;
        public string transport { get => _transport; set => SetProperty(ref _transport, value); }

        private string _matricule;
        public string matricule { get => _matricule; set => SetProperty(ref _matricule, value); }

        




        private Delivery_man _deliveryMan;
        public Delivery_man deliveryMan { get => _deliveryMan; set => SetProperty(ref _deliveryMan, value); }

        public string profilesPicturesFolder { get => Constants.delivery_menPicturesFolder; }

        public Command LogoutCommand { get; }
        public Command ChangeInfoCommand { get; }
        public Command ChangePasswordCommand { get; }
        public Command ChangeTransportCommand { get; }

        public ProfilViewModel()
        {

            OnAppearing();
            Title = string.Format("{0} {1}", deliveryMan.firstname, deliveryMan.lastname);

            LogoutCommand = new Command(Logout);
            ChangeInfoCommand = new Command(async ()=> OnChangeInfo());
            ChangePasswordCommand = new Command(async () => OnChangePassword());
            ChangeTransportCommand = new Command(async () => OnChangeTransport());

        }

        protected void OnAppearing()
        {
            deliveryMan = App.deliveryMan;
            firstname = deliveryMan.firstname;
            lastname = deliveryMan.lastname;

            if (deliveryMan.gender == "Male")
            {
                IsMale = true; IsFemale = false;
            }
            else
            {
                IsMale = false; IsFemale = true;
            }

            email = deliveryMan.email;
            phone = deliveryMan.phone;
            password = "";
            photo = deliveryMan.photo;

            matricule = deliveryMan.matricule;
            if (deliveryMan.transport == "car.png")
            {
                IsCar = true; IsMoto = false;
            }
            else if(deliveryMan.transport == "moto.png")
            {
                IsCar = false; IsMoto = true;
            }
            else
            {
                deliveryMan.transport = "moto.png";
                transport = "moto.png";
                IsCar = false; IsMoto = true;
            }

            password1 = "";
            password2 = "";

        }


        private void Logout()
        {
            MyHelper.RemoveAllSavedValuesForApp();

            NavigationPage np = new NavigationPage(new SignInPage());
            App.Current.MainPage = np;
        }

        private async Task OnChangeInfo()
        {
            Delivery_man user = deliveryMan;
            user.firstname = firstname;
            user.lastname = lastname;
            if (IsMale)
            {
                user.gender = "Male";
            }
            else if (IsFemale)
            {
                user.gender = "Female";
            }
            user.email = email;
            user.phone = phone;
            user.password = password;


            if (user.firstname.Length == 0 || user.lastname.Length == 0 || user.email.Length == 0 || user.password.Length == 0 || user.phone.Length == 0)
            {
                await App.Current.MainPage.DisplayAlert("Empty field", "You must fill all the fields.", "OK");
            }
            else if (!MyHelper.IsValidEmail(user.email))
            {
                await App.Current.MainPage.DisplayAlert("Invalid email", "Enter a valid email : example@domain.com", "OK");
            }
            else
            {
                try
                {
                    JObject jRes = await myHttpClient.sendHttpPutAsyncJson(Constants.ApiPutChangeDeliveryManInfo, user);

                    if ((bool)jRes["HttpClient error"])
                    {
                        await App.Current.MainPage.DisplayAlert("Connection failed", "Connection failed.", "OK");
                    }
                    else if ((bool)jRes["HttpClient parsing error"])
                    {

                    }
                    else
                    {
                        if ((bool)jRes["Error"])
                        {
                            await App.Current.MainPage.DisplayAlert((string)jRes["TitleMessage"], (string)jRes["Message"], "OK");
                        }
                        else
                        {
                            await MyHelper.SaveValueForAppAsync("LoggedIn", "true");
                            await MyHelper.SaveValueForAppAsync("JWT", (string)jRes["JWT"]);
                            await MyHelper.SaveValueForAppAsync("id_user", (string)jRes["id_delivery_man"]);

                            JObject user_info = (JObject)jRes["user_info"];
                            await MyHelper.SaveValueForAppAsync("user_info", user_info.ToString());
                            user = user_info.ToObject<Delivery_man>();

                            App.deliveryMan = user;
                            deliveryMan = user;

                            OnAppearing();

                            await App.Current.MainPage.DisplayAlert("Notification", "The information has been changed.", "OK");
                            password = "";

                            OnAppearing();
                        }
                    }
                }
                catch (Exception ex)
                {
                    await App.Current.MainPage.DisplayAlert("Connection failed", "Connection failed.", "OK");
                }
            }

        }

        private async Task OnChangePassword()
        {
            if (password1.Length == 0 || password2.Length == 0)
            {
                await App.Current.MainPage.DisplayAlert("Empty field", "You must fill all the fields.", "OK");
            }
            else
            {
                deliveryMan.password = password1;
                deliveryMan.confirmation_code = password2;

                JObject jRes = await myHttpClient.sendHttpPutAsyncJson(Constants.ApiPutChangeDeliveryManPassword, deliveryMan);

                if ((bool)jRes["Error"])
                {
                    await App.Current.MainPage.DisplayAlert((string)jRes["TitleMessage"], (string)jRes["Message"], "OK");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Notification", "The password has been changed.", "OK");
                    password1 = "";
                    password2 = "";
                }

            }
        }



        private async Task OnChangeTransport()
        {
            if ((IsCar == false && IsMoto == false) || matricule == "") 
            {
                await App.Current.MainPage.DisplayAlert("Notification", "Add transportation information to make it easier for the client to recognize you.", "OK");
            }
            else
            {
                if (IsCar) { transport = "car.png"; }
                else if (IsMoto) { transport = "moto.png"; }

                deliveryMan.transport = transport;
                deliveryMan.matricule = matricule;

                JObject jRes = await myHttpClient.sendHttpPutAsyncJson(Constants.ApiPutChangeDeliveryManTransport, deliveryMan);

                if ((bool)jRes["Error"])
                {
                    await App.Current.MainPage.DisplayAlert((string)jRes["TitleMessage"], (string)jRes["Message"], "OK");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Notification", "The transportation information has been changed.", "OK");
                }

                OnAppearing();
            }
        }



    }
}
