using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Sample.Data;
using Sample.DataAccessLayer;
using SQLite;
using System;
using System.Reflection;
using Fragment = Android.App.Fragment;
using View = Android.Views.View;

namespace Sample.Fragments
{
    public class CityRowFragment : Fragment
    {
        private enum SubmitModes
        {
            Add,
            Edit
        }

        private SubmitModes _submitMode;
        private int _id;

        public static CityRowFragment Instantiate()
        {
            var fragment = new CityRowFragment()
            {
                Arguments = new Bundle()
            };
            fragment.Arguments.PutInt("id", 0);

            return fragment;
        }

        public static CityRowFragment Instantiate(int id)
        {
            var fragment = new CityRowFragment()
            {
                Arguments = new Bundle()
            };
            fragment.Arguments.PutInt("id", id);

            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetHasOptionsMenu(true);

            // ID will be returned as zero if no mapping was found (i.e. we are adding, rather than editing)
            _id = Arguments.GetInt("id");
            _submitMode = (_id == 0) ? SubmitModes.Add : SubmitModes.Edit;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_row_edit, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            var title = view.FindViewById<TextView>(Resource.Id.title);
            title.Text = _submitMode.ToString() + " City";

            var readFields = view.FindViewById<LinearLayout>(Resource.Id.read_fields);
            var writeFields = view.FindViewById<LinearLayout>(Resource.Id.write_fields);
            var submitButton = view.FindViewById<AppCompatButton>(Resource.Id.btn_submit);

            Type type = typeof(City);
            var entity = _submitMode == SubmitModes.Add ? Activator.CreateInstance(type) : DBTable.Get(type, _id);

            foreach (var property in type.GetProperties())
            {
                if (Attribute.IsDefined(property, typeof(PrimaryKeyAttribute)))
                {
                    if (_submitMode == SubmitModes.Edit)
                    {
                        AddReadField(property.Name, property.GetValue(entity).ToString(), readFields);
                    }
                }
                /*else if (Attribute.IsDefined(property, typeof(ForeignKeyAttribute)) && _submitMode == SubmitModes.Edit)
                {
                    AddReadField(property.Name, property.GetValue(entity).ToString(), readFields);
                }*/
                else if (!Attribute.IsDefined(property, typeof(IgnoreAttribute)))
                {
                    var value = property.GetValue(entity);
                    string currentValue = (value != null) ? value.ToString() : "";

                    AddWriteField(property.Name, currentValue, writeFields);
                }
            }

            submitButton.Click += (s, e) =>
            {
                /*ProgressDialogHelper.RunTask(Activity, "Submitting...", () =>
                {*/
                    Submit(view, entity);
                    Activity.OnBackPressed();
                //});
            };
        }

        private void Submit(View view, object entity)
        {
            var readFields = view.FindViewById<LinearLayout>(Resource.Id.read_fields);
            var writeFields = view.FindViewById<LinearLayout>(Resource.Id.write_fields);

            var type = entity.GetType();

            for (var i = 0; i < readFields.ChildCount; i += 2)
            {
                var textField = readFields.GetChildAt(i) as TextView;
                var valueField = readFields.GetChildAt(i + 1) as EditText;

                var property = type.GetProperty(textField.Text);
                SetPropertyValue(property, entity, valueField.Text);
            }

            for (var i = 0; i < writeFields.ChildCount; i += 2)
            {
                var textField = writeFields.GetChildAt(i) as TextView;
                var valueField = writeFields.GetChildAt(i + 1) as EditText;

                var property = type.GetProperty(textField.Text);
                SetPropertyValue(property, entity, valueField.Text);
            }

            switch (_submitMode)
            {
                case SubmitModes.Add:
                    DBTable.Insert(entity);
                    break;
                case SubmitModes.Edit:
                    DBTable.Update(entity);
                    break;
            }
        }

        private void SetPropertyValue(PropertyInfo property, object entity, string valueText)
        {
            if (property.PropertyType == typeof(int))
            {
                int value = int.Parse(valueText);
                property.SetValue(entity, value);
            }
            else if (property.PropertyType == typeof(double))
            {
                double value = double.Parse(valueText);
                property.SetValue(entity, value);
            }
            else if (property.PropertyType == typeof(string))
            {
                property.SetValue(entity, valueText);
            }
            else if (property.PropertyType == typeof(DateTime))
            {
                DateTime value = DateTime.Parse(valueText);
                property.SetValue(entity, value);
            }
        }

        private void AddReadField(string fieldName, string currentValue, LinearLayout layout)
        {
            var textView = new TextView(Activity)
            {
                Text = fieldName,
                TextSize = 12.0f
            };
            textView.SetTextColor(Android.Graphics.Color.White);

            var editText = new EditText(Activity)
            {
                Text = currentValue,
                InputType = Android.Text.InputTypes.Null,
                Enabled = false
            };
            editText.SetTextColor(Android.Graphics.Color.White);

            layout.AddView(textView);
            layout.AddView(editText);
        }

        private void AddWriteField(string fieldName, string currentValue, LinearLayout layout)
        {
            var textView = new TextView(Activity)
            {
                Text = fieldName,
                TextSize = 12.0f
            };
            textView.SetTextColor(Android.Graphics.Color.White);

            var editText = new EditText(Activity)
            {
                Text = (_submitMode == SubmitModes.Edit) ? currentValue : "",
                InputType = Android.Text.InputTypes.TextVariationEmailAddress,
            };
            editText.SetTextColor(Android.Graphics.Color.White);

            layout.AddView(textView);
            layout.AddView(editText);
        }

        public override void OnPrepareOptionsMenu(IMenu menu)
        {
            /*var item = menu.FindItem(Resource.Id.action_search);
            item.SetEnabled(false);
            item.SetVisible(false);*/
        }
    }
}
