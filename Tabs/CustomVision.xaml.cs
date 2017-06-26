using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Forms;
using Newtonsoft.Json.Linq;
using System.Linq;
using Plugin.Geolocator;
using Newtonsoft.Json;

namespace Tabs
{
    public partial class CustomVision : ContentPage
    {
        public CustomVision()
        {
            InitializeComponent();
        }

        private async void loadCamera(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", "Sorry, the camera could not be initialised.", "Okay");
                return;
            }

            MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                PhotoSize = PhotoSize.Medium,
                Directory = "Sample",
                Name = $"{DateTime.UtcNow}.jpg"
            });

            if (file == null)
                return;

            image.Source = ImageSource.FromStream(() =>
            {
                return file.GetStream();
            });

            await MakePredictionRequest(file); // do the request first before posting, it's much better UX
            await postLocationAsync();


        }

        async Task postLocationAsync()
        {

            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 50;

            var position = await locator.GetPositionAsync(10000);

            DogOrCatModel model = new DogOrCatModel()
            {
                Longitude = (float)position.Longitude,
                Latitude = (float)position.Latitude

            };

            await AzureManager.AzureManagerInstance.PostDogOrCatInformation(model);
        }

        static byte[] GetImageAsByteArray(MediaFile file)
        {
            var stream = file.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }

        async Task MakePredictionRequest(MediaFile file)
        {
            Loading.Text += "Please wait, your result is loading... \n";
            TagLabel.Text = "\n"; //reset on new query
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Prediction-Key", "9547337b978c4c2f92ecf9d5ebc3e24f");

            string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/86a79548-259e-467f-92b3-67577ce05fb8/image?iterationId=9c3eb031-32fe-4bf3-85b2-dcd8fb0c6ba2";

            HttpResponseMessage response;

            byte[] byteData = GetImageAsByteArray(file);

            using (var content = new ByteArrayContent(byteData))
            {

                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(url, content);


                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    JObject rss = JObject.Parse(responseString);

                    //Get all Prediction Values
                    var Probability = from p in rss["Predictions"] select (string)p["Probability"];
                    var Tag = from p in rss["Predictions"] select (string)p["Tag"];
                    //APPEND values to labels in XAML
                    for (int item = 0; item < Tag.Count(); item++) {
                        Loading.Text = "\n";
                        TagLabel.Text += "It looks " + Math.Round(Convert.ToDouble(Probability.ElementAt(item)) * 100, 2) + "% like a " + Tag.ElementAt(item) + ". \n";
                    }
                    //Get rid of file once we have finished using it
                    file.Dispose();
                }
            }
        }
    }
}