using Excel = Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace ImportData.Helpers
{
    /// <summary>
    /// Cung cấp các hàm thư viện phục vụ cho Read/Write Excel
    /// </summary>
    public static class ExcelReaderWriterHelper
    {
        /// <summary>
        /// Hàm này đóng vai trò như mồi cho việc kiểm tra Excel (COM+) có tồn tại hay chưa
        /// </summary>
        private static void TestCalculatorExcel()
        {
            Excel.ApplicationClass excelApplication = new Microsoft.Office.Interop.Excel.ApplicationClass();
            CloseExcelApplicationClass(excelApplication);
        }

        public static void CloseExcelApplicationClass(Excel.ApplicationClass _ExcelApplication)
        {
            try
            {
                if (_ExcelApplication != null)
                {
                    _ExcelApplication.Workbooks.Close();
                    _ExcelApplication.Quit();
                }
            }
            finally { }
            try
            {
                //Close ComObject:
                if (_ExcelApplication.ActiveWorkbook != null)
                {
                    Marshal.ReleaseComObject(_ExcelApplication.ActiveWorkbook);
                }
            }
            finally { }

            try
            {
                if (_ExcelApplication != null)
                {
                    Marshal.ReleaseComObject(_ExcelApplication);
                }
            }
            finally { }

        }
        private static bool? _IsExcelDLLAvailable = null;
        /// <summary>
        /// Kiểm tra có cài đặt Excel (COM+) hay chưa
        /// Just test once when application run
        /// </summary>
        /// <returns>False: khi chưa cài đặt Excel</returns>
        public static bool IsExcelDLLAvailable()
        {
            try
            {
                if (_IsExcelDLLAvailable == null)
                    TestCalculatorExcel();
                _IsExcelDLLAvailable = true;
                return _IsExcelDLLAvailable.Value;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Mở một ứng dụng Excel.
        /// </summary>
        /// <returns>null: Nếu chưa có cài đặt Excel (COM+), ngược lại trả về excel application</returns>
        public static Excel.ApplicationClass GetApplication()
        {
            if (IsExcelDLLAvailable())
                //return new Excel.ApplicationClass() { Visible = true };
                return new Excel.ApplicationClass();
            else return null;
        }


        /// <summary>
        /// Lấy Workbook từ excel file. Có cơ chế đọc dạng ReadOnly
        /// </summary>
        /// <param name="excelApplication">Excel Application</param>
        /// <param name="excelFileName">Đường dẫn excel File</param>
        /// <returns>null: nếu không lấy workbook, ngược lại thì trả về Workbook tương ứng</returns>
        public static Excel.Workbook GetWorkbook(Excel.ApplicationClass excelApplication, string excelFileName, bool aReadOnly)
        {
            Debug.Assert(excelApplication != null);

            if (excelApplication != null)
            {
                try
                {

                    if (!string.IsNullOrEmpty(excelFileName) && File.Exists(excelFileName))
                        return (Excel.Workbook)excelApplication.Workbooks.Open(excelFileName, 0, aReadOnly, 5, "", "", false, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                    else return null;
                }
                catch
                {
                    return null;
                }
                finally
                {

                }
            }
            else return null;
        }
        /// <summary>
        /// Lấy Workbook từ excel file. ReadOnly = False
        /// </summary>
        public static Excel.Workbook GetWorkbook(Excel.ApplicationClass excelApplication, string excelFileName)
        {
            return GetWorkbook(excelApplication, excelFileName, false);
        }

        /// <summary>
        /// Lấy Worksheet từ workbook theo sheetName
        /// </summary>
        /// <param name="workbook">Workbook</param>
        /// <param name="sheetName">Tên sheet</param>
        /// <returns>null: nếu không lấy được worksheet, ngược lại trả về worksheet tương ứng</returns>
        public static Excel.Worksheet GetWorksheet(Excel.Workbook workbook, string sheetName)
        {
            Debug.Assert(workbook != null);

            foreach (Excel.Worksheet worksheet in workbook.Worksheets)
            {
                if (worksheet.Name.Equals(sheetName))
                    return worksheet;
            }

            return null;
        }
        public static ColumnInfo[] GetColumnInfoUsed(string sFileName, string sSheetName)
        {
            if (!string.IsNullOrEmpty(sSheetName) && (!string.IsNullOrEmpty(sFileName)))
            {
                using (var excel = new ExcelReaderWriter(sFileName, sSheetName))
                {
                    excel.OpenWorkbook(true);
                    int nColumn = excel.ActiveWorksheet.UsedRange.Columns.Count;
                    int? nColumnTo = GetLastCoumnIndex(excel.ActiveWorksheet.UsedRange);
                    if (!nColumnTo.HasValue)
                    {
                        excel.CloseWorkbook();
                        return null;
                    }
                    int nColumnFrom = nColumnTo.Value - nColumn + 1;
                    List<ColumnInfo> list = new List<ColumnInfo>();
                    for (int i = nColumnFrom; i <= nColumnTo; i++)
                        list.Add(new ColumnInfo()
                        {
                            Index = i,
                            Name = GetNameFromColumnIndex(i)
                        });
                    excel.CloseWorkbook();
                    return list.ToArray();
                }
            }
            return null;
        }
        public static string GetNameFromColumnIndex(int aColumnIndex)
        {
            string lastColumn = "";
            // check whether the column count is > 26
            if (aColumnIndex > 26)
            {
                // If the column count is > 26, the the last column index will be something
                // like "AA", "DE", "BC" etc

                // Get the first letter
                // ASCII index 65 represent char. 'A'. So, we use 64 in this calculation as a starting point
                char first = Convert.ToChar(64 + ((aColumnIndex - 1) / 26));

                // Get the second letter
                char second = Convert.ToChar(64 + (aColumnIndex % 26 == 0 ? 26 : aColumnIndex % 26));

                // Concat. them
                lastColumn = first.ToString() + second.ToString();
            }
            else
            {
                // ASCII index 65 represent char. 'A'. So, we use 64 in this calculation as a starting point
                lastColumn = Convert.ToChar(64 + aColumnIndex).ToString();
            }
            return lastColumn;
        }
        public static int? GetLastCoumnIndex(Microsoft.Office.Interop.Excel.Range range)
        {
            var cell = range.SpecialCells(Microsoft.Office.Interop.Excel.XlCellType.xlCellTypeLastCell, System.Reflection.Missing.Value);
            return cell == null ? null : (int?)cell.Column;
        }
        public static int? GetLastRowIndex(Microsoft.Office.Interop.Excel.Range range)
        {
            var cell = range.SpecialCells(Microsoft.Office.Interop.Excel.XlCellType.xlCellTypeLastCell, System.Reflection.Missing.Value);
            return cell == null ? null : (int?)cell.Row;
        }
        /// <summary>
        /// Get list of Sheet. ReadOnly = fasle
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static IList<SheetInfo> GetSheetInfos(string fileName)
        {
            return GetSheetInfos(fileName, false);
        }
        /// <summary>
        /// Get the list of Sheets
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="aReadOnly"></param>
        /// <returns></returns>
        public static IList<SheetInfo> GetSheetInfos(string fileName, bool aReadOnly)
        {
            if (!File.Exists(fileName))
                return null;

            IList<SheetInfo> m_SheetInfos = new List<SheetInfo>();

            ExcelReaderWriter m_ExcelReaderWriter = new ExcelReaderWriter(fileName, 1);
            try
            {
                m_ExcelReaderWriter.OpenWorkbook(true);
                int i = 1;
                foreach (Excel.Worksheet m_WorkSheet in m_ExcelReaderWriter.ActiveWorkbook.Sheets)
                {
                    var m_SheetInfo = new SheetInfo() { Name = m_WorkSheet.Name, Index = i };

                    //#region Set Column Infos
                    //object[,] m_Values = (object[,])m_ExcelReaderWriter.ActiveWorksheet.UsedRange.Value2;
                    //if (m_Values != null)
                    //{
                    //    int m_UseRangeRowLength = (int)m_Values.GetLongLength(0);
                    //    int m_UseRangeColumnLength = (int)m_Values.GetLongLength(1);

                    //    //Recheck
                    //    //  - BeginHeaderColumnIndex
                    //    if (m_SheetInfo.BeginHeaderColumnIndex == 0)
                    //        m_SheetInfo.BeginHeaderColumnIndex = 1;
                    //    else if (m_SheetInfo.BeginHeaderColumnIndex > m_UseRangeColumnLength)
                    //        m_SheetInfo.BeginHeaderColumnIndex = m_UseRangeColumnLength;

                    //    //  - EndHeaderColumnIndex
                    //    if (m_SheetInfo.EndHeaderColumnIndex == 0)
                    //        m_SheetInfo.EndHeaderColumnIndex = m_UseRangeColumnLength;
                    //    else if (m_SheetInfo.EndHeaderColumnIndex < m_SheetInfo.BeginHeaderColumnIndex)
                    //        m_SheetInfo.EndHeaderColumnIndex = m_SheetInfo.BeginHeaderColumnIndex;
                    //    else if (m_SheetInfo.EndHeaderColumnIndex > m_UseRangeColumnLength)
                    //        m_SheetInfo.EndHeaderColumnIndex = m_UseRangeColumnLength;

                    //    //  - BeginRowIndex
                    //    if (m_SheetInfo.BeginRowIndex == 0)
                    //        m_SheetInfo.BeginRowIndex = 1;
                    //    else if (m_SheetInfo.BeginRowIndex > m_UseRangeRowLength)
                    //        m_SheetInfo.BeginRowIndex = m_UseRangeRowLength;

                    //    //  - EndRowIndex
                    //    if (m_SheetInfo.EndRowIndex == 0)
                    //        m_SheetInfo.EndRowIndex = m_UseRangeRowLength;
                    //    else if (m_SheetInfo.EndRowIndex > m_UseRangeRowLength)
                    //        m_SheetInfo.EndRowIndex = m_UseRangeRowLength;
                    //    else if (m_SheetInfo.EndRowIndex < m_SheetInfo.BeginRowIndex)
                    //        m_SheetInfo.EndRowIndex = m_SheetInfo.BeginRowIndex;

                    //    //  - HeaderRowIndex
                    //    if (m_SheetInfo.HeaderRowIndex == 0)
                    //        m_SheetInfo.HeaderRowIndex = 1;
                    //    else if (m_SheetInfo.HeaderRowIndex > m_SheetInfo.EndRowIndex)
                    //        m_SheetInfo.HeaderRowIndex = m_SheetInfo.EndRowIndex;

                    //    for (int c = m_SheetInfo.BeginHeaderColumnIndex; c <= m_SheetInfo.EndHeaderColumnIndex; c++)
                    //    {
                    //        ColumnInfo m_ColumnInfo = new ColumnInfo()
                    //        {
                    //            Index = c,
                    //            Header = ConvertedColumnData.Instance.Data[c],
                    //            Name = ConvertedColumnData.Instance.Data[c]
                    //        };
                    //        m_SheetInfo.Columns.Add(m_ColumnInfo);
                    //    }
                    //}
                    //#endregion

                    m_SheetInfos.Add(m_SheetInfo);
                    i++;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                m_SheetInfos.Clear();
            }
            finally
            {
                m_ExcelReaderWriter.CloseWorkbook();
            }

            return m_SheetInfos;
        }

        /// <summary>
        /// Get the list of Columns for sheet
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="aReadOnly"></param>
        /// <returns></returns>
        public static void SetColumnInfos(string fileName, SheetInfo sheetInfo)
        {
            if (sheetInfo.Columns != null && sheetInfo.Columns.Count > 0)
                return;

            if (!File.Exists(fileName))
                return;

            IList<SheetInfo> m_SheetInfos = new List<SheetInfo>();
            ExcelReaderWriter m_ExcelReaderWriter = new ExcelReaderWriter(fileName, 1);
            try
            {
                m_ExcelReaderWriter.OpenWorkbook(true);
                foreach (Excel.Worksheet m_WorkSheet in m_ExcelReaderWriter.ActiveWorkbook.Sheets)
                {
                    if (m_WorkSheet.Name == sheetInfo.Name)
                    {
                        #region Set Column Infos
                        object[,] m_Values = (object[,])m_WorkSheet.UsedRange.Value2;
                        if (m_Values != null)
                        {
                            int m_UseRangeRowLength = (int)m_Values.GetLongLength(0);
                            int m_UseRangeColumnLength = (int)m_Values.GetLongLength(1);

                            //Recheck
                            //  - BeginHeaderColumnIndex
                            if (sheetInfo.BeginHeaderColumnIndex == 0)
                                sheetInfo.BeginHeaderColumnIndex = 1;
                            else if (sheetInfo.BeginHeaderColumnIndex > m_UseRangeColumnLength)
                                sheetInfo.BeginHeaderColumnIndex = m_UseRangeColumnLength;

                            //  - EndHeaderColumnIndex
                            if (sheetInfo.EndHeaderColumnIndex == 0)
                                sheetInfo.EndHeaderColumnIndex = m_UseRangeColumnLength;
                            else if (sheetInfo.EndHeaderColumnIndex < sheetInfo.BeginHeaderColumnIndex)
                                sheetInfo.EndHeaderColumnIndex = sheetInfo.BeginHeaderColumnIndex;
                            else if (sheetInfo.EndHeaderColumnIndex > m_UseRangeColumnLength)
                                sheetInfo.EndHeaderColumnIndex = m_UseRangeColumnLength;

                            //  - BeginRowIndex
                            if (sheetInfo.BeginRowIndex == 0)
                                sheetInfo.BeginRowIndex = 1;
                            else if (sheetInfo.BeginRowIndex > m_UseRangeRowLength)
                                sheetInfo.BeginRowIndex = m_UseRangeRowLength;

                            //  - EndRowIndex
                            if (sheetInfo.EndRowIndex == 0)
                                sheetInfo.EndRowIndex = m_UseRangeRowLength;
                            else if (sheetInfo.EndRowIndex > m_UseRangeRowLength)
                                sheetInfo.EndRowIndex = m_UseRangeRowLength;
                            else if (sheetInfo.EndRowIndex < sheetInfo.BeginRowIndex)
                                sheetInfo.EndRowIndex = sheetInfo.BeginRowIndex;

                            //  - HeaderRowIndex
                            if (sheetInfo.HeaderRowIndex == 0)
                                sheetInfo.HeaderRowIndex = 1;
                            else if (sheetInfo.HeaderRowIndex > sheetInfo.EndRowIndex)
                                sheetInfo.HeaderRowIndex = sheetInfo.EndRowIndex;

                            for (int c = sheetInfo.BeginHeaderColumnIndex; c <= sheetInfo.EndHeaderColumnIndex; c++)
                            {
                                ColumnInfo m_ColumnInfo = new ColumnInfo()
                                {
                                    Index = c,
                                    Header = ConvertedColumnData.Instance.Data[c],
                                    Name = ConvertedColumnData.Instance.Data[c]
                                };
                                sheetInfo.Columns.Add(m_ColumnInfo);
                            }
                        }
                        #endregion

                        return;
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
        }
    }

    public class SheetInfo : INotifyPropertyChanged
    {
        public SheetInfo()
            : base()
        {
            this.Columns = new List<ColumnInfo>();
        }

        private int index;
        public int Index
        {
            get { return this.index; }
            set
            {
                if (this.index != value)
                {
                    this.index = value;
                    this.OnPropertyChanged("Index");
                }
            }
        }

        private string name;
        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    this.OnPropertyChanged("Name");
                }
            }
        }

        private int headerRowIndex = 1;
        public int HeaderRowIndex
        {
            get { return this.headerRowIndex; }
            set
            {
                if (this.headerRowIndex != value)
                {
                    this.headerRowIndex = value;
                    this.OnPropertyChanged("HeaderRowIndex");
                }
            }
        }

        private int beginHeaderColumnIndex = 1;
        public int BeginHeaderColumnIndex
        {
            get { return this.beginHeaderColumnIndex; }
            set
            {
                if (this.beginHeaderColumnIndex != value)
                {
                    this.beginHeaderColumnIndex = value;
                    this.OnPropertyChanged("BeginHeaderColumnIndex");
                }
            }
        }

        private int endHeaderColumnIndex = 0;
        public int EndHeaderColumnIndex
        {
            get { return this.endHeaderColumnIndex; }
            set
            {
                if (this.endHeaderColumnIndex != value)
                {
                    this.endHeaderColumnIndex = value;
                    this.OnPropertyChanged("EndHeaderColumnIndex");
                }
            }
        }

        private int beginRowIndex = 1;
        public int BeginRowIndex
        {
            get { return this.beginRowIndex; }
            set
            {
                if (this.beginRowIndex != value)
                {
                    this.beginRowIndex = value;
                    this.OnPropertyChanged("BeginRowIndex");
                }
            }
        }

        private int endRowIndex = 0;
        public int EndRowIndex
        {
            get { return this.endRowIndex; }
            set
            {
                if (this.endRowIndex != value)
                {
                    this.endRowIndex = value;
                    this.OnPropertyChanged("EndRowIndex");
                }
            }
        }

        public IList<ColumnInfo> Columns { get; set; }
        public void FillColumns(IList<ColumnInfo> columns)
        {
            this.Columns.Clear();
            Debug.Assert(columns != null);
            foreach (var m_Column in columns)
            {
                this.Columns.Add(m_Column);
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class ColumnInfo : INotifyPropertyChanged
    {
        public ColumnInfo() { }

        private int index;
        public int Index
        {
            get { return this.index; }
            set
            {
                if (this.index != value)
                {
                    this.index = value;
                    this.ExcelIndex = ExcelInfoHelper.ConvertToExcelIndex(this.index);
                    this.OnPropertyChanged("Index");
                }
            }
        }

        private string excelIndex;
        public string ExcelIndex
        {
            get { return excelIndex; }
            set
            {
                if (this.excelIndex != value)
                {
                    this.excelIndex = value;
                    this.OnPropertyChanged("ExcelIndex");
                }
            }
        }

        private string name;
        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    this.OnPropertyChanged("Name");
                }
            }
        }

        private string header;
        public string Header
        {
            get { return this.header; }
            set
            {
                if (this.header != value)
                {
                    this.header = value;
                    this.OnPropertyChanged("Header");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class ExcelInfoHelper
    {
        private static IDictionary<int, string> _Data;
        private static IDictionary<int, string> Data
        {
            get
            {
                if (_Data == null)
                    _Data = ConvertedColumnData.Instance.Data;

                return _Data;
            }
        }
        public static string ConvertToExcelIndex(int nIndex)
        {
            string cIndex = Data[nIndex];
            return cIndex.ToString();
        }
        public static int ConvertToViewIndex(string sIndex)
        {
            int nIndex = 0;

            if (Data.Where(d => d.Value.Equals(sIndex)).Count() > 0)
            {
                KeyValuePair<int, string> keyValuePair = Data.First(d => d.Value.Equals(sIndex));
                nIndex = keyValuePair.Key;
            }
            else
                nIndex = 0;

            return nIndex;
        }
    }

    public sealed class ConvertedColumnData
    {
        public static ConvertedColumnData Instance = new ConvertedColumnData();

        private ConvertedColumnData()
        {
            _Data = new Dictionary<int, string>();

            int i = 1;
            //A -> Z
            for (char c = 'A'; c <= 'Z'; c++)
            {
                _Data.Add(i, string.Concat(c));
                i++;
            }

            //AA -> ...
            for (i = 27; i <= 256; i++)
            {
                //int nNguyenIndex = i / 26 > 0 ? i / 26 : 26;
                //20110110: KhanhTL modify 
                int nNguyenIndex = (i - 1) / 26;
                string sNguyenValue = _Data[nNguyenIndex];

                int nDuIndex = i % 26 > 0 ? i % 26 : 26;
                string sDuValue = _Data[nDuIndex];

                _Data.Add(i, string.Concat(sNguyenValue, sDuValue));
            }
        }

        private IDictionary<int, string> _Data;
        public IDictionary<int, string> Data { get { return _Data; } }
    }
}
