using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace FeserWard.Controls
{
    [TemplatePart(Name = PART_TextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PART_ListView, Type = typeof(ListView))]
    [TemplatePart(Name = PART_WaitPop, Type = typeof(Popup))]
    [TemplatePart(Name = PART_NOResultPop, Type = typeof(Popup))]
    public class Intellibox : Control
    {
        #region 构造函数
        static Intellibox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Intellibox), new FrameworkPropertyMetadata(typeof(Intellibox)));
        }
        #endregion

        #region 变量
        public const string PART_TextBox = "PART_TextBox";
        public const string PART_ListView = "PART_ListView";
        public const string PART_WaitPop = "PART_WaitPop";
        public const string PART_NOResultPop = "PART_NOResultPop";
        private const int MinimumSearchDelayMS = 125;


        private ICommand _cancelAllSearches;
        private ICommand _showAllSearches;
        private DateTime _lastTimeSearchRecievedUtc;
        private string _lastTextValue;
        private BindingBase _displayedValueBinding;
        private BindingBase _selectedValueBinding;
        private IntelliboxColumnCollection columns;
        private static readonly Type[] _baseTypes = new[] {
            typeof(bool), typeof(byte), typeof(sbyte), typeof(char), typeof(decimal),
            typeof(double), typeof(float),
            typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
            typeof(string)
        };
        #endregion

        #region 属性
        public GridView GridView
        {
            get; set;
        }
        /// <summary>
        /// Identifies the <see cref="DataProviderProperty"/> Dependancy Property.
        /// </summary>
        public static readonly DependencyProperty DataProviderProperty =
            DependencyProperty.Register("DataProvider", typeof(IIntelliboxResultsProvider), typeof(Intellibox),
            new UIPropertyMetadata(new PropertyChangedCallback(OnDataProviderChanged)));

        /// <summary>
        /// For Internal Use Only. Identifies the <see cref="DisplayTextFromHighlightedItemProperty"/> Dependancy Property.
        /// </summary>
        protected static readonly DependencyProperty DisplayTextFromHighlightedItemProperty =
            DependencyProperty.Register("DisplayTextFromHighlightedItem", typeof(string), typeof(Intellibox), new UIPropertyMetadata(null));

        /// <summary>
        /// For Internal Use Only. Identifies the <see cref="DisplayTextFromSelectedItemProperty"/> Dependancy Property.
        /// </summary>
        protected static readonly DependencyProperty DisplayTextFromSelectedItemProperty =
            DependencyProperty.Register("DisplayTextFromSelectedItem", typeof(string), typeof(Intellibox), new UIPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="HideColumnHeadersProperty"/> Dependancy Property.
        /// </summary>
        public static readonly DependencyProperty HideColumnHeadersProperty =
            DependencyProperty.Register("HideColumnHeaders", typeof(bool), typeof(Intellibox), new UIPropertyMetadata(false));

        /// <summary>
        /// For Internal Use Only. Identifies the <see cref="ItemsProperty"/> Dependancy Property.
        /// </summary>
        protected static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(IList), typeof(Intellibox), new UIPropertyMetadata(null));

        /// <summary>
        /// For Internal Use Only. Identifies the <see cref="IntermediateSelectedValueProperty"/> Dependancy Property.
        /// </summary>
        public static readonly DependencyProperty IntermediateSelectedValueProperty =
            DependencyProperty.Register("IntermediateSelectedValue", typeof(object), typeof(Intellibox), new UIPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="MaxResultsProperty"/> Dependancy Property.
        /// </summary>
        public static readonly DependencyProperty MaxResultsProperty =
            DependencyProperty.Register("MaxResults", typeof(int), typeof(Intellibox), new UIPropertyMetadata(10));

        /// <summary>
        /// Identifies the <see cref="MinimumPrefixLengthProperty"/> Dependancy Property.
        /// </summary>
        public static readonly DependencyProperty MinimumPrefixLengthProperty =
            DependencyProperty.Register("MinimumPrefixLength", typeof(int), typeof(Intellibox),
            new UIPropertyMetadata(1, null, CoerceMinimumPrefixLengthProperty));

        /// <summary>
        /// Identifies the <see cref="MinimumSearchDelayProperty"/> Dependancy Property. Default is 250 milliseconds.
        /// </summary>
        public static readonly DependencyProperty MinimumSearchDelayProperty =
            DependencyProperty.Register("MinimumSearchDelay", typeof(int), typeof(Intellibox),
            new UIPropertyMetadata(250, null, CoerceMinimumSearchDelayProperty));

        /// <summary>
        /// Identifies the <see cref="NoResultsTextProperty"/> Dependancy Property.
        /// </summary>
        protected static readonly DependencyProperty NoResultsTextProperty =
            DependencyProperty.Register("NoResultsText", typeof(string), typeof(Intellibox), new UIPropertyMetadata("No results found"));

        /// <summary>
        ///Using a DependencyProperty as the backing store for PageUpOrDownScrollRows.  This enables animation, styling, binding, etc... 
        /// </summary>
        public static readonly DependencyProperty PagingScrollRowsProperty =
            DependencyProperty.Register("PagingScrollRows", typeof(int), typeof(Intellibox), new UIPropertyMetadata(0));

        /// <summary>
        /// Identifies the <see cref="ResultsHeightProperty"/> Dependancy Property.
        /// </summary>
        public static readonly DependencyProperty ResultsHeightProperty =
            DependencyProperty.Register("ResultsHeight", typeof(double), typeof(Intellibox), new UIPropertyMetadata(double.NaN));

        /// <summary>
        /// Identifies the <see cref="ResultsMaxHeightProperty"/> Dependancy Property.
        /// </summary>
        public static readonly DependencyProperty ResultsMaxHeightProperty =
            DependencyProperty.Register("ResultsMaxHeight", typeof(double), typeof(Intellibox), new UIPropertyMetadata(double.PositiveInfinity));

        /// <summary>
        /// Identifies the <see cref="ResultsMaxWidthProperty"/> Dependancy Property.
        /// </summary>
        public static readonly DependencyProperty ResultsMaxWidthProperty =
            DependencyProperty.Register("ResultsMaxWidth", typeof(double), typeof(Intellibox), new UIPropertyMetadata(double.PositiveInfinity));

        /// <summary>
        /// Identifies the <see cref="ResultsMinHeightProperty"/> Dependancy Property.
        /// </summary>
        public static readonly DependencyProperty ResultsMinHeightProperty =
            DependencyProperty.Register("ResultsMinHeight", typeof(double), typeof(Intellibox), new UIPropertyMetadata(0d));

        /// <summary>
        /// Identifies the <see cref="ResultsMinWidthProperty"/> Dependancy Property.
        /// </summary>
        public static readonly DependencyProperty ResultsMinWidthProperty =
                    DependencyProperty.Register("ResultsMinWidth", typeof(double), typeof(Intellibox), new UIPropertyMetadata(0d));

        /// <summary>
        /// Identifies the <see cref="ResultsWidthProperty"/> Dependancy Property.
        /// </summary>
        public static readonly DependencyProperty ResultsWidthProperty =
            DependencyProperty.Register("ResultsWidth", typeof(double), typeof(Intellibox), new UIPropertyMetadata(double.NaN));

        /// <summary>
        /// Identifies the <see cref="SelectedItemProperty"/> Dependancy Property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(Intellibox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnSelectedItemChanged)));

        /// <summary>
        /// Identifies the <see cref="SelectedValueProperty"/> Dependancy Property.
        /// </summary>
        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register("SelectedValue", typeof(object), typeof(Intellibox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// For Internal Use Only. Identifies the <see cref="ShowResultsProperty"/> Dependancy Property.
        /// </summary>
        protected static readonly DependencyProperty ShowResultsProperty =
            DependencyProperty.Register("ShowResults", typeof(bool), typeof(Intellibox), new UIPropertyMetadata(false));


        // Using a DependencyProperty as the backing store for AutoFocus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoFocusProperty =
            DependencyProperty.Register("AutoFocus", typeof(bool), typeof(Intellibox), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="WatermarkBackground"/> Dependancy Property.
        /// </summary>
        public static readonly DependencyProperty WatermarkBackgroundProperty =
            DependencyProperty.Register("WatermarkBackground", typeof(Brush), typeof(Intellibox), new UIPropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        /// <summary>
        /// Identifies the <see cref="WatermarkFontStyle"/> Dependancy Property.
        /// </summary>
        public static readonly DependencyProperty WatermarkFontStyleProperty =
            DependencyProperty.Register("WatermarkFontStyle", typeof(FontStyle), typeof(Intellibox), new UIPropertyMetadata(FontStyles.Italic));

        /// <summary>
        /// Identifies the <see cref="TimeBeforeWaitNotification"/> Dependancy Property. Default is 125 milliseconds.
        /// </summary>
        public static readonly DependencyProperty TimeBeforeWaitNotificationProperty =
            DependencyProperty.Register("TimeBeforeWaitNotification", typeof(int), typeof(Intellibox),
            new UIPropertyMetadata(125, null, CoerceTimeBeforeWaitNotificationProperty));

        /// <summary>
        /// Identifies the <see cref="WatermarkFontWeight"/> Dependancy Property.
        /// </summary>
        public static readonly DependencyProperty WatermarkFontWeightProperty =
            DependencyProperty.Register("WatermarkFontWeight", typeof(FontWeight), typeof(Intellibox), new UIPropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Identifies the <see cref="WatermarkForeground"/> Dependancy Property.
        /// </summary>
        public static readonly DependencyProperty WatermarkForegroundProperty =
            DependencyProperty.Register("WatermarkForeground", typeof(Brush), typeof(Intellibox), new UIPropertyMetadata(new SolidColorBrush(Colors.Gray)));

        /// <summary>
        /// Identifies the <see cref="WatermarkText"/> Dependancy Property.
        /// </summary>
        public static readonly DependencyProperty WatermarkTextProperty =
            DependencyProperty.Register("WatermarkText", typeof(string), typeof(Intellibox), new UIPropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the <see cref="AutoSelectSingleResult"/> Dependancy Property
        /// </summary>
        public static readonly DependencyProperty AutoSelectSingleResultProperty =
            DependencyProperty.Register("AutoSelectSingleResult", typeof(bool), typeof(Intellibox),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Identifies the <see cref="SingleClickToSelectResult"/> Dependancy Property
        /// </summary>
        public static readonly DependencyProperty SingleClickToSelectResultProperty =
            DependencyProperty.Register("SingleClickToSelectResult", typeof(bool), typeof(Intellibox),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public TextBox TxtInput { get; private set; }
        public ListView ListViewResult { get; private set; }
        public Popup PopNoResult { get; private set; }
        public Popup PopWait { get; private set; }

        /// <summary>
        /// This event is fired immediately before a new search is started.
        /// Note that not every <see cref="SearchBeginning"/> event has a matching <see cref="SearchCompleted"/> event.
        /// </summary>
        public event Action<string, int, object> SearchBeginning;

        /// <summary>
        /// This event is fired once a search has completed and the search results have been processed.
        /// Note that not every <see cref="SearchBeginning"/> event has a matching <see cref="SearchCompleted"/> event.
        /// </summary>
        public event Action SearchCompleted;

        /// <summary>
        /// Cancel all pending searches for the provider.
        /// </summary>
        public ICommand CancelAllSearches
        {
            get
            {
                if (_cancelAllSearches == null)
                {
                    _cancelAllSearches = new DelegateCommand(CancelSelection);
                }
                return _cancelAllSearches;
            }
        }

        /// <summary>
        /// show results
        /// </summary>
        public ICommand ShowAllResultsCommand
        {
            get
            {
                if (_showAllSearches == null)
                {
                    _showAllSearches = new DelegateCommand(OnShowAllResults);
                }
                return _showAllSearches;
            }
        }

        /// <summary>
        /// The columns in the search result set to display. When <see cref="ExplicitlyIncludeColumns"/>
        /// is set to true, then only the <see cref="IntelliboxColumn"/>s in this collection will be shown.
        /// Setting <see cref="HideColumnHeaders"/> to true will prevent column headers from being shown.
        /// </summary>
        public IntelliboxColumnCollection Columns
        {
            get
            {
                if (columns == null)
                {
                    columns = new IntelliboxColumnCollection();
                }
                return columns;
            }
            set => columns = value;
        }

        /// <summary>
        /// This is the <see cref="IIntelliboxResultsProvider"/> that the <see cref="Intellibox"/> uses
        /// to ask for search results. This is a Dependancy Property.
        /// </summary>
        public IIntelliboxResultsProvider DataProvider
        {
            get
            {
                return (IIntelliboxResultsProvider)GetValue(DataProviderProperty);
            }
            set
            {
                SetValue(DataProviderProperty, value);
            }
        }

        /// <summary>
        /// When True, the text in the search field will NOT be trimmed for
        /// whitespace prior to being passed to the <see cref="DataProvider"/>.
        /// </summary>
        public bool DisableWhitespaceTrim
        {
            get;
            set;
        }

        /// <summary>
        /// A binding expression that determines which column in the search result set
        /// displays its value in the text field. Typically, the value displayed should
        /// correspond to the column the <see cref="DataProvider"/> searches on. This binding
        /// expression can be different from the on in the <see cref="SelectedValueBinding"/>.
        /// If this property is NULL, then an entire row from the search result set displays
        /// its value in the text field.
        /// This is a Dependancy Property.
        /// </summary>
        public BindingBase DisplayedValueBinding
        {
            get
            {
                return _displayedValueBinding;
            }
            set
            {
                if (_displayedValueBinding != value)
                {
                    _displayedValueBinding = value;
                    //the call is commented out so that people can type w/o the displayed value overwriting what they're trying to do
                    OnDisplayedValueBindingChanged();
                }
            }
        }

        public string DisplayTextFromHighlightedItem
        {
            get
            {
                return (string)GetValue(DisplayTextFromHighlightedItemProperty);
            }
            set
            {
                SetValue(DisplayTextFromHighlightedItemProperty, value);
            }
        }

        public string DisplayTextFromSelectedItem
        {
            get
            {
                return (string)GetValue(DisplayTextFromSelectedItemProperty);
            }
            set
            {
                SetValue(DisplayTextFromSelectedItemProperty, value);
            }
        }

        /// <summary>
        /// When True, only the <see cref="IntelliboxColumn"/>s in the <see cref="Columns"/> collection
        /// will display in the search results set. When False, all the columns in the search result set
        /// will show, but any columns in the <see cref="Columns"/> collection then override specific columns.
        /// </summary>
        public bool ExplicitlyIncludeColumns
        {
            get;
            set;
        }

        private bool HasDataProvider
        {
            get
            {
                return DataProvider != null && SearchProvider != null;
            }
        }

        private bool HasItems
        {
            get
            {
                return Items != null && Items.Count > 0;
            }
        }

        /// <summary>
        /// When True, columns in the search result set will not have headers. This is a Dependancy Property.
        /// </summary>
        public bool HideColumnHeaders
        {
            get
            {
                return (bool)GetValue(HideColumnHeadersProperty);
            }
            set
            {
                SetValue(HideColumnHeadersProperty, value);
            }
        }

        /// <summary>
        /// When true, means that the control is in 'Search' mode.
        /// i.e. that it is firing searches as the user types and waiting for results.
        /// </summary>
        private bool IsSearchInProgress
        {
            get
            {
                return SearchTimer != null;
            }
        }

        /// <summary>
        /// This is the binding target of the <see cref="SelectedValueBinding"/> property,
        /// so that users of the control can place their own bindings on the <see cref="SelectedValue"/> property.
        /// </summary>
        private object IntermediateSelectedValue
        {
            get
            {
                return GetValue(IntermediateSelectedValueProperty);
            }
            set
            {
                SetValue(IntermediateSelectedValueProperty, value);
            }
        }

        private IList Items
        {
            get
            {
                return (IList)GetValue(ItemsProperty);
            }
            set
            {
                SetValue(ItemsProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of results that the <see cref="Intellibox"/> asks
        /// its <see cref="IIntelliboxResultsProvider"/> for. This is a Dependancy Property.
        /// </summary>
        public int MaxResults
        {
            get
            {
                return (int)GetValue(MaxResultsProperty);
            }
            set
            {
                SetValue(MaxResultsProperty, value);
            }
        }

        /// <summary>
        /// The minimum number of characters to wait for the user to enter before starting the first search.
        /// After the first search has been started, the <see cref="MinimumSearchDelay"/> property controls how often
        /// additional searches are performed (assumming that additional text has been entered).
        /// Minimum value is 1 (one). Defaults to 1 (one);
        /// </summary>
        public int MinimumPrefixLength
        {
            get
            {
                return (int)GetValue(MinimumPrefixLengthProperty);
            }
            set
            {
                SetValue(MinimumPrefixLengthProperty, value);
            }
        }

        /// <summary>
        /// The number of milliseconds the <see cref="Intellibox"/> control will wait between searches
        /// when the user is rapidly entering text. Minimum is 125 milliseconds. Defaults to 250 milliseconds.
        /// </summary>
        public int MinimumSearchDelay
        {
            get
            {
                return (int)GetValue(MinimumSearchDelayProperty);
            }
            set
            {
                SetValue(MinimumSearchDelayProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the text displayed in the search results popup when no records are found.
        /// This is a Dependancy Property.
        /// </summary>
        public string NoResultsText
        {
            get
            {
                return (string)GetValue(NoResultsTextProperty);
            }
            set
            {
                SetValue(NoResultsTextProperty, value);
            }
        }

        /// <summary>
        /// The number of rows to scroll up or down when a user uses the Page Up or Page Down key.
        /// </summary>
        public int PagingScrollRows
        {
            get
            {
                return (int)GetValue(PagingScrollRowsProperty);
            }
            set
            {
                SetValue(PagingScrollRowsProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the suggested height that the search results popup.
        /// The default value is 200.
        /// This is a Dependancy Property.
        /// </summary>
        public double ResultsHeight
        {
            get
            {
                return (double)GetValue(ResultsHeightProperty);
            }
            set
            {
                SetValue(ResultsHeightProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum height that the search results popup is allowed to have.
        /// This is a Dependancy Property.
        /// </summary>
        public double ResultsMaxHeight
        {
            get
            {
                return (double)GetValue(ResultsMaxHeightProperty);
            }
            set
            {
                SetValue(ResultsMaxHeightProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum width that the search results popup is allowed to have.
        /// This is a Dependancy Property.
        /// </summary>
        public double ResultsMaxWidth
        {
            get
            {
                return (double)GetValue(ResultsMaxWidthProperty);
            }
            set
            {
                SetValue(ResultsMaxWidthProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the minimum height that the search results popup is allowed to have.
        /// This is a Dependancy Property.
        /// </summary>
        public double ResultsMinHeight
        {
            get
            {
                return (double)GetValue(ResultsMinHeightProperty);
            }
            set
            {
                SetValue(ResultsMinHeightProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the minimum width that the search results popup is allowed to have.
        /// This is a Dependancy Property.
        /// </summary>
        public double ResultsMinWidth
        {
            get
            {
                return (double)GetValue(ResultsMinWidthProperty);
            }
            set
            {
                SetValue(ResultsMinWidthProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the suggested width that the search results popup is allowed to have.
        /// The default value is 400.
        /// This is a Dependancy Property.
        /// </summary>
        public double ResultsWidth
        {
            get
            {
                return (double)GetValue(ResultsWidthProperty);
            }
            set
            {
                SetValue(ResultsWidthProperty, value);
            }
        }

        /// <summary>
        /// The Search provider that will actually perform the search
        /// </summary>
        private IntelliboxAsyncProvider SearchProvider
        {
            get;
            set;
        }

        /// <summary>
        /// Using a dispatcher timer so that the 'Tick' event gets posted on the UI thread and
        /// we don't have to worry about exceptions throwing when accessing UI controls.
        /// </summary>
        private DispatcherTimer SearchTimer
        {
            get;
            set;
        }

        /// <summary>
        /// When true, all of the text in the field will be selected when the control gets focus.
        /// </summary>
        public bool SelectAllOnFocus
        {
            get;
            set;
        }

        /// <summary>
        /// The data row from the search result set that the user has most recently selected and confirmed.
        /// This is a Dependancy Property.
        /// </summary>
        public object SelectedItem
        {
            get
            {
                return GetValue(SelectedItemProperty);
            }
            set
            {
                SetValue(SelectedItemProperty, value);
            }
        }

        /// <summary>
        /// A value out of the <see cref="SelectedItem"/>. The exact value depends on
        /// the <see cref="SelectedValueBinding"/> property. This is a Dependancy Property.
        /// </summary>
        public object SelectedValue
        {
            get
            {
                return GetValue(SelectedValueProperty);
            }
            set
            {
                SetValue(SelectedValueProperty, value);
            }
        }

        /// <summary>
        /// A binding expression that determines what <see cref="SelectedValue"/>
        /// will be chosen out of the <see cref="SelectedItem"/>. If this property is
        /// NULL, then the entire <see cref="SelectedItem"/> is chosen as the <see cref="SelectedValue"/>.
        /// This property exists so that the <see cref="SelectedValue" /> can differ from the
        /// value displayed in the text field.
        /// This is a Dependancy Property.
        /// </summary>
        public BindingBase SelectedValueBinding
        {
            get
            {
                return _selectedValueBinding;
            }
            set
            {
                if (_selectedValueBinding != value)
                {
                    _selectedValueBinding = value;
                    OnSelectedValueBindingChanged();
                }
            }
        }

        /// <summary>
        /// When <see cref="bool"/> query results that have only a single result will will be automatically selected.
        /// </summary>
        public bool AutoSelectSingleResult
        {
            get
            {
                return (bool)GetValue(AutoSelectSingleResultProperty);
            }
            set
            {
                SetValue(AutoSelectSingleResultProperty, value);
            }
        }

        private bool ShowResults
        {
            get
            {
                return (bool)GetValue(ShowResultsProperty);
            }
            set
            {
                SetValue(ShowResultsProperty, value);
            }
        }

        public bool AutoFocus
        {
            get { return (bool)GetValue(AutoFocusProperty); }
            set { SetValue(AutoFocusProperty, value); }
        }
        /// <summary>
        /// The amount of time (in milliseconds) that the <see cref="Intellibox"/> control
        /// will wait for results to come back before showing the user a "Waiting for results" message.
        /// Minimum: 0ms, Default: 125ms
        /// </summary>
        public int TimeBeforeWaitNotification
        {
            get
            {
                return (int)GetValue(TimeBeforeWaitNotificationProperty);
            }
            set
            {
                SetValue(TimeBeforeWaitNotificationProperty, value);
            }
        }

        private DispatcherTimer WaitNotificationTimer
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the background <see cref="Brush"/> of the <see cref="WatermarkText"/> when it is displayed.
        /// </summary>
        public Brush WatermarkBackground
        {
            get
            {
                return (Brush)GetValue(WatermarkBackgroundProperty);
            }
            set
            {
                SetValue(WatermarkBackgroundProperty, value);
            }
        }

        /// <summary>
        /// Sets the <see cref="FontStyle"/> of the <see cref="WatermarkText"/> when it is displayed.
        /// Default is <see cref="FontStyles.Italic"/>.
        /// </summary>
        public FontStyle WatermarkFontStyle
        {
            get
            {
                return (FontStyle)GetValue(WatermarkFontStyleProperty);
            }
            set
            {
                SetValue(WatermarkFontStyleProperty, value);
            }
        }

        /// <summary>
        /// Sets the <see cref="FontWeight"/> of the <see cref="WatermarkText"/> when it is displayed.
        /// </summary>
        public FontWeight WatermarkFontWeight
        {
            get
            {
                return (FontWeight)GetValue(WatermarkFontWeightProperty);
            }
            set
            {
                SetValue(WatermarkFontWeightProperty, value);
            }
        }

        /// <summary>
        /// Sets the foreground <see cref="Brush"/> of the <see cref="WatermarkText"/> when it is displayed.
        /// Default is <see cref="Colors.Gray"/>.
        /// </summary>
        public Brush WatermarkForeground
        {
            get
            {
                return (Brush)GetValue(WatermarkForegroundProperty);
            }
            set
            {
                SetValue(WatermarkForegroundProperty, value);
            }
        }

        /// <summary>
        /// Sets the text that is displayed when the <see cref="Intellibox"/> doesn't have focus or any entered content.
        /// </summary>
        public string WatermarkText
        {
            get
            {
                return (string)GetValue(WatermarkTextProperty);
            }
            set
            {
                SetValue(WatermarkTextProperty, value);
            }
        }

        /// <summary>
        /// When True, selects the result after a single click instead of a double click. This is a Dependancy Property.
        /// </summary>
        public bool SingleClickToSelectResult
        {
            get
            {
                return (bool)GetValue(SingleClickToSelectResultProperty);
            }
            set
            {
                SetValue(SingleClickToSelectResultProperty, value);
            }
        }
        #endregion

        #region 方法
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _lastTimeSearchRecievedUtc = DateTime.Now.ToUniversalTime();
            PagingScrollRows = 5;
            OnSelectedValueBindingChanged();
            OnDisplayedValueBindingChanged();

            PopNoResult = Template.FindName(PART_NOResultPop, this) as Popup;
            PopWait = Template.FindName(PART_WaitPop, this) as Popup;
            ListViewResult = Template.FindName(PART_ListView, this) as ListView;
            if (ListViewResult != null)
            {
                ListViewResult.PreviewKeyDown += ListViewResult_PreviewKeyDown;
                ListViewResult.MouseDoubleClick += ListViewResult_MouseDoubleClick;
                ListViewResult.PreviewMouseLeftButtonUp += ListViewResult_PreviewMouseLeftButtonUp;
            }
            TxtInput = Template.FindName(PART_TextBox, this) as TextBox;
            if (TxtInput != null)
            {
                TxtInput.PreviewKeyDown += TxtInput_PreviewKeyDown;
                TxtInput.KeyUp += TxtInput_KeyUp;
                TxtInput.GotFocus += TxtInput_GotFocus;
            }

            UpdateSearchBoxText(true);
        }

        private void TxtInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SelectAllOnFocus)
            {
                TxtInput.SelectAll();
            }
        }

        private void TxtInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (!HasDataProvider)
            {
                return;
            }

            var field = sender as TextBox;
            if (field != null)
            {
                if ((e.Key == Key.Down || e.Key == Key.NumPad2) && ShowResults == false)
                {
                    if (string.IsNullOrEmpty(field.Text))
                    {
                        OnShowAllResults();
                    }
                    else
                    {
                        PerformSearchActions(field.Text);
                    }
                    return;
                }
                else if (IsCancelKey(e.Key) || IsChooseCurrentItemKey(e.Key) || IsNavigationKey(e.Key))
                {
                    return;
                }

                PerformSearchActions(field.Text);
            }
        }

        private void TxtInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!HasDataProvider || !ShowResults)
            {
                return;
            }

            if (IsCancelKey(e.Key))
            {
                CancelSelection();
                return;
            }

            if (IsChooseCurrentItemKey(e.Key))
            {
                ChooseCurrentItem();
                e.Handled = true;
                return;
            }

            if (IsNavigationKey(e.Key))
            {
                HighlightNextItem(e.Key);
                e.Handled = true;
            }
        }

        private void ListViewResult_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var flag = !(e.OriginalSource is RepeatButton || e.OriginalSource is Thumb);//判断鼠标点击的是滚动调部分
            if (flag && SingleClickToSelectResult)
            {
                ChooseCurrentItem();
            }
        }

        private void ListViewResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!SingleClickToSelectResult)
            {
                ChooseCurrentItem();
            }
        }

        private void ListViewResult_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsCancelKey(e.Key))
            {
                CancelSelection();
                return;
            }
        }

        private void HighlightNextItem(Key pressed)
        {
            if (ListViewResult != null && HasItems)
            {
                //I used this solution partially
                //http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=324064
                //the only way I have been able to solve the lockups is to use the background priority
                //the default still causes lockups.
                //be very careful changing this line
                Dispatcher.BeginInvoke(new Action<Key>(HighlightNewItem), DispatcherPriority.Background, pressed);
            }
        }
        private int GetIncrementValueForKey(Key pressed)
        {
            switch (pressed)
            {
                case Key.Down:
                case Key.Up:
                case Key.NumPad8:
                case Key.NumPad2:
                    return 1;
                case Key.PageDown:
                    return PagingScrollRows;
                case Key.PageUp:
                    return PagingScrollRows;
                default:
                    return 0;
            }
        }
        private void HighlightNewItem(Key pressed)
        {
            var goDown = pressed == Key.Tab || pressed == Key.Down || pressed == Key.NumPad2 || pressed == Key.PageDown;
            var nextIndex = goDown
                ? ListViewResult.SelectedIndex + GetIncrementValueForKey(pressed)
                : ListViewResult.SelectedIndex - GetIncrementValueForKey(pressed);

            int maxIndex = Items.Count - 1; //dangerous, since the list could be really large

            if (nextIndex < 0)
            {
                if (ListViewResult.SelectedIndex != 0)
                {
                    nextIndex = 0;
                }
                else
                {
                    nextIndex = maxIndex;
                }
            }

            if (nextIndex >= maxIndex)
            {
                if (ListViewResult.SelectedIndex != maxIndex)
                {
                    nextIndex = maxIndex;
                }
                else
                {
                    nextIndex = 0;
                }
            }

            var selectedItem = Items[nextIndex];

            ListViewResult.SelectedItem = selectedItem;
            ListViewResult.ScrollIntoView(selectedItem);
        }
        private bool IsNavigationKey(Key pressed)
        {
            var numLockOn = (Keyboard.GetKeyStates(Key.NumLock) == KeyStates.Toggled);
            return pressed == Key.Down
                || pressed == Key.Up
                || (pressed == Key.NumPad8 && !numLockOn)
                || (pressed == Key.NumPad2 && !numLockOn)
                || pressed == Key.PageUp    //TODO need to handle navigation keys that skip items
                || pressed == Key.PageDown;
        }
        private bool IsChooseCurrentItemKey(Key pressed)
        {
            return pressed == Key.Enter || pressed == Key.Return || pressed == Key.Tab;
        }

        private bool IsCancelKey(Key key)
        {
            return key == Key.Escape;
        }
        private static void OnDataProviderChanged(DependencyObject receiver, DependencyPropertyChangedEventArgs args)
        {
            var ib = receiver as Intellibox;
            if (ib != null && args != null && args.NewValue is IIntelliboxResultsProvider)
            {
                if (ib.SearchProvider != null)
                {
                    ib.SearchProvider.CancelAllSearches();
                }

                var provider = args.NewValue as IIntelliboxResultsProvider;
                //Create the wrapper used to make the calls async. This hides the details from the user.
                ib.SearchProvider = new IntelliboxAsyncProvider(provider.DoSearch);
            }
        }

        private static object CoerceTimeBeforeWaitNotificationProperty(DependencyObject reciever, object val)
        {
            return (int)val < 0 ? 0 : val;
        }


        private static object CoerceMinimumPrefixLengthProperty(DependencyObject reciever, object val)
        {
            var intval = (int)val;
            if (intval < 1)
            {
                intval = 1;
            }

            return intval;
        }

        private static object CoerceMinimumSearchDelayProperty(DependencyObject reciever, object val)
        {
            var intval = (int)val;
            if (intval < MinimumSearchDelayMS)
            {
                intval = MinimumSearchDelayMS;
            }

            return intval;
        }

        private static void OnSelectedItemChanged(DependencyObject receiver, DependencyPropertyChangedEventArgs args)
        {
            var ib = receiver as Intellibox;
            if (ib != null)
            {
                ib.OnDisplayedValueBindingChanged();
                ib._lastTextValue = ib.UpdateSearchBoxText(true);

                // have to set this after the SelectedItem property is set
                ib.OnSelectedValueBindingChanged();
                ib.SetValue(SelectedValueProperty, ib.IntermediateSelectedValue);
            }
        }

        private void OnSelectedValueBindingChanged()
        {
            var bind = BindingBaseFactory.ConstructBindingForSelected(this, SelectedValueBinding);
            this.SetBinding(IntermediateSelectedValueProperty, bind);
        }

        private string UpdateSearchBoxText(bool useSelectedItem)
        {
            var text = useSelectedItem
                ? this.DisplayTextFromSelectedItem
                : this.DisplayTextFromHighlightedItem;
            if (TxtInput != null)
            {
                TxtInput.Text = text;
                if (!string.IsNullOrEmpty(text))
                {
                    TxtInput.CaretIndex = text.Length;
                }
            }
            return text;
        }

        private void OnDisplayedValueBindingChanged()
        {
            if (ListViewResult != null)
            {
                this.SetBinding(Intellibox.DisplayTextFromHighlightedItemProperty,
                    BindingBaseFactory.ConstructBindingForHighlighted(this, DisplayedValueBinding));
            }

            this.SetBinding(Intellibox.DisplayTextFromSelectedItemProperty,
                BindingBaseFactory.ConstructBindingForSelected(this, DisplayedValueBinding));
        }

        private void CancelSelection()
        {
            _lastTextValue = UpdateSearchBoxText(true);

            OnUserEndedSearchEvent();

            if (Items != null)
            {
                Items = null;
            }
        }

        private void OnShowAllResults()
        {
            TxtInput.Focus();
            PerformSearchActions(TxtInput.Text, true);
        }

        private void OnUserEndedSearchEvent()
        {
            if (SearchTimer != null)
            {
                SearchTimer.Stop();
                //setting to null so that when a new search starts, we grab fresh values for the time interval
                SearchTimer = null;
            }

            if (WaitNotificationTimer != null)
            {
                WaitNotificationTimer.Stop();
                //setting to null so that when a new search starts, we grab fresh values for the time interval
                WaitNotificationTimer = null;
            }

            if (SearchProvider != null)
            {
                SearchProvider.CancelAllSearches();
            }

            ShowResults = false;
            PopNoResult.IsOpen = false;
            PopWait.IsOpen = false;
        }

        private void PerformSearchActions(string enteredText, bool showAll = false)
        {
            enteredText = ApplyDisableWhitespaceTrim(enteredText);

            //if (enteredText.Equals(_lastTextValue) && !showAll)
            //    return;

            if (string.IsNullOrEmpty(enteredText) && !showAll)
            {
                this.SelectedItem = null;
                OnUserEndedSearchEvent();
            }
            else
            {
                bool doSearchNow = !IsSearchInProgress && enteredText.Length >= MinimumPrefixLength;
                if (doSearchNow || showAll)
                {
                    SearchTimer = new DispatcherTimer(
                        TimeSpan.FromMilliseconds(MinimumSearchDelay),
                        DispatcherPriority.Background,
                        new EventHandler(OnSearchTimerTick),
                        this.Dispatcher);

                    CreateSearch(enteredText);
                    SearchTimer.Start();
                }
            }
        }

        private string ApplyDisableWhitespaceTrim(string input)
        {
            // if the entered text isn't supposed to be trimmed, then use it as-is
            // otherwise Trim() it if it's not null, or set to string.Empty if it is null
            return DisableWhitespaceTrim
                    ? input
                    : (string.IsNullOrEmpty(input) ? string.Empty : input.Trim());
        }

        private void CreateSearch(string current)
        {
            _lastTextValue = current;
            OnSearchBeginning(current, MaxResults, Tag);
            if (SearchProvider == null)
            {
                throw new Exception("SearchProvider is empty");
            }
            SearchProvider.BeginSearchAsync(current, DateTime.Now.ToUniversalTime(), MaxResults, Tag, ProcessSearchResults);
        }

        private void OnSearchTimerTick(object sender, EventArgs e)
        {
            if (IsSearchInProgress)
            {
                var last = ApplyDisableWhitespaceTrim(_lastTextValue);
                var current = ApplyDisableWhitespaceTrim(TxtInput.Text);

                // we don't want to search for an empty string, but unlike the first search, we don't want
                // empty search strings to cancel existing searches, because that responsibility
                // belongs to the the code that kicks off the first search
                bool startAnotherSearch = !last.Equals(current) && !string.IsNullOrEmpty(current);
                if (startAnotherSearch)
                {
                    CreateSearch(current);
                }
            }
        }

        private void OnSearchBeginning(string term, int max, object data)
        {
            // we don't want to re-start the timer if it's already been started
            // or if results are showing
            if (WaitNotificationTimer == null && !ShowResults)
            {
                WaitNotificationTimer = new DispatcherTimer(
                            TimeSpan.FromMilliseconds(TimeBeforeWaitNotification),
                            DispatcherPriority.Background,
                            new EventHandler(OnWaitNotificationTimerTick),
                            this.Dispatcher);

                WaitNotificationTimer.Start();
            }

            SearchBeginning?.Invoke(term, max, data);
        }

        private void OnWaitNotificationTimerTick(object sender, EventArgs args)
        {
            if (WaitNotificationTimer != null)
            {
                WaitNotificationTimer.Stop();
            }

            // this timer only needs to fire once
            WaitNotificationTimer = null;

            //determine if we have any active searches
            var activeSearches = false;
            if (SearchProvider != null && SearchProvider.HasActiveSearches)
            {
                activeSearches = true;
            }

            PopWait.IsOpen = IsSearchInProgress && !ShowResults && activeSearches;
        }

        private bool IsBaseType(object item)
        {
            var type = item.GetType();
            return _baseTypes.Any(i => i == type);
        }

        private Style ZeroHeightColumnHeader
        {
            get
            {
                var noHeader = new Style(typeof(GridViewColumnHeader));
                noHeader.Setters.Add(new Setter(GridViewColumnHeader.HeightProperty, 0.0));
                return noHeader;
            }
        }

        private GridView ConstructGridView(object item)
        {
            var view = new GridView();

            bool isBaseType = IsBaseType(item);

            if (isBaseType || HideColumnHeaders)
            {
                view.ColumnHeaderContainerStyle = ZeroHeightColumnHeader;
            }

            if (isBaseType)
            {
                var gvc = new GridViewColumn();
                gvc.Header = item.GetType().Name;

                gvc.DisplayMemberBinding = new Binding();
                view.Columns.Add(gvc);

                return view;
            }

            if (ExplicitlyIncludeColumns && Columns != null && Columns.Count > 0)
            {
                foreach (var col in Columns.Where(c => !c.Hide).OrderBy(c => c.Position ?? int.MaxValue))
                {
                    view.Columns.Add(CloneHelper.Clone(col));
                }
                return view;
            }

            var typeProperties = (from p in item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                  where p.CanRead && p.CanWrite
                                  select new
                                  {
                                      Name = p.Name,
                                      Column = Columns.FirstOrDefault(c => p.Name.Equals(c.ForProperty))
                                  }).ToList();

            //This is a shortcut to sort the nulls to the back of the list instead of the front
            //we did this instead of creating an IComparer.
            var typesWithPositions = typeProperties
                .Where(a => a.Column != null && a.Column.Position != null).OrderBy(a => a.Column.Position);

            var typesWithoutPositions = typeProperties.Except(typesWithPositions);

            var sortedProperties = typesWithPositions.Concat(typesWithoutPositions);

            foreach (var currentProperty in sortedProperties)
            {
                if (currentProperty.Column != null)
                {
                    if (!currentProperty.Column.Hide)
                    {
                        var gvc = CloneHelper.Clone(currentProperty.Column);

                        if (gvc.Header == null)
                        { // TODO check if this is bound to anything
                            gvc.Header = currentProperty.Name;
                        }

                        if (gvc.DisplayMemberBinding == null)
                        {
                            gvc.DisplayMemberBinding = new Binding(currentProperty.Name);
                        }

                        view.Columns.Add(gvc);
                    }
                }
                else
                {
                    var gvc = new GridViewColumn();
                    gvc.Header = currentProperty.Name;

                    gvc.DisplayMemberBinding = new Binding(currentProperty.Name);
                    view.Columns.Add(gvc);
                }
            }

            return view;
        }
        private void ProcessSearchResults(DateTime startTimeUtc, IEnumerable results)
        {
            if (_lastTimeSearchRecievedUtc > startTimeUtc)
            {
                return; // this result set isn't fresh, so don't bother processing it
            }

            _lastTimeSearchRecievedUtc = startTimeUtc;

            ShowResults = false;
            PopWait.IsOpen = false;

            Items = results == null
                ? new List<string>()
                : ((results is IList)
                    ? (IList)results //optimization to keep from making a copy of the list
                    : results.Cast<object>().ToList());

            PopNoResult.IsOpen = Items.Count < 1;

            if (Items.Count > 0)
            {
                ListViewResult.View = GridView ?? ConstructGridView(Items[0]);

                ListViewResult.SelectedIndex = 0;
                ShowResults = true;
            }

            if (AutoSelectSingleResult && Items.Count == 1)
            {
                ListViewResult.SelectedItem = Items[0];
                ChooseCurrentItem();
                ShowResults = false;
            }

            OnSearchCompleted();
        }

        private void OnSearchCompleted()
        {
            SearchCompleted?.Invoke();
        }

        private void ChooseCurrentItem()
        {
            _lastTextValue = UpdateSearchBoxText(true);

            OnUserEndedSearchEvent();

            this.SetValue(SelectedItemProperty, ListViewResult.SelectedItem);

            if (Items != null)
            {
                Items = null;
            }

        }
        #endregion

    }
}
