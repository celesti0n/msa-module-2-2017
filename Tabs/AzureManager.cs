using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace Tabs
{
	public class AzureManager
	{

		private static AzureManager instance;
		private MobileServiceClient client;
		private IMobileServiceTable<DogOrCatModel> DogOrCatTable;

		private AzureManager()
		{
			this.client = new MobileServiceClient("http://dogorcat.azurewebsites.net/");
            this.DogOrCatTable = this.client.GetTable<DogOrCatModel>();
		}

		public MobileServiceClient AzureClient
		{
			get { return client; }
		}

		public static AzureManager AzureManagerInstance
		{
			get
			{
				if (instance == null)
				{
					instance = new AzureManager();
				}

				return instance;
			}
		}

		public async Task<List<DogOrCatModel>> GetDogOrCatInformation()
		{
			return await this.DogOrCatTable.ToListAsync();
		}

        public async Task PostDogOrCatInformation(DogOrCatModel DogOrCatModel)
		{
			await this.DogOrCatTable.InsertAsync(DogOrCatModel);
		}
	}
}
