using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using EcoDrive.Droid.Fragments;
using System;
using Xamarin.Forms;
using AndroidResource = Android.Resource;
using SearchView = Android.Support.V7.Widget.SearchView;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace EcoDrive.Droid.Activities
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize, HardwareAccelerated = true)]
    public class DatabaseActivity : AppCompatActivity
    {
        public const string TITLE = "Database";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_base);

            var toolbar = FindViewById<Toolbar>(Resource.Id.main_toolbar);
            SetSupportActionBar(toolbar);

            SupportActionBar.Title = TITLE;
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu_home);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            FragmentManager.BeginTransaction()
                .AddToBackStack(null)
                .Add(Resource.Id.frame_layout, TableListFragment.Instantiate())
                .Commit();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.search_action_menu, menu);

            var searchItem = menu.FindItem(Resource.Id.action_search);
            ((SearchView)searchItem.ActionView).QueryTextChange += (s, e) =>
            {
                if (FragmentManager.FindFragmentById(Resource.Id.frame_layout) is ISearchFragment searchFragment)
                {
                    searchFragment.Filter(e.NewText);
                }
            };

            FragmentManager.BackStackChanged += (s, e) =>
            {
                MenuItemCompat.CollapseActionView(searchItem);
            };

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case AndroidResource.Id.Home:
                    StartActivity(new Intent(this, typeof(HomeActivity))
                        .SetFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask | ActivityFlags.NewTask));
                    Finish();
                    return true;
                case Resource.Id.action_settings:
                    StartActivity(new Intent(this, typeof(SettingsActivity)));
                    return true;
                case Resource.Id.action_feedback:
                    Device.OpenUri(new Uri("mailto:takaji.messer@traffictechservices.com?subject=EcoDrive-Feedback"));
                    return true;
                case Resource.Id.action_logout:
                    StartActivity(new Intent(this, typeof(LoginActivity))
                        .SetFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask | ActivityFlags.NewTask)
                        .PutExtra("logout", true));
                    Finish();
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
                StartActivity(new Intent(this, typeof(HomeActivity))
                    .SetFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask | ActivityFlags.NewTask));
                Finish();
            }
        }
    }
}
