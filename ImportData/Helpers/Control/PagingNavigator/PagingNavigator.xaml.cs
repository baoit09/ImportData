using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Windows.Controls;

namespace ImportData.Helpers.Control.PagingNavigator
{
    /// <summary>
    /// Interaction logic for PagingNavigator.xaml
    /// </summary>
    //public partial class PagingNavigator<T> : UserControl where T : class, new() 
    public partial class PagingNavigator : UserControl 
    {
        public static int PageSize = 50;

        #region Private Data
        private int _PageSize;
        private int _CurrentPage;
        private int _PageCount;
        private int _RecordCount;

        private DataGrid _DataGrid;
        private IEnumerable<CustomEntity> _ItemsSource;
        #endregion

        public PagingNavigator()
        {
            InitializeComponent();
            _PageSize = PagingNavigator.PageSize;
            _PageCount = 0;
            this.Loaded += new RoutedEventHandler(PagingNavigator_Loaded);
        }

        void PagingNavigator_Loaded(object sender, RoutedEventArgs e)
        {
            InitCommandBindings();
            InitEvents();
        }

        private void InitEvents()
        {
            ToggleButton_Expaned.Click += new RoutedEventHandler(ToggleButton_Expaned_Click);
        }

        void ToggleButton_Expaned_Click(object sender, RoutedEventArgs e)
        {
            if (StackPanel_ItemsPerPage.Visibility == Visibility.Visible)
            {
                StackPanel_ItemsPerPage.Visibility = Visibility.Collapsed;
            }
            else
            {
                StackPanel_ItemsPerPage.Visibility = Visibility.Visible;
            }
        }

        private void InitCommandBindings()
        {
            buttonRefresh.Command = new RoutedCommand();
            CommandBinding cbRefresh = new CommandBinding(buttonRefresh.Command, RefreshExecute, RefreshCanExecute);
            this.CommandBindings.Add(cbRefresh);

            buttonFirst.Command = new RoutedCommand();
            CommandBinding cbFirst = new CommandBinding(buttonFirst.Command, FirstExecute, FirstCanExecute);
            this.CommandBindings.Add(cbFirst);

            buttonPrevious.Command = new RoutedCommand();
            CommandBinding cbPrevious = new CommandBinding(buttonPrevious.Command, PreviousExecute, PreviousCanExecute);
            this.CommandBindings.Add(cbPrevious);

            buttonNext.Command = new RoutedCommand();
            CommandBinding cbNext = new CommandBinding(buttonNext.Command, NextExecute, NextCanExecute);
            this.CommandBindings.Add(cbNext);

            buttonLast.Command = new RoutedCommand();
            CommandBinding cbLast = new CommandBinding(buttonLast.Command, LastExecute, LastCanExecute);
            this.CommandBindings.Add(cbLast);

            buttonGo.Command = new RoutedCommand();
            CommandBinding cbGo = new CommandBinding(buttonGo.Command, GoExecute, GoCanExecute);
            this.CommandBindings.Add(cbGo);
        }

        #region Refresh
        private void RefreshCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void RefreshExecute(object sender, ExecutedRoutedEventArgs e)
        {
            RefreshPaging();
        }
        #endregion

        #region First
        private void FirstCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = 0 < _CurrentPage;
            e.Handled = true;
        }

        private void FirstExecute(object sender, ExecutedRoutedEventArgs e)
        {
            _CurrentPage = 0;
            RefreshPaging();
        }
        #endregion

        #region Previous
        private void PreviousCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = 0 < _CurrentPage;
            e.Handled = true;
        }

        private void PreviousExecute(object sender, ExecutedRoutedEventArgs e)
        {
            _CurrentPage--;
            RefreshPaging();
        }
        #endregion

        #region Next
        private void NextCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _CurrentPage < _PageCount;
        }

        private void NextExecute(object sender, ExecutedRoutedEventArgs e)
        {
            _CurrentPage++;
            RefreshPaging();
        }
        #endregion

        #region Last
        private void LastCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _CurrentPage < _PageCount;
        }

        private void LastExecute(object sender, ExecutedRoutedEventArgs e)
        {
            _CurrentPage = _PageCount;
            RefreshPaging();
        }
        #endregion

        #region Go

        private void textBoxCurrentPage_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int? iPageToGo = GetPageToGo();
                if (iPageToGo.HasValue && 0 <= (iPageToGo - 1) && (iPageToGo - 1) <= _PageCount)
                {
                    _CurrentPage = iPageToGo.Value - 1;
                    RefreshPaging();
                }
            }
        }

        private void GoCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            int? iPageToGo = GetPageToGo();
            bool bCanGo = iPageToGo.HasValue && 0 <= (iPageToGo - 1) && (iPageToGo - 1) <= _PageCount;
            e.CanExecute = bCanGo;
        }

        private int? GetPageToGo()
        {
            int iPageToGo;
            if (int.TryParse(textBoxCurrentPage.Text, out iPageToGo))
            {
                return iPageToGo;
            }
            return null;
        }

        private void GoExecute(object sender, ExecutedRoutedEventArgs e)
        {
            int iPageToGo;
            if (int.TryParse(textBoxCurrentPage.Text, out iPageToGo))
            {
                _CurrentPage = iPageToGo - 1;
            }

            RefreshPaging();
        }
        #endregion

        //public void SetupPaging<T>(DataGrid ListView, IEnumerable<T> ItemsSource)
        //{
        //    SetupPaging(ListView, ItemsSource, PagingNavigator.PageSize);
        //}

        public void SetupPaging(DataGrid ListView, IEnumerable<CustomEntity> ItemsSource, int PageSize)
        {
            _DataGrid = ListView;
            _ItemsSource = ItemsSource;
            _PageSize = PageSize;
            ComboBox_ItemsPerPage.Text = _PageSize.ToString();

            _RecordCount = _ItemsSource.Count();
            TextBlock_Total_Items.Text = string.Format("Total {0} record(s)", _RecordCount);

            _PageCount = _RecordCount / _PageSize;
            labelPageCount.Text = _PageCount.ToString();

            RefreshPaging();
        }

        private void RefreshPaging()
        {
            textBoxCurrentPage.Text = (_CurrentPage + 1).ToString();
            _DataGrid.ItemsSource = _ItemsSource.Skip(_PageSize * _CurrentPage).Take(_PageSize);
        }

        private void ComboBox_ItemsPerPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_ItemsPerPage.SelectedValue != null)
            {
                int iPageSize;
                string sSelectedText = ((ComboBoxItem)ComboBox_ItemsPerPage.SelectedValue).Content.ToString();
                if (int.TryParse(sSelectedText, out iPageSize))
                {
                    UpdatePageSize(iPageSize);
                }
            }
        }

        private void UpdatePageSize(int iPageSize)
        {
            _PageSize = iPageSize;

            _PageCount = _RecordCount / _PageSize;
            labelPageCount.Text = _PageCount.ToString();

            _CurrentPage = 0;
            RefreshPaging();
        }
    }
}
