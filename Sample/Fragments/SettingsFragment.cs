using Android.App;
using Android.OS;
using Android.Preferences;
using Android.Views;
using View = Android.Views.View;

namespace Sample.Fragments
{
    public class SettingsFragment : PreferenceFragment
    {
        public static SettingsFragment Instantiate() => new SettingsFragment();

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            InitializePreferences();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        private void InitializePreferences()
        {
            AddPreferencesFromResource(Resource.Layout.fragment_settings);

            var version = PreferenceManager.FindPreference("version");
            version.Summary = Activity.PackageManager.GetPackageInfo(Activity.PackageName, 0).VersionName;
        }
    }
}
