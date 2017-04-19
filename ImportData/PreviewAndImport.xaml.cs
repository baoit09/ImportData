using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ImportData.Helpers;
using Microsoft.Windows.Controls;

namespace ImportData
{
    /// <summary>
    /// Interaction logic for PreviewAndImport.xaml
    /// </summary>
    public partial class PreviewAndImport : Window
    {
        #region Private Data
        private IList<FieldInfo> _Allfields = null;
        private IList<FieldInfo> _AllFields
        {
            get
            {
                return _Allfields;
            }
            set
            {
                _Allfields = value;
            }
        }

        private IList<FieldInfo> _Selectedfields = null;
        private IList<FieldInfo> _SelectedFields
        {
            get
            {
                return _Selectedfields;
            }
            set
            {
                _Selectedfields = value;
            }
        }

        private TableInfo _TableInfo { get; set; }
        private IList<CustomEntity> _CustomEntities { get; set; }
        private IList<CustomEntity> _PreviewEntities { get; set; }
        private string _NameOfUniqueField { get; set; }
        private string _Mark { get; set; }
        //private EntityCollectionNonGeneric SavedEntities { get; set; }
        private DataProcessingFunction _DPFunction { get; set; }

        /// <summary>
        /// The lisf of column names need to selected
        /// </summary>
        private string _ColumnNames { get; set; }
        #endregion

        public PreviewAndImport()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(PreviewAndImport_Loaded);
        }

        void PreviewAndImport_Loaded(object sender, RoutedEventArgs e)
        {
            BuildSelectedFields();
            BuildPreviewData();
            ValidatePreviewData();

            DataGrid_Entities.Visibility = Visibility.Visible;
            RadioButton_ShowAll.IsChecked = true;
        }

        public void SetParamaters(TableInfo tableInfo, IList<CustomEntity> customEntities, IList<FieldInfo> fields, string nameOfUniqueField)
        {
            _TableInfo = tableInfo;
            _CustomEntities = customEntities;
            _AllFields = fields;
            _NameOfUniqueField = nameOfUniqueField;

            //SavedEntities = new EntityCollectionNonGeneric(EntityFactoryFactory.GetFactory(GeneralEntityFactory.Create(TypeOfEntity).GetType()));
            _DPFunction = new DataProcessingFunction();
        }

