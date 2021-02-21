using D_Fast_food.Models.MyModels;
using D_Fast_food.MyHelpers;
using D_Fast_food.Views;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace D_Fast_food.ViewModels
{
    public class OrdersViewModel : BaseViewModel
    {

        private MyHttpClient myHttpClient = new MyHttpClient();

        private OrderFullInfo _selectedOrder;

        public List<string> pickerStatesList { get; }
        public int pickedIndex { get; set; }
        public string ordersState { get; set; }

        public bool newOrders { get; set; }
        public bool showOrdersDateTime { get; set; }
        public bool showDeliveryMen { get; set; }
        public bool ordersDelivred { get; set; }

        private OrderVisibility orderVisibility = new OrderVisibility();


        public ProductCategory productCategory;

        public List<OrderFullInfo> listOrders { get; set; }
        public ObservableCollection<OrderFullInfo> listOrdersBinding { get; set; }


        public Command LoadDataCommand { get; }
        public Command<OrderFullInfo> OrderTapped { get; }
        public Command AddOrderCommand { get; }

        public OrdersViewModel()
        {
            Title = "Orders";

            pickerStatesList = OrderStates.app_states_list_D;
            pickedIndex = 0;

            listOrders = new List<OrderFullInfo>();
            listOrdersBinding = new ObservableCollection<OrderFullInfo>();

            LoadDataCommand = new Command(async () => await ExecuteLoadDataCommand());
            OrderTapped = new Command<OrderFullInfo>(OnOrderSelected);



        }


        public async Task ExecuteLoadDataCommand()
        {
            IsBusy = true;
            try
            {
                string link = "";
                ordersState = OrderStates.server_states_list_D[pickedIndex];

                if (ordersState == OrderStates.order_to_deliver)
                {
                    orderVisibility.showClient = false;
                    orderVisibility.ordersDelivred = false;

                    link = Constants.ApiGetOrderToDeliver;
                }
                else if (ordersState == OrderStates.order_on_the_way)
                {
                    orderVisibility.showClient = true;
                    orderVisibility.ordersDelivred = false;

                    link = string.Format("{0}?id_delivery_man={1}", Constants.ApiGetOrderOnTheWayByDeliveryManId, App.deliveryMan.id_delivery_man);
                }
                else if (ordersState == OrderStates.order_has_been_delivered)
                {
                    orderVisibility.showClient = false;
                    orderVisibility.ordersDelivred = true;

                    link = string.Format("{0}?id_delivery_man={1}", Constants.ApiGetOrderDeliveredByDeliveryManId, App.deliveryMan.id_delivery_man);
                }



                listOrders = await myHttpClient.sendHttpGetAsyncList<OrderFullInfo>(link);
                listOrdersBinding.Clear();
                foreach (var p in listOrders)
                {
                    p.orderVisibility = orderVisibility;

                    p.delivery_man.photo = Constants.delivery_menPicturesFolder + p.delivery_man.photo;
                    p.delivery_man.transport = Constants.appPicturesFolder + p.delivery_man.transport;
                    p.client.photo = Constants.clientsPicturesFolder + p.client.photo;

                    listOrdersBinding.Add(p);
                }

            }
            catch (Exception e)
            {
                IsBusy = true;
            }
            finally
            {
                IsBusy = false;
            }
        }


        public void OnAppearing()
        {
            IsBusy = true;
            SelectedOrder = null;
        }



        public OrderFullInfo SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                SetProperty(ref _selectedOrder, value);
                OnOrderSelected(value);
            }
        }

        async void OnOrderSelected(OrderFullInfo order)
        {
            if (order == null)
            {
                return;
            }
            else
            {
                if (ordersState == OrderStates.order_to_deliver)
                {
                    var res= await App.Current.MainPage.DisplayAlert("Deliver order", "Will you deliver that order ?", "Yes", "No");

                    if (res)
                    {
                        OrderFullInfo o = new OrderFullInfo();
                        o.id_order = order.id_order;
                        o.id_delivery_man = App.deliveryMan.id_delivery_man;
                        JObject jRes = await myHttpClient.sendHttpPutAsyncJson(Constants.ApiPutDeliveryManWillDeliverAnOrder, o);


                        if ((bool)jRes["HttpClient parsing error"] || (bool)jRes["HttpClient error"])
                        {
                            await App.Current.MainPage.DisplayAlert("Failed", "Failed, try again.", "Ok");
                        }
                        else
                        {
                            await App.Current.MainPage.DisplayAlert((string)jRes["MessageTitele"], (string)jRes["Message"], "Ok");

                            if (!(bool)jRes["Error"])
                            {
                                await ExecuteLoadDataCommand();
                            }
                        }
                    }

                }
                else if (ordersState == OrderStates.order_on_the_way)
                {
                    string action = await App.Current.MainPage.DisplayActionSheet("", "Close", null, "Open the Map", "Cancel delivring");
                    if (action=="Open the Map")
                    {
                        await Shell.Current.GoToAsync($"{nameof(MapPage)}?{nameof(MapViewModel.OrderId)}={order.id_order}&{nameof(MapViewModel.ClientId)}={order.id_client}&{nameof(MapViewModel.PhoneNum)}={order.client.phone}");
                    }
                    else if (action == "Cancel delivring")
                    {
                        OrderFullInfo o = new OrderFullInfo();
                        o.id_order = order.id_order;
                        o.id_delivery_man = App.deliveryMan.id_delivery_man;
                        JObject jRes = await myHttpClient.sendHttpPutAsyncJson(Constants.ApiPutDeliveryManCancelDeliverinAnOrder, o);

                        if ((bool)jRes["HttpClient parsing error"] || (bool)jRes["HttpClient error"])
                        {
                            await App.Current.MainPage.DisplayAlert("Failed", "Failed, try again.", "Ok");
                        }
                        else
                        {
                            await App.Current.MainPage.DisplayAlert((string)jRes["MessageTitele"], (string)jRes["Message"], "Ok");

                            await ExecuteLoadDataCommand();
                        }
                    }
                    
                }
                else if (ordersState == OrderStates.order_has_been_delivered)
                {
                }
            }
        }



    }
}
