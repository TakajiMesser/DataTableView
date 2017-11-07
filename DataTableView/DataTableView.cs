using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Support.V4.Content.Res;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTableView
{
    [Register("com.DataTableView")]
    public class DataTableView : ViewGroup
    {
        private LinearLayout _headers;
        private RecyclerView _rows;
        private DataTableAdapter _adapter;
        private SQLiteConnection _connection;
        private TableMapping _tableMapping;
        private int _offset = 0;
        private bool _gettingRows = false;

        public bool LazyLoad { get; private set; }
        public int LazyLoadLimit { get; private set; }
        public ChoiceMode ChoiceMode { get; private set; }
        public float HeaderTextSize { get; private set; }
        public Color HeaderTextColor { get; private set; }
        public Typeface HeaderTypeface { get; private set; }
        public Drawable HeaderBackground { get; private set; }
        public float RowTextSize { get; private set; }
        public Color RowTextColor { get; private set; }
        public Typeface RowTypeface { get; private set; }
        public Drawable RowBackground { get; private set; }
        public Drawable HorizontalRowDivider { get; private set; }
        public Drawable VerticalRowDivider { get; private set; }

        public SQLiteConnection Connection
        {
            get => _connection;
            set => _connection = value;
        }

        public IEnumerable<int> SelectedIDs => _adapter.SelectedIDs;

        public EventHandler<DataTableAdapter.ItemClickEventArgs> ItemClick;
        public EventHandler<DataTableAdapter.ItemLongClickEventArgs> ItemLongClick;

        public DataTableView(Context context) : base(context) { }
        public DataTableView(Context context, IAttributeSet attrs) : base(context, attrs) { InitializeFromAttributes(context, attrs); }
        public DataTableView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle) { InitializeFromAttributes(context, attrs); }

        protected DataTableView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

        private void InitializeFromAttributes(Context context, IAttributeSet attrs)
        {
            var attr = context.ObtainStyledAttributes(attrs, Resource.Styleable.DataTableView, 0, 0);

            LazyLoad = attr.GetBoolean(Resource.Styleable.DataTableView_lazyLoad, false);
            LazyLoadLimit = attr.GetInteger(Resource.Styleable.DataTableView_lazyLoadLimit, 0);
            ChoiceMode = (ChoiceMode)attrs.GetAttributeIntValue(Android.Resource.Attribute.ChoiceMode, 0);
            HeaderTextSize = attr.GetFloat(Resource.Styleable.DataTableView_headerTextSize, 20.0f);
            HeaderTextColor = attr.GetColor(Resource.Styleable.DataTableView_headerTextColor, unchecked((int)0xFFFFFFFF));

            var headerFontID = attr.GetResourceId(Resource.Styleable.DataTableView_headerFontFamily, -1);
            /*if (headerFontID != -1)
            {
                HeaderTypeface = (true) ? ResourcesCompat.Get(Context, headerFontID) : Context.Resources.GetFont(headerFontID);
            }*/

            HeaderBackground = attr.GetDrawable(Resource.Styleable.DataTableView_headerBackground);
            RowTextSize = attr.GetFloat(Resource.Styleable.DataTableView_rowTextSize, DataTableAdapter.DEFAULT_TEXT_SIZE);
            RowTextColor = attr.GetColor(Resource.Styleable.DataTableView_rowTextColor, DataTableAdapter.DEFAULT_TEXT_COLOR);

            var rowFontID = attr.GetResourceId(Resource.Styleable.DataTableView_rowFontFamily, -1);
            /*if (rowFontID != -1)
            {
                RowTypeface = (true) ? ResourcesCompat.Get(Context, rowFontID) : Context.Resources.GetFont(rowFontID);
            }*/

            RowBackground = attr.GetDrawable(Resource.Styleable.DataTableView_rowBackground);
            HorizontalRowDivider = attr.GetDrawable(Resource.Styleable.DataTableView_horizontalRowDivider)
                ?? ResourcesCompat.GetDrawable(Resources, Resource.Drawable.horizontal_divider, null);
            VerticalRowDivider = attr.GetDrawable(Resource.Styleable.DataTableView_verticalRowDivider)
                ?? ResourcesCompat.GetDrawable(Resources, Resource.Drawable.vertical_divider, null);

            CreateHeaderView();
            CreateRowView();
        }

        private void CreateHeaderView()
        {
            _headers = new LinearLayout(Context)
            {
                Orientation = Orientation.Horizontal
            };

            if (HeaderBackground != null)
            {
                _headers.Background = HeaderBackground;
            }
        }

        private void CreateRowView()
        {
            _rows = new RecyclerView(Context);
            ((SimpleItemAnimator)_rows.GetItemAnimator()).SupportsChangeAnimations = false;

            var layoutManager = new LinearLayoutManager(Context);
            _rows.SetLayoutManager(layoutManager);

            var dividerDecoration = new DividerItemDecoration(Context, layoutManager.Orientation);
            dividerDecoration.SetDrawable(HorizontalRowDivider);
            _rows.AddItemDecoration(dividerDecoration);

            if (LazyLoad)
            {
                _rows.ScrollChange += (s, e) =>
                {
                    int firstVisibleItemPosition = layoutManager.FindFirstVisibleItemPosition();

                    if (!_gettingRows && firstVisibleItemPosition > 0 && _adapter.ItemCount % LazyLoadLimit == 0 && firstVisibleItemPosition + _rows.ChildCount >= _adapter.ItemCount)
                    {
                        _gettingRows = true;

                        var dialog = new ProgressDialog(Context, Resource.Style.ProgressDialogTheme)
                        {
                            Indeterminate = true
                        };
                        dialog.SetCancelable(false);
                        dialog.Show();

                        _adapter.AddRows(GetNextRowSet());

                        dialog.Dismiss();
                        _gettingRows = false;
                    }
                };
            }
        }

        public void Adapter_ItemClick(object sender, DataTableAdapter.ItemClickEventArgs e) => ItemClick?.Invoke(sender, e);

        public void Adapter_ItemLongClick(object sender, DataTableAdapter.ItemLongClickEventArgs e) => ItemLongClick?.Invoke(sender, e);

        public void SetTableMapping(TableMapping mapping)
        {
            _tableMapping = mapping;

            _adapter = new DataTableAdapter(Context)
            {
                TextSize = RowTextSize,
                TextColor = RowTextColor,
                Typeface = RowTypeface,
                RowBackground = RowBackground,
                VerticalDivider = VerticalRowDivider
            };
            _adapter.ItemClick += Adapter_ItemClick;
            _adapter.ItemLongClick += Adapter_ItemLongClick;
            _adapter.SetTableMapping(mapping);

            foreach (var row in GetNextRowSet())
            {
                _adapter.AddRow(row);
            }

            _rows.SetAdapter(_adapter);

            for (var i = 0; i < _tableMapping.Columns.Length; i++)
            {
                var headerText = new TextView(Context)
                {
                    Text = _tableMapping.Columns[i].Name,
                    //Typeface = FontHelper.GetTypeface(Context, CustomFonts.RobotoCondensedRegular)
                };
                headerText.SetTextColor(HeaderTextColor);
                headerText.SetPadding(20, 10, 10, 10);
                headerText.SetTextSize(ComplexUnitType.Dip, HeaderTextSize);

                var ems = _adapter.GetMaxCharacters(i);
                headerText.SetMinEms(ems);

                _headers.AddView(headerText);
            }
        }

        public IEnumerable<DataTableRow> GetNextRowSet()
        {
            var queryBuilder = new StringBuilder("SELECT * FROM " + _tableMapping.TableName);

            if (LazyLoad)
            {
                queryBuilder.Append(" LIMIT " + LazyLoadLimit + " OFFSET " + _offset);
            }
            _offset += LazyLoadLimit;

            foreach (var entity in Connection.Query(_tableMapping, queryBuilder.ToString()))
            {
                var row = new DataTableRow();

                foreach (var column in _tableMapping.Columns)
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

        public void Filter(string text) => _adapter.Filter.InvokeFilter(text);

        public void SelectAllItems() => _adapter.SetAllItemsChecked(true);

        public void DeleteSelectedItems() => _adapter.DeleteSelectedItems();

        public void SetMultiChoiceModeListener(AbsListView.IMultiChoiceModeListener listener) => _adapter.SetMultiChoiceModeListener(listener);

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();

            FocusableInTouchMode = true;

            AddView(_headers, GenerateDefaultLayoutParams());
            BringChildToFront(_headers);

            AddView(_rows, GenerateDefaultLayoutParams());
            BringChildToFront(_rows);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            /*for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);
                if (child.Visibility != ViewStates.Gone)
                {
                    MeasureChildWithMargins(child, widthMeasureSpec, 0, heightMeasureSpec, 0);
                }
            }

            var marginLayoutParameters = (MarginLayoutParams)LayoutParameters;*/

            MeasureChildren(widthMeasureSpec, heightMeasureSpec);

            /*int width = (IsOpened || LayoutParameters.Width == ViewGroup.LayoutParams.MatchParent)
                ? GetDefaultSize(SuggestedMinimumWidth, widthMeasureSpec) + marginLayoutParameters.LeftMargin + marginLayoutParameters.RightMargin
                : _menuButton.MeasuredWidth + PaddingLeft + PaddingRight;

            int height = (IsOpened || LayoutParameters.Height == ViewGroup.LayoutParams.MatchParent)
                ? GetDefaultSize(SuggestedMinimumHeight, heightMeasureSpec) + marginLayoutParameters.TopMargin + marginLayoutParameters.BottomMargin
                : _menuButton.MeasuredHeight + PaddingTop + PaddingBottom;*/

            int width = _headers.MeasuredWidth + PaddingLeft + PaddingRight;// GetDefaultSize(SuggestedMinimumWidth, widthMeasureSpec);
            int height = GetDefaultSize(SuggestedMinimumHeight, heightMeasureSpec);

            SetMeasuredDimension(width, height);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            int headerLeft = r - l - _headers.MeasuredWidth - PaddingRight;
            int headerRight = headerLeft + _headers.MeasuredWidth;
            int headerTop = t;//b - t;// - _headers.MeasuredHeight - PaddingBottom;
            int headerBottom = headerTop + _headers.MeasuredHeight;

            _headers.Layout(headerLeft, headerTop, headerRight, headerBottom);

            int rowLeft = headerLeft;
            int rowRight = r;// rowLeft + _rows.MeasuredWidth;
            int rowTop = headerBottom;
            int rowBottom = b;// rowTop + _rows.MeasuredHeight;

            _rows.Layout(rowLeft, rowTop, rowRight, rowBottom);
        }
    }
}
