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
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Messert.Controls.Droid
{
    [Register("com.messert.datatable.droid.DataTableView")]
    public class DataTableView : ViewGroup
    {
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

        public string TableName { get; set; }
        public List<string> ColumnNames { get; private set; } = new List<string>();

        public IEnumerable<int> SelectedIDs => _adapter.SelectedIDs;

        public event EventHandler<DataTableAdapter.ItemClickEventArgs> ItemClick;
        public event EventHandler<DataTableAdapter.ItemLongClickEventArgs> ItemLongClick;

        private LinearLayout _headers;
        private RecyclerView _rows;
        private DataTableAdapter _adapter;
        private int _offset = 0;
        private bool _gettingRows = false;

        private IQueryPreparedListener _queryPreparedListener;

        public DataTableView(Context context) : base(context) { }
        public DataTableView(Context context, IAttributeSet attrs) : base(context, attrs) { InitializeFromAttributes(context, attrs); }
        public DataTableView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle) { InitializeFromAttributes(context, attrs); }

        protected DataTableView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

        public void SetQueryPreparedListener(IQueryPreparedListener listener) => _queryPreparedListener = listener;

        public void SetColumnNames(IEnumerable<string> columnNames)
        {
            ColumnNames.Clear();
            ColumnNames.AddRange(columnNames);
        }

        public void LoadData()
        {
            if (string.IsNullOrEmpty(TableName)) throw new InvalidOperationException("TableName must be set");
            if (ColumnNames.Count == 0) throw new InvalidOperationException("ColumnNames must be set");

            _adapter.SetColumnNames(ColumnNames);

            foreach (var row in GetNextRowSet())
            {
                _adapter.AddRow(row);
            }

            _rows.SetAdapter(_adapter);

            for (var i = 0; i < ColumnNames.Count; i++)
            {
                var headerText = new TextView(Context)
                {
                    Text = ColumnNames[i],
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

        public void SetMultiChoiceModeListener(AbsListView.IMultiChoiceModeListener listener) => _adapter.SetMultiChoiceModeListener(listener);

        public void Filter(string text) => _adapter.Filter.InvokeFilter(text);

        public void SelectAllItems() => _adapter.SetAllItemsChecked(true);

        public void DeleteSelectedItems() => _adapter.DeleteSelectedItems();

        private void InitializeFromAttributes(Context context, IAttributeSet attrs)
        {
            var attr = context.ObtainStyledAttributes(attrs, Resource.Styleable.DataTableView, 0, 0);

            LazyLoad = attr.GetBoolean(Resource.Styleable.DataTableView_lazyLoad, false);
            LazyLoadLimit = attr.GetInteger(Resource.Styleable.DataTableView_lazyLoadLimit, 0);
            ChoiceMode = (ChoiceMode)attrs.GetAttributeIntValue(Android.Resource.Attribute.ChoiceMode, 0);
            HeaderTextSize = attr.GetFloat(Resource.Styleable.DataTableView_headerTextSize, 20.0f);
            HeaderTextColor = attr.GetColor(Resource.Styleable.DataTableView_headerTextColor, unchecked((int)0xFFFFFFFF));

            var headerFontID = attr.GetResourceId(Resource.Styleable.DataTableView_headerFontFamily, -1);
            if (headerFontID >= 0)
            {
                HeaderTypeface = ResourcesCompat.GetFont(Context, headerFontID);
            }

            HeaderBackground = attr.GetDrawable(Resource.Styleable.DataTableView_headerBackground);
            RowTextSize = attr.GetFloat(Resource.Styleable.DataTableView_rowTextSize, DataTableAdapter.DEFAULT_TEXT_SIZE);
            RowTextColor = attr.GetColor(Resource.Styleable.DataTableView_rowTextColor, DataTableAdapter.DEFAULT_TEXT_COLOR);

            var rowFontID = attr.GetResourceId(Resource.Styleable.DataTableView_rowFontFamily, -1);
            if (rowFontID >= 0)
            {
                RowTypeface = ResourcesCompat.GetFont(Context, rowFontID);
            }

            RowBackground = attr.GetDrawable(Resource.Styleable.DataTableView_rowBackground);
            HorizontalRowDivider = attr.GetDrawable(Resource.Styleable.DataTableView_horizontalRowDivider)
                ?? ResourcesCompat.GetDrawable(Resources, Resource.Drawable.horizontal_divider, null);
            VerticalRowDivider = attr.GetDrawable(Resource.Styleable.DataTableView_verticalRowDivider)
                ?? ResourcesCompat.GetDrawable(Resources, Resource.Drawable.vertical_divider, null);

            CreateAdapter();
            CreateHeaderView();
            CreateRowView();
        }

        private void CreateAdapter()
        {
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

                        try
                        {
                            Task.Run(() => _adapter.AddRows(GetNextRowSet()));
                        }
                        finally
                        {
                            dialog.Dismiss();
                        }

                        _gettingRows = false;
                    }
                };
            }
        }

        private void Adapter_ItemClick(object sender, DataTableAdapter.ItemClickEventArgs e) => ItemClick?.Invoke(sender, e);

        private void Adapter_ItemLongClick(object sender, DataTableAdapter.ItemLongClickEventArgs e) => ItemLongClick?.Invoke(sender, e);

        private IEnumerable<DataTableRow> GetNextRowSet()
        {
            var queryBuilder = new StringBuilder("SELECT * FROM " + TableName);

            if (LazyLoad)
            {
                queryBuilder.Append(" LIMIT " + LazyLoadLimit + " OFFSET " + _offset);
            }
            _offset += LazyLoadLimit;

            foreach (var row in _queryPreparedListener?.OnExecute(queryBuilder.ToString()))
            {
                yield return row;
            }
        }

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
            MeasureChildren(widthMeasureSpec, heightMeasureSpec);

            int width = _headers.MeasuredWidth + PaddingLeft + PaddingRight;
            int height = GetDefaultSize(SuggestedMinimumHeight, heightMeasureSpec);

            SetMeasuredDimension(width, height);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            int headerLeft = r - l - _headers.MeasuredWidth - PaddingRight;
            int headerRight = headerLeft + _headers.MeasuredWidth;
            int headerTop = t;
            int headerBottom = headerTop + _headers.MeasuredHeight;

            _headers.Layout(headerLeft, headerTop, headerRight, headerBottom);

            int rowLeft = headerLeft;
            int rowRight = r;
            int rowTop = headerBottom;
            int rowBottom = b;

            _rows.Layout(rowLeft, rowTop, rowRight, rowBottom);
        }
    }
}
