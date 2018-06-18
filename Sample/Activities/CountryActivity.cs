using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Widget;
using Sample.Fragments;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Sample
{
    [Activity(Label = "Countries", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize, HardwareAccelerated = true)]
    public class CountryActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_table);

            var toolbar = FindViewById<Toolbar>(Resource.Id.main_toolbar);
            SetSupportActionBar(toolbar);

            FragmentManager.BeginTransaction()
                .AddToBackStack(null)
                .Add(Resource.Id.frame_layout, CountryTableFragment.Instantiate())
                .Commit();
        }

        public override void OnBackPressed()
        {
            if (FragmentManager.BackStackEntryCount > 1)
            {
                FragmentManager.PopBackStack();
            }
            else
            {
                Finish();
            }
        }
    }
}

