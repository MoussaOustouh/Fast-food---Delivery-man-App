using D_Fast_food.ViewModels;
using D_Fast_food.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace D_Fast_food
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(MapPage), typeof(MapPage));
        }

    }
}
