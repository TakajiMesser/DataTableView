using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Sample.Data;
using Sample.DataAccessLayer;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Sample.Activities
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

            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu_home);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

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

            var populateDatabase = FindViewById<RelativeLayout>(Resource.Id.populate_database);
            populateDatabase.Click += (s, e) =>
            {
                SetUpDataTableView();

                var alertDialog = new Android.App.AlertDialog.Builder(this, Resource.Style.AlertsDialogTheme)
                .SetMessage("Populated database with dummy values")
                .SetPositiveButton("OK", (sender, args) => { })
                .Create();

                alertDialog.Window.SetBackgroundDrawable(ContextCompat.GetDrawable(this, Resource.Drawable.rounded_border_dark));
                alertDialog.Show();
            };
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

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.action_menu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    return true;
                case Resource.Id.action_settings:
                    StartActivity(new Intent(this, typeof(SettingsActivity)));
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override void OnBackPressed()
        {
            if (FragmentManager.BackStackEntryCount > 1)
            {
                FragmentManager.PopBackStack();
            }
            else
            {
                StartActivity(new Intent(Intent.ActionMain)
                    .AddCategory(Intent.CategoryHome));
            }
        }
    }
}

