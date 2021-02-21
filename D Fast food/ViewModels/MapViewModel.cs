using D_Fast_food.Models.MyModels;
using D_Fast_food.MyHelpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace D_Fast_food.ViewModels
{
    [QueryProperty(nameof(PhoneNum), nameof(PhoneNum))]
    [QueryProperty(nameof(ClientId), nameof(ClientId))]
    [QueryProperty(nameof(OrderId), nameof(OrderId))]
    class MapViewModel : BaseViewModel
    {
        private string _phoneNum;
        public string PhoneNum
        {
            get
            {
                return _phoneNum;
            }
            set
            {
                _phoneNum = value;
            }
        }

        private string _clientId;
        public string ClientId
        {
            get
            {
                return _clientId;
            }
            set
            {
                _clientId = value;
            }
        }

        private string _orderId;
        public string OrderId
        {
            get
            {
                return _orderId;
            }
            set
            {
                _orderId = value;
            }
        }

        private string _orderCode;
        public string OrderCode
        {
            get
            {
                return _orderCode;
            }
            set
            {
                _orderId = value;
            }
        }

        private MyHttpClient myHttpClient = new MyHttpClient();
        public Command PhoneCallCommand { get; }
        public Command ConfirmeAsDelivredCommand { get; }


        public MapViewModel()
        {
            Title = "Map";

            PhoneCallCommand = new Command(OnPhoneCallTapped);
            ConfirmeAsDelivredCommand = new Command(OnConfirmeAsDelivred);


        }



        private async void OnPhoneCallTapped()
        {
            await Browser.OpenAsync("tel:"+_phoneNum);
        }


        public async void OnConfirmeAsDelivred()
        {
            string result = await App.Current.MainPage.DisplayPromptAsync("Order code", "You will get the order code from the clients when you give them the orders.", placeholder: "123456", maxLength: 6, keyboard: Keyboard.Numeric);

            if (result != null) 
            {
                OrderFullInfo ofi = new OrderFullInfo();
                ofi.id_order = Convert.ToInt32(OrderId);
                ofi.order_code = result.Trim();

                JObject jRes = await myHttpClient.sendHttpPutAsyncJson(Constants.ApiPutDelivringComplete, ofi);

                await App.Current.MainPage.DisplayAlert((string)jRes["MessageTitele"], (string)jRes["Message"], "Ok");

                if (!(bool)jRes["Error"])
                {
                    await Shell.Current.GoToAsync($"..");
                }
            }

        }


    }
}
