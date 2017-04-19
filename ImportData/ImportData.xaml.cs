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
using ImportData.Helpers;
using System.IO;
using Microsoft.Windows.Controls;
using System.Diagnostics;

namespace ImportData
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        #region Data
        private TableInfo _TableInfo;
        private IList<FieldInfo> _FieldInfos;
        private SheetInfo _SheetInfo;
        private string _ExcelFile;
        private IList<CustomEntity> _CustomEntities; 
        #endregion

        public Window1()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Window1_Loaded);
        }

        private ObjectDataProvider MyObjectDataProvider
        {
            get
            {
                return this.TryFindResource("ExcelColumns") as ObjectDataProvider;
            }
        }

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
            InitData();
        }

        private void InitData()
        {
            ComboBox_Table.ItemsSource = TableInfo.GetTableInfos();
        }

        private void Button_Browse_Click(object sender, RoutedEventArgs e)
        {
            DoBrowse();
        }

        private void DoBrowse()
        {
            System.Windows.Forms.OpenFileDialog openFile = new System.Windows.Forms.OpenFileDialog();
            openFile.Filter = @"Excel Files|*.xls;*.xlsx;*.xlsm";
            openFile.RestoreDirectory = true;
            openFile.ShowDialog();

            _ExcelFile = openFile.FileName;
            TextBox_File.Text = _ExcelFile;
            this.ComboBox_Sheet.ItemsSource = ExcelReaderWriterHelper.GetSheetInfos(_ExcelFile, true);
        }

        private void ComboBox_Table_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid_Fields.ItemsSource = null;
            if (ComboBox_Table.SelectedIndex >= 0)
            {
                this._TableInfo = (TableInfo)ComboBox_Table.SelectedItem;
                this._FieldInfos = this._TableInfo.GetFieldInfos();
                DataGrid_Fields.ItemsSource = this._FieldInfos;
            }
        }

        private void ComboBox_Sheet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ComboBox_Sheet.SelectedIndex > -1)
            {
                this._SheetInfo = (SheetInfo)this.ComboBox_Sheet.SelectedItem;
                ExcelReaderWriterHelper.SetColumnInfos(_ExcelFile, this._SheetInfo);

                ColumnInfo cInfoEmpty = new ColumnInfo() 
                { 
                    Index = 0,
                    Header = string.Empty,
                    Name = string.Empty,
                };
                this._SheetInfo.Columns.Insert(0, cInfoEmpty);
               
                DataGridComboBoxColumn_Columns.ItemsSource = null;
                DataGridComboBoxColumn_Columns.ItemsSource = this._SheetInfo.Columns;
                
                StackPanel_SheetInfo.DataContext = this._SheetInfo;
            }
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFile = new System.Windows.Forms.OpenFileDialog();
            openFile.Filter = @"XML Files|*.xml";
            openFile.RestoreDirectory = true;
            openFile.CheckFileExists = false;
            openFile.ShowDialog();

            if (string.IsNullOrEmpty(openFile.FileName))
                return;

            MappingFile mFile = new MappingFile();
            mFile.TableName = this._TableInfo.Name;
            mFile.Fields.AddRange(this._FieldInfos);
            File.WriteAllText(openFile.FileName, SerializeHelper.XmlSerializeObject<MappingFile>(mFile));
        }

        private void Button_Load_Click(object sender, RoutedEventArgs e)
        {
            #region Check if Table and Sheet has been selected first.
            if (this._TableInfo == null)
            {
                MessageBox.Show("Please select a Table from the dropdownlist.");
                return;
            }

            if (this._SheetInfo == null)
            {
                MessageBox.Show("Please select a sheet in an excel file.");
                return;
            }
            #endregion

            #region Select the mapping file and read it

            MappingFile mFileData = null;
            System.Windows.Forms.OpenFileDialog openFile = new System.Windows.Forms.OpenFileDialog();
            openFile.Filter = @"XML Files|*.xml";
            openFile.RestoreDirectory = true;
            openFile.ShowDialog();

            if (File.Exists(openFile.FileName))
            {
                try
                {
                    string xml = File.ReadAllText(openFile.FileName);
                    mFileData = SerializeHelper.XmlDeserializeObject<MappingFile>(xml);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("DEBUG: " + ex.Message);
                    mFileData = null;
                }
            }
            else
            {
                mFileData = null;
            }

            #endregion

            #region Rebuild the list of FieldInfos with the data in the mapping file

            if (mFileData != null)
            {
                if (String.Compare(this._TableInfo.Name, mFileData.TableName, true) != 0)
                {
                    MessageBox.Show("The mapping file is configured for Table: " + mFileData.TableName + ", not for the selected one: " + this._TableInfo.Name);
                }
                else
                {
                    this._FieldInfos = this._TableInfo.GetFieldInfos();

                    foreach (FieldInfo fieldInfo in _FieldInfos)
                    {
                        FieldInfo mappingField = mFileData.GetFieldInfo(fieldInfo.Name);
                        if (mappingField != null)
                        {
                            fieldInfo.DefaultValue = mappingField.DefaultValue;
                            fieldInfo.ExcelColumnIndex = mappingField.ExcelColumnIndex.HasValue ? mappingField.ExcelColumnIndex.Value > 0 ? mappingField.ExcelColumnIndex : null : null;
                            fieldInfo.FunctionIDs = mappingField.FunctionIDs;
                            fieldInfo.FunctionArgs = mappingField.FunctionArgs;
                            fieldInfo.IsUnique = mappingField.IsUnique;
                        }
                    }
                }
            }

            #endregion

            #region Refresh the DataGrid_Fields

            DataGrid_Fields.ItemsSource = this._FieldInfos;

            #endregion
        }

        private void TextBox_File_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DoBrowse();
        }

        private void GenerateColumns(DataGrid dataGrid, SheetInfo sheetInfo)
        {
            dataGrid.Columns.Clear();
            int i = 0;

            #region Dynamic Columns

            foreach (var m_Column in sheetInfo.Columns)
            {
                if (m_Column.Index > 0)
                {
                    string m_FieldName = CustomEntity.GetPropertyName(i);
                    string m_BackgroundFieldName = CustomEntity.GetBackgroundFieldName(i);

                    //Use FColumn -> CellTemplate
                    DataGridTemplateColumn m_TemplateColumn = new DataGridTemplateColumn()
                    {
                        Header = m_Column.Header,
                        IsReadOnly = false,
                    };
                    m_TemplateColumn.CellTemplate = GetDataTemplate(m_FieldName, m_BackgroundFieldName);

                    dataGrid.Columns.Add(m_TemplateColumn);
                    i++;
                }
            }

            #endregion
        }

        private DataTemplate GetDataTemplate(string m_FieldName, string m_BackgroundField)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(TextBlock));
            template.VisualTree = factory;
            factory.SetBinding(TextBlock.TextProperty, new Binding(m_FieldName));
            factory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            factory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Stretch);

            //Background color
            //Binding bdColor = new Binding("DataContext." + m_BackgroundField)
            //{
            //    RelativeSource = new RelativeSource() { Mode = RelativeSourceMode.FindAncestor, AncestorType = typeof(DataRow) }
            //};
            //factory.SetBinding(TextBlock.BackgroundProperty, bdColor);
            return template;
        }

        private void Button_Read_Click(object sender, RoutedEventArgs e)
        {
            GenerateColumns(DataGrid_Columns, this._SheetInfo);
            _CustomEntities = ReadData(this._ExcelFile, this._SheetInfo);

            PagingNavigator_Step3.SetupPaging(DataGrid_Columns, _CustomEntities, 20);
        }

        private IList<CustomEntity> ReadData(string fileName, SheetInfo sheetInfo)
        {
            IList<CustomEntity> m_CustomEntities = new List<CustomEntity>();
            ExcelReaderWriter m_ExcelReaderWriter = new ExcelReaderWriter(fileName, sheetInfo.Index);
            try
            {
                m_ExcelReaderWriter.OpenWorkbook();
                object[,] m_Values = (object[,])m_ExcelReaderWriter.ActiveWorksheet.UsedRange.Value2;
                if (m_Values != null)
                {
                    if (sheetInfo.BeginRowIndex == 0) sheetInfo.BeginRowIndex = 0;
                    if (sheetInfo.EndRowIndex == 0) sheetInfo.EndRowIndex = (int)m_Values.GetLongLength(0);

                    if (m_Values != null)
                    {
                        for (int m_RowIndex = sheetInfo.BeginRowIndex; m_RowIndex <= sheetInfo.EndRowIndex; m_RowIndex++)
                        {
                            CustomEntity m_CustomEntity = new CustomEntity();

                            //Dynamic value excel
                            foreach (var m_ColumnInfo in sheetInfo.Columns)
                            {
                                if (m_ColumnInfo.Index != 0)
                                {
                                    //Range a = (Range)m_ExcelReaderWriter.ActiveWorksheet.UsedRange[m_RowIndex, m_ColumnInfo.Index];
                                    //var x = a.Text;
                                    var x = m_Values[m_RowIndex, m_ColumnInfo.Index];
                                    string m_ValueString = string.Format("{0}", x);// m_Values[m_RowIndex, m_ColumnInfo.Index]);
                                    m_CustomEntity.Properties.Add(m_ValueString);
                                }
                            }

                            m_CustomEntities.Add(m_CustomEntity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                m_ExcelReaderWriter.CloseWorkbook();
            }

            return m_CustomEntities;
        }

        private void Button_Preview_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBox_Table.SelectedIndex < 0) return;

            List<FieldInfo> Fields = DataGrid_Fields.ItemsSource.Cast<FieldInfo>().ToList();
            if (Fields == null) return;
            else
            {
                MessageBoxResult ret = MessageBox.Show("Do you want to check for not null fields?", "Field checking", MessageBoxButton.YesNoCancel);
                switch (ret)
                {
                    case MessageBoxResult.Yes:
                        foreach (FieldInfo field in Fields)
                        {
                            if (// Is not allow null field
                                !field.IsNullable
                                // Is not IndentityColumn
                                &&!field.IsIdentity
                                // Is not mapped to any excel column
                                && (!field.ExcelColumnIndex.HasValue || field.ExcelColumnIndex.HasValue && field.ExcelColumnIndex.Value <= 0) 
                                // Has no Default Value
                                && string.IsNullOrEmpty(field.DefaultValue)
                                // Has no Function
                                && string.IsNullOrEmpty(field.FunctionIDs))
                            {
                                MessageBox.Show("Not nullable column must be configured with an Excel column index or a default value!\n\n" + field.Name);
                                return;
                            }
                        }
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                    case MessageBoxResult.No:
                    default:
                        break;
                }
            }

            if (this._CustomEntities == null || this._CustomEntities.Count == 0) return;

            PreviewAndImport view = new PreviewAndImport();
            var uniqueField = Fields.Where(f => f.IsUnique).FirstOrDefault();
            string sUniqueField = uniqueField != null ? uniqueField.Name : string.Empty;
            view.SetParamaters(this._TableInfo, this._CustomEntities, this._FieldInfos, sUniqueField);
            view.WindowState = WindowState.Maximized;
            view.Show();
        }

        private void DataGrid_Fields_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

        }

        private void DataGrid_Fields_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {

        }

        private void DataGrid_Fields_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

        }

        private void ComboBox_Columns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
