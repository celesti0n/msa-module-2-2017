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

            await postLocationAsync();

            await MakePredictionRequest(file);
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
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Prediction-Key", "a51ac8a57d4e4345ab0a48947a4a90ac");

            string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/4da1555c-14ca-4aaf-af01-d6e1e97e5fa6/image?iterationId=7bc76035-3825-4643-917e-98f9d9f79b71";

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
                    int conclusion = 0; //0 = looks like neither, 1 = dog, 2 = cat,
                    //APPEND values to labels in XAML
                    for (int item = 0; item < Tag.Count(); item++) {
                        Loading.Text = "\n";
                        TagLabel.Text += "It looks " + Math.Round(Convert.ToDouble(Probability.ElementAt(item)) * 100, 2) + "% like a " + Tag.ElementAt(item) + ". \n";
						// if any exceed 0.5, set flag
                        if (Math.Round(Convert.ToDouble(Probability.ElementAt(item)) * 100, 2) > 50.00)
						{
                            if (Tag.ElementAt(item) == "Dog")
							{
								conclusion = 1;
								break;
							}
                            else if (Tag.ElementAt(item) == "Cat")
							{
								conclusion = 2;
								break;
							}
							else
							{
								conclusion = 0;
								break;
							}
						}
                    }
                    if (conclusion == 1)
                    {
                        ConclusionLabel.Text += "Dog.";
                    }
                    else if (conclusion == 2)
                    {
                        ConclusionLabel.Text += "Cat.";
                    }
                    else
                    {
                        ConclusionLabel.Text += "This isn't a dog or a cat.";
                    }

                    //Get rid of file once we have finished using it
                    file.Dispose();
                }
            }
        }
    }
}