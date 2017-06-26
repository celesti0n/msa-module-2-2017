using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Tabs
{
    public partial class AzureTable : ContentPage
    {
		Geocoder geoCoder;
       
        public AzureTable()
        {
            InitializeComponent();
            geoCoder = new Geocoder();

		}

		async void Handle_ClickedAsync(object sender, System.EventArgs e)
		{
            loading.IsRunning = true;
			List<DogOrCatModel> DogOrCatInformation = await AzureManager.AzureManagerInstance.GetDogOrCatInformation();

			foreach (DogOrCatModel model in DogOrCatInformation)
			{
				var position = new Position(model.Latitude, model.Longitude);
				var possibleAddresses = await geoCoder.GetAddressesForPositionAsync(position);
				foreach (var address in possibleAddresses)
                    model.City = address;
			}

			DogOrCatList.ItemsSource = DogOrCatInformation;
            loading.IsRunning = false;
		}

    }
}
