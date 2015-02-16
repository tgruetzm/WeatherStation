using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace WeatherStation
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string stationId;
        private string uri1 = "https://script.google.com/macros/s/AKfycbwV58H8Tyw-jvFoI1ClSBpkvvobi53R8FecJhCf0e5wObL4ZI0/exec?station={0}&page=temp";
        private string uri2 = "https://script.google.com/macros/s/AKfycbwV58H8Tyw-jvFoI1ClSBpkvvobi53R8FecJhCf0e5wObL4ZI0/exec?station={0}&page=wind";
        private List<Uri> uris = new List<Uri>();

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            TileUpdate();
        }

        private void TileUpdate()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);

            PeriodicUpdateRecurrence recurrence = PeriodicUpdateRecurrence.HalfHour;
            TileUpdateManager.CreateTileUpdaterForApplication().StartPeriodicUpdateBatch(uris, recurrence);
        }

        private async void FindStations_Click(object sender, RoutedEventArgs e)
        {
            string zipCode = NearbyTextBox.Text;
            WebRequest webRequest = WebRequest.Create("http://api.wunderground.com/api/94125ecc51d856e8/geolookup/q/" + zipCode +".json");
            WebResponse webResp = await webRequest.GetResponseAsync();
            StreamReader sr = new StreamReader(webResp.GetResponseStream());

            StringWriter sw = new StringWriter();
            string line;

            while((line = sr.ReadLine()) != null)
            {
                sw.WriteLine(line);
            }
            sw.Flush();

            JObject json = JObject.Parse(sw.ToString());

            foreach (var s in json["location"]["nearby_weather_stations"]["pws"]["station"])
            {
                Stations.Items.Add(s["id"] + "(" + s["neighborhood"] + ")");
            }
        }

        private void Stations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string value = (string)Stations.SelectedItem;
            stationId = value.Substring(0, value.IndexOf("("));

            uris.Add(new Uri(String.Format(uri1, stationId)));
            uris.Add(new Uri(String.Format(uri2, stationId)));

            TileUpdate();
        }
    }
}
