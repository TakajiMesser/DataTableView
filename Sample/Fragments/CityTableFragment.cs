using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Messert.Controls.Droid;
using Sample.Data;
using Sample.DataAccessLayer;
using System.Collections.Generic;
using System.Linq;
using Fragment = Android.App.Fragment;
using View = Android.Views.View;

namespace Sample.Fragments
{
    public class CityTableFragment : Fragment, AbsListView.IMultiChoiceModeListener, IQueryPreparedListener
    {
        private DataTableView _dataTable;

        public static CityTableFragment Instantiate() => new CityTableFragment();

        public void Filter(string text) => _dataTable.Filter(text);

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_table_rows, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            _dataTable = view.FindViewById<DataTableView>(Resource.Id.table);

            var tableMapping = DBAccess.GetMapping(typeof(City));
            _dataTable.TableName = tableMapping.TableName;
            _dataTable.SetColumnNames(tableMapping.Columns.Select(c => c.Name));

            _dataTable.SetMultiChoiceModeListener(this);
            _dataTable.SetQueryPreparedListener(this);
            _dataTable.ItemClick += (s, e) =>
            {
                Activity.FragmentManager.BeginTransaction()
                    .AddToBackStack(null)
                    .Replace(Resource.Id.frame_layout, CityRowFragment.Instantiate(e.Row.ID))
                    .Commit();
            };

            _dataTable.LoadData();

            var addButton = view.FindViewById<FloatingActionButton>(Resource.Id.fab_add);
            addButton.Click += (s, e) =>
            {
                Activity.FragmentManager.BeginTransaction()
                    .AddToBackStack(null)
                    .Replace(Resource.Id.frame_layout, CityRowFragment.Instantiate())
                    .Commit();
            };
        }

        private void DeleteSelectedItems(ActionMode mode)
        {
            var titleView = new TextView(Context)
            {
                Text = "Delete selected rows",
                TextSize = 20,
                Gravity = GravityFlags.Center,
            };
            titleView.SetTextColor(Android.Graphics.Color.White);
            titleView.SetPadding(5, 5, 5, 2);
            titleView.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.ic_menu_notifications, 0, 0, 0);

            var alertDialog = new Android.App.AlertDialog.Builder(Context, Resource.Style.AlertsDialogTheme)
                .SetCustomTitle(titleView)
                .SetMessage("Are you sure?")
                .SetCancelable(true)
                .SetNegativeButton("Cancel", (s, args) => { })
                .SetPositiveButton("OK", (s, args) =>
                {
                    DBTable.DeleteAll(_dataTable.SelectedIDs, DBAccess.ParseTableName(_dataTable.TableName));
                    _dataTable.DeleteSelectedItems();
                    Activity.RunOnUiThread(() => mode.Finish());
                })
                .Create();

            alertDialog.Window.SetBackgroundDrawable(ContextCompat.GetDrawable(Context, Resource.Drawable.rounded_border_dark));
            alertDialog.Show();
        }

        private static bool IsActivityAlive(Activity activity)
        {
            return Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1
                ? !activity.IsDestroyed
                : !activity.IsFinishing;
        }

        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            mode.MenuInflater.Inflate(Resource.Menu.row_actions, menu);
            return true;
        }

        public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_delete:
                    DeleteSelectedItems(mode);
                    return true;
                case Resource.Id.action_select_all:
                    _dataTable.SelectAllItems();
                    return true;
                default:
                    return false;
            }
        }

        public void OnItemCheckedStateChanged(ActionMode mode, int position, long id, bool @checked) { }

        public void OnDestroyActionMode(ActionMode mode) { }

        public bool OnPrepareActionMode(ActionMode mode, IMenu menu) => false;

        public IEnumerable<DataTableRow> OnExecute(string query)
        {
            var tableMapping = DBAccess.GetMapping(typeof(City));
            foreach (var entity in DBAccess.Connection.Query(tableMapping, query))
            {
                var row = new DataTableRow();

                foreach (var column in tableMapping.Columns)
                {
                    var value = column.GetValue(entity);

                    if (column.IsPK)
                    {
                        row.ID = (int)value;
                    }

                    row.Cells.Add(value == null ? "NULL" : value.ToString());
                }

                yield return row;
            }
        }
    }
}
