using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Widget;
using Sample.Data;
using Sample.DataAccessLayer;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Sample
{
    [Activity(Label = "DataTableView Sample", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize, HardwareAccelerated = true, MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            var toolbar = FindViewById<Toolbar>(Resource.Id.main_toolbar);
            SetSupportActionBar(toolbar);

            var exampleOne = FindViewById<RelativeLayout>(Resource.Id.example_one);
            exampleOne.Click += (s, e) =>
            {
                StartActivity(new Intent(this, typeof(CountryActivity)));
            };

            var exampleTwo = FindViewById<RelativeLayout>(Resource.Id.example_two);
            exampleTwo.Click += (s, e) =>
            {
                StartActivity(new Intent(this, typeof(CityActivity)));
            };

            var preferences = Application.Context.GetSharedPreferences(Application.PackageName, FileCreationMode.Private);
            if (!preferences.Contains("IsStartup"))
            {
                SetUpDataTableView();
                preferences.Edit().PutBoolean("IsStartup", false).Commit();
            }
        }

        private void SetUpDataTableView()
        {
            DBAccess.ResetTables();

            var country = new Country()
            {
                Name = "Canada"
            };
            DBTable.Insert(country);

            var city = new City()
            {
                Name = "Edmonton",
                Latitude = 53.631611,
                Longitude = -113.323975,
                CountryID = DBTable.Get<Country>(c => c.Name == "Canada").ID
            };
            DBTable.Insert(city);
        }
    }
}