        private DataTemplate BuildDataTemplate(int index, FieldInfo fieldInfo)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory textBlock = new FrameworkElementFactory(typeof(TextBlock));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(2.0));
            border.AppendChild(textBlock);
            template.VisualTree = border;

            string sValuePath = CustomEntity.GetPropertyName(index);
            if (FieldInfo.HasFunctionField(fieldInfo))
            {
                sValuePath = CustomEntity.GetFuncValueName(index);
            }
            textBlock.SetBinding(TextBlock.TextProperty, new Binding(sValuePath));
            textBlock.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            textBlock.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Stretch);

            //Background color
            Binding bdColor = new Binding(CustomEntity.GetPropertyPath_ErrorBackground(index))
            {
                //RelativeSource = new RelativeSource() { Mode = RelativeSourceMode.FindAncestor, AncestorType = typeof(DataRow) },
                Mode = BindingMode.OneWay
            };
           
            border.SetBinding(Border.BorderBrushProperty, bdColor);

            //Tooltip
            Binding bdTooltip = new Binding(CustomEntity.GetPropertyPath_ErrorTooltip(index))
            {
                //RelativeSource = new RelativeSource() { Mode = RelativeSourceMode.FindAncestor, AncestorType = typeof(DataRow) },
                Mode = BindingMode.OneWay
            };
            textBlock.SetBinding(TextBlock.ToolTipProperty, bdTooltip);

            return template;
        }

        public void BuildPreviewData()
        {
            if (this._AllFields == null) return;
            if (this._CustomEntities == null || this._CustomEntities.Count == 0) return;

            #region Generate Columns that have been mapped to any excel column or have Function or DefaulValue
            //Add column GetPropertyPath_Error
            Binding binding = new Binding(CustomEntity.GetPropertyPath_Error);
            binding.Mode = BindingMode.OneWay;
            DataGridCheckBoxColumn clmError = new DataGridCheckBoxColumn()
            {
                Header = "Has Error",
                Binding = binding
            };
            DataGrid_Entities.Columns.Add(clmError);

            Binding binding2 = new Binding(CustomEntity.GetPropertyPath_Warning);
            binding2.Mode = BindingMode.OneWay;
            DataGridCheckBoxColumn clmError2 = new DataGridCheckBoxColumn()
            {
                Header = "Has Warning",
                Binding = binding2
            };
            DataGrid_Entities.Columns.Add(clmError2);

            for (int i = 0; i < this._SelectedFields.Count; i++)
            {
                FieldInfo field = this._SelectedFields[i];
                DataGridTemplateColumn m_TextBoxColumn = new DataGridTemplateColumn();
                m_TextBoxColumn.Header = field.Name;

                m_TextBoxColumn.CellTemplate = BuildDataTemplate(i, field);

                DataGrid_Entities.Columns.Add(m_TextBoxColumn);
            }
            #endregion

            #region Generate Data
            this._PreviewEntities = new List<CustomEntity>();
            //Get the remaining rows into the displaying grid
            foreach (CustomEntity entity in this._CustomEntities)
            {
                //Data
                CustomEntity addedEntity = new CustomEntity();
                for (int i = 0; i < this._SelectedFields.Count; i++)
                {
                    FieldInfo field = this._SelectedFields[i];
                    // If This field is mapped to Excel Column
                    if (field.ExcelColumnIndex.HasValue)
                    {
                        string sValue = entity.Properties[field.ExcelColumnIndex.Value - 1].ToString();
                        if (sValue != null)
                        {
                            if (sValue == string.Empty && field.ReplaceEmptyByDefaultValue)
                            {
                                sValue = field.DefaultValue;
                            }

                            if (string.IsNullOrEmpty(sValue) && field.ReplaceEmptyByDBDefaultValue)
                            {
                                sValue = field.GetDBDefaulValueOrBinding();
                            }

                            addedEntity.Properties.Add(sValue);
                        }
                        else // If there is no data in excel file (sValue == NULL)
                        {
                            // then use Default Value
                            if (!string.IsNullOrEmpty(field.DefaultValue))
                            {
                                addedEntity.Properties.Add(field.DefaultValue);
                            }
                            // or use DBDefaultValue
                            else if (!string.IsNullOrEmpty(field.DBDefaulValueOrBinding))
                            {
                                addedEntity.Properties.Add(field.GetDBDefaulValueOrBinding());
                            }
                        }
                    }
                    // Else If this field has DefaultValue
                    else if (field.DefaultValue != null)
                    {
                        addedEntity.Properties.Add(field.DefaultValue);
                    }
                    else if (FieldInfo.HasFunctionField(field))
                    {
                        addedEntity.Properties.Add(string.Empty);
                    }

                    if (field.IsUnique)
                    {
                        addedEntity.UniqueColumnIndex = i;
                    }

                    // If this field has function
                    if (FieldInfo.HasFunctionField(field))
                    {
                        #region Cal for the function
                        string sValue = (string)addedEntity.Properties[i];
                        // Preprocessing first, using function to convert all data first.
                        if (field.FunctionIDs.Contains('1'))
                        {
                            //Try to load data into the Cache
                            _DPFunction.fLookupPreload(field.FunctionArgs);
                        }

                        sValue = (string)_DPFunction.Process(field.FunctionIDs, field.FunctionArgs, sValue);
                        addedEntity.FuncValues[i] = sValue;
                        #endregion
                    }
                }

                this._PreviewEntities.Add(addedEntity);
            }
            #endregion
        }

        public void ValidatePreviewData()
        {
            if (this._PreviewEntities == null || this._PreviewEntities.Count == 0)
                return;

            foreach (CustomEntity previewEntity in _PreviewEntities)
            {
                for (int i = 0; i < this._Selectedfields.Count; i++)
                {
                    FieldInfo fieldInfo = _Selectedfields[i];
                    Type type = TypeHelper.GetType(fieldInfo, this._TableInfo);
                    string sValue = (string)previewEntity.Properties[i];
                    if (FieldInfo.HasFunctionField(fieldInfo))
                    {
                        sValue = (string)previewEntity.FuncValues[i];
                    }

                    #region Not allow Null columns
                    if (!fieldInfo.IsNullable && !fieldInfo.IsIdentity && sValue == null)
                    {
                        CustomEntityError error = new CustomEntityError()
                        {
                            ErrorTypeEnum = ErrorTypeEnum.Error,
                            ColorDisplayWhenError = Brushes.Red,
                            Tooltip = string.Format("The column [{0}] must have value", fieldInfo.Name)
                        };
                        previewEntity.SetError(i, error);
                        continue;
                    }
                    #endregion

                    #region Check for Converting
                    if (type != null && !string.IsNullOrEmpty(sValue))
                    {
                        object objOut = null;
                        bool bSuccess = TypeHelper.GetValue(sValue, type, fieldInfo, out objOut);
                        if (!bSuccess)
                        {
                            CustomEntityError error = new CustomEntityError()
                            {
                                ErrorTypeEnum = fieldInfo.IsNullable == false ? ErrorTypeEnum.Error : ErrorTypeEnum.Warning,
                                ColorDisplayWhenError = fieldInfo.IsNullable == false ? Brushes.Red : Brushes.Yellow,
                                Tooltip = string.Format("Can not convert [{0}] to type {1}", sValue, type.Name)
                            };
                            previewEntity.SetError(i, error);
                        }
                    }
                    #endregion

                    #region Check string length
                    if (type != null && type == typeof(string) && !string.IsNullOrEmpty(sValue) && fieldInfo.MaxLength != "MAX")
                    {
                        object nMaxLength = 0;
                        TypeHelper.GetValue(fieldInfo.MaxLength, typeof(int), fieldInfo, out nMaxLength);
                        if (sValue.Length > (int)nMaxLength)
                        {
                            CustomEntityError error = new CustomEntityError()
                            {
                                ColorDisplayWhenError = Brushes.Yellow,
                                Tooltip = string.Format("Warning : String value length [{0}] > MaxLength [{1}]", sValue.Length, fieldInfo.MaxLength)
                            };
                            previewEntity.SetError(i, error);
                        }
                    }
                    #endregion
                }
            }
        }

        public void BuildSelectedFields()
        {
            this._SelectedFields = this._AllFields.Where(FieldInfo.IsSelectedField).ToList();
        }

        private void Button_Import_Click(object sender, RoutedEventArgs e)
        {
            DoImport();
        }

        private void DoImport()
        {
            //Waiting waiting = new Waiting();
            //waiting.ShowDialog();

            if (this._PreviewEntities == null || this._PreviewEntities.Count == 0)
            {
                return;
            }

            bool bSuccess = false;
            string sMessage = string.Empty;
            int nCountEntitiesAdded = 0, nCountEntitiesUpdated = 0;
            bool bIsUpdateCase = this._PreviewEntities[0].UniqueColumnIndex.HasValue;

            using (SqlConnection connection = new SqlConnection(DataBaseInfo.ConnectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    int nPageSize = 100, nCurrentPageIndex = 0;
                    while (true)
                    {
                        IList<CustomEntity> CustomEntities = this._PreviewEntities.Skip(nCurrentPageIndex * nPageSize).Take(nPageSize).ToList();
                        if (CustomEntities != null && CustomEntities.Count == 0)
                        {
                            break;
                        }

                        #region For each page
                        _ColumnNames = GetColumns();
                        string SQLSelectCommand = string.Empty;
                        if (bIsUpdateCase)
                        {
                            string sValuesOfUniqueColumn = GetValues(CustomEntities);
                            SQLSelectCommand = string.Format("SELECT {0} FROM {1} WHERE {2} IN ({3})",
                                this._ColumnNames, this._TableInfo.Name, _NameOfUniqueField, sValuesOfUniqueColumn);
                        }
                        else
                        {
                            SQLSelectCommand = string.Format("SELECT {0} FROM {1} WHERE 1 = 2",
                                this._ColumnNames, this._TableInfo.Name);
                        }

                        SqlDataAdapter da = new SqlDataAdapter();
                        da.SelectCommand = new SqlCommand(SQLSelectCommand, connection);
                        da.SelectCommand.Transaction = transaction;

                        SqlCommandBuilder cb = new SqlCommandBuilder(da);
                        da.UpdateCommand = cb.GetUpdateCommand();
                        da.UpdateCommand.Transaction = transaction;
                        da.InsertCommand = cb.GetInsertCommand();
                        da.InsertCommand.Transaction = transaction;

                        DataSet ds = new DataSet();
                        da.Fill(ds, this._TableInfo.Name);

                        DataTable dt = ds.Tables[this._TableInfo.Name];
                        foreach (CustomEntity customEntity in CustomEntities)
                        {
                            bool bAddNewRow = false;
                            DataRow dr = FindDataRow(dt, customEntity);
                            if (dr == null)
                            {
                                nCountEntitiesAdded++;
                                bAddNewRow = true;
                                dr = dt.NewRow();
                            }
                            else
                            {
                                nCountEntitiesUpdated++;
                            }

                            BuildDataRow(dt, dr, customEntity);

                            if (bAddNewRow)
                            {
                                dt.Rows.Add(dr);
                            }
                        }

                        if (ds.HasChanges())
                        {
                            da.Update(ds, this._TableInfo.Name);
                            ds.AcceptChanges();
                        }
                        #endregion

                        nCurrentPageIndex++;
                    }

                    transaction.Commit();
                    bSuccess = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    sMessage = "Import failed";
                    sMessage += "\r\n" + ex.Message.ToString();
                    bSuccess = false;
                }
                finally
                {
                    connection.Close();
                }
            }

            if (bSuccess)
            {
                sMessage = "Import successfully";
                sMessage += "\r\n" + string.Format("Added {0} record(s), Updated {1} record(s), ", nCountEntitiesAdded, nCountEntitiesUpdated);
                MessageBox.Show(this, sMessage, "Import Resulting", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(this, sMessage, "Import Resulting", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //waiting.Close();
        }

        public void BuildDataRow(DataTable dt, DataRow dr, CustomEntity customEntity)
        {
            int nLength = this._SelectedFields.Count;
            for (int i = 0; i < nLength; i++)
            {
                FieldInfo field = this._SelectedFields[i];
                Type colType = dt.Columns[_SelectedFields[i].Name].DataType;
                string sValue = customEntity.Properties[i] != null ? customEntity.Properties[i].ToString().Trim() : string.Empty;
                if (FieldInfo.HasFunctionField(field))
                {
                    sValue = customEntity.FuncValues[i] != null ? customEntity.FuncValues[i].ToString().Trim() : string.Empty;
                }

                object value = null;
                TypeHelper.GetValue(sValue, colType, field, out value);
                if (value == null)
                {
                    if (!string.IsNullOrEmpty(field.DBDefaulValueOrBinding))
                    {
                        value = field.GetDBDefaulValueOrBinding();
                    }

                    if (value == null)
                    {
                        value = DBNull.Value;
                    }
                }

                dr[this._SelectedFields[i].Name] = value;
            }
        }

        public DataRow FindDataRow(DataTable dataTable, CustomEntity customEntity)
        {
            if (dataTable.Rows.Count <= 0 || !customEntity.UniqueColumnIndex.HasValue)
            {
                return null;
            }

            string sUniqueValue = (string)customEntity.Properties[customEntity.UniqueColumnIndex.Value];
            string sFilterExpression = string.Format("{0} = {1}{2}{3}", _NameOfUniqueField, _Mark, sUniqueValue, _Mark);
            DataRow[] rows = dataTable.Select(sFilterExpression);
            if (rows.Length > 0)
            {
                return rows[0];
            }

            return null;
        }

        /// <summary>
        /// Get the list of column names that need to be selected from DB
        /// </summary>
        /// <returns></returns>
        public string GetColumns()
        {
            string sColumnNames = "*";
            if (this._SelectedFields == null || this._SelectedFields.Count == 0)
            {
                return sColumnNames;
            }

            FieldInfo fieldInfoKey = this._AllFields.FirstOrDefault(f => f.IsPrimaryKey);
            if (fieldInfoKey != null)
            {
                FieldInfo[] fieldInfosIncludeKey = new FieldInfo[this._SelectedFields.Count + 1];
                this._SelectedFields.CopyTo(fieldInfosIncludeKey, 1);
                fieldInfosIncludeKey[0] = fieldInfoKey;
                string[] ColumnNames = fieldInfosIncludeKey.Select(field => field.Name).ToArray();
                if (ColumnNames != null && ColumnNames.Length > 0)
                {
                    sColumnNames = string.Empty;
                    foreach (string columnName in ColumnNames)
                    {
                        sColumnNames += (string.IsNullOrEmpty(sColumnNames) ? string.Empty : ", ") + columnName;
                    }
                }
            }

            return sColumnNames;
        }

        public string GetValues(IList<CustomEntity> CustomEntities)
        {
            string sValues = string.Empty;
            if (CustomEntities == null || CustomEntities.Count == 0)
            {
                return sValues;
            }

            FieldInfo fieldInfo = this._SelectedFields.FirstOrDefault(f => f.Name == _NameOfUniqueField);
            if (fieldInfo == null)
            {
                return sValues;
            }

            this._Mark = (fieldInfo.DataTypeName.EndsWith("char") || fieldInfo.DataTypeName.EndsWith("text")) ? "'" : "";
            foreach (CustomEntity entity in CustomEntities)
            {
                sValues += (string.IsNullOrEmpty(sValues) ? string.Empty : ", ") + _Mark + (string)entity.Properties[entity.UniqueColumnIndex.Value] + _Mark;
            }

            return sValues;
        }

        private void RadioButton_ShowError_Check(object sender, RoutedEventArgs e)
        {
            PagingNavigator_Main.SetupPaging(DataGrid_Entities, this._PreviewEntities.Where(row => row.HasError), 25);
        }

        private void RadioButton_ShowWarning_Check(object sender, RoutedEventArgs e)
        {
            PagingNavigator_Main.SetupPaging(DataGrid_Entities, this._PreviewEntities.Where(row => row.HasWarning), 25);
        }

        private void RadioButton_ShowGood_Check(object sender, RoutedEventArgs e)
        {
            PagingNavigator_Main.SetupPaging(DataGrid_Entities, this._PreviewEntities.Where(row => !row.HasWarning && !row.HasError), 25);
        }

        private void Button_ShowAll_Check(object sender, RoutedEventArgs e)
        {
            PagingNavigator_Main.SetupPaging(DataGrid_Entities, this._PreviewEntities, 25);
        }
    }
}
