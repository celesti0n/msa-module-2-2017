using System;
using System.Collections.Generic;
using System.Linq;
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
			List<DogOrCatModel> DogOrCatInformation = await AzureManager.AzureManagerInstance.GetDogOrCatInformation(); //calls ToListAsync on table
			foreach (DogOrCatModel model in DogOrCatInformation) //loop to show table entries
			{
				var position = new Position(model.Latitude, model.Longitude);
				var possibleAddresses = await geoCoder.GetAddressesForPositionAsync(position);
                foreach (var address in possibleAddresses) //get city value from API call
                    model.City = address;
			}
            
            DogOrCatList.ItemsSource = DogOrCatInformation; //DogOrCatList declared in .xaml as ListView, ItemsSource is property to display
            loading.IsRunning = false;
		}

    }
}
