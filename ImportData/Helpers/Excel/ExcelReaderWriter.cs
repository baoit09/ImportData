using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Globalization;

namespace ImportData.Helpers
{
    /// <summary>
    /// Lớp cơ sở cho phép truy cập đến ExcelFile, hỗ trợ đóng mở Excel Application, Workbook, Worksheet...
    /// </summary>
    public class ExcelReaderWriter : IDisposable
    {
        #region Constructors

        /// <summary>
        /// Khởi tạo mặc định không tham số
        /// </summary>
        public ExcelReaderWriter() { Initialize(); }

        /// <summary>
        /// Init instance with config info, recall Constructor()
        /// </summary>
        /// <param name="fileName">The path of excel file</param>
        /// <param name="sheetIndex">The index of current sheet</param>
        public ExcelReaderWriter(string fileName, int sheetIndex)
            : this()
        {
            this.FileName = fileName;
            this.SheetIndex = sheetIndex;
        }

        /// <summary>
        /// Khởi tạo contructor với config, gọi lại Constructor()
        /// </summary>
        /// <param name="fileName">Đường dẫn của file thao tác</param>
        /// <param name="sheetName">Tên của Sheet hiện hành</param>
        public ExcelReaderWriter(string fileName, string sheetName)
            : this()
        {
            this.FileName = fileName;
            this.SheetName = sheetName;
        }

        /// <summary>
        /// Hàm khởi tạo chung.
        /// Hàm này được gọi khi thực hiện Constructor "không tham số" (Constructor()).
        /// Các Constructors còn lại đều gọi Constructor(). Do đó, hàm này đều gọi trước các constructors "có tham số"
        /// </summary>
        protected virtual void Initialize()
        {
            this.SheetIndex = 1;	//Giá trị mặc định
        }

        #endregion

        #region Properties

        #region Config

        /// <summary>
        /// Đường dẫn của file thao tác
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Index của Sheet hiện hành
        /// </summary>
        public int SheetIndex
        {
            get { return _SheetIndex; }
            set
            {
                _SheetIndex = value;
                _SheetName = SheetNameNoSelect;
                GetSheetMode = GetSheetModeEnum.Index;
            }
        }
        private int _SheetIndex = SheetIndexNoSelect;

        /// <summary>
        /// Tên của Sheet hiện hành
        /// </summary>
        public string SheetName
        {
            get { return _SheetName; }
            set
            {
                _SheetName = value;
                _SheetIndex = SheetIndexNoSelect;
                GetSheetMode = GetSheetModeEnum.Name;
            }
        }
        private string _SheetName = SheetNameNoSelect;

        //Trong một thời điểm chỉ có thể là SheetIndex hoặc là SheetName.
        //Nghĩa là nếu sử dụng SheetIndex thì SheetName phải được clear và ngược lại
        private const int SheetIndexNoSelect = 0;
        private const string SheetNameNoSelect = "";

        private GetSheetModeEnum GetSheetMode;
        private enum GetSheetModeEnum { Index, Name }

        #endregion

        #region Info

        /// <summary>
        /// Working with excel file via this Workbook.
        /// This Workbook is also a current Workbook of Excel Application.
        /// Using this Workbook once opening the connection to excel file.
        /// </summary>
        public Excel.Workbook ActiveWorkbook { get; protected set; }

        /// <summary>
        /// ActiveSheet là Worksheet hiện hành của Workbook
        /// </summary>
        public Excel.Worksheet ActiveWorksheet { get; protected set; }

        /// <summary>
        /// Cho biết hiện tại đã mở excel file chưa
        /// </summary>
        public bool IsOpened
        {
            get { return _ExcelApplication != null; }
        }

        /// <summary>
        /// ExcelApplication sẽ được tạo mới nếu = null
        /// </summary>
        private Excel.ApplicationClass ExcelApplication
        {
            get
            {
                if (_ExcelApplication == null)
                {
                    _ExcelApplication = ExcelReaderWriterHelper.GetApplication();
                    GetCurrrentExcelProcessId();
                }

                return _ExcelApplication;
            }
        }
        private Excel.ApplicationClass _ExcelApplication = null;

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Mở kết nối với Excel file. Sau bước này mới có thể truy cập Workbook, Worksheet
        /// </summary>
        /// <returns>True: Nếu mở excel file thành công</returns>
        public bool OpenWorkbook()
        {
            return OpenWorkbook(FileName, false);
        }
        /// <summary>
        /// Mở kết nối với Excel file. Sau bước này mới có thể truy cập Workbook, Worksheet
        /// </summary>
        /// <returns>True: Nếu mở excel file thành công</returns>
        public bool OpenWorkbook(bool aReadOnly)
        {
            return OpenWorkbook(FileName, false, aReadOnly);
        }
        /// <summary>
        /// Mở kết nối với Excel file. Sau bước này mới có thể truy cập Workbook, Worksheet
        /// </summary>
        /// <param name="fileName">Đường dẫn của file thao tác</param>
        /// <returns>True: Nếu mở excel file thành công</returns>
        public bool OpenWorkbook(string fileName)
        {
            return OpenWorkbook(fileName, true);
        }
        /// <summary>
        /// Mở kết nối với Excel file. Sau bước này mới có thể truy cập Workbook, Worksheet
        /// ReadOnly = fasle
        /// </summary>
        public bool OpenWorkbook(string fileName, bool saveFileName)
        {
            return OpenWorkbook(fileName, saveFileName, false);
        }
        /// <summary>
        /// Mở kết nối với Excel file. Sau bước này mới có thể truy cập Workbook, Worksheet
        /// </summary>
        /// <param name="fileName">Đường dẫn của file thao tác</param>
        /// <param name="saveFileName">True: thực hiện gán FileName và ngược lại</param>
        /// <returns>True: Nếu mở excel file thành công</returns>
        public bool OpenWorkbook(string fileName, bool saveFileName, bool aReadOnly)
        {
            if (saveFileName)
                this.FileName = fileName;

            System.Threading.Thread thisThread = System.Threading.Thread.CurrentThread;
            System.Globalization.CultureInfo originalCulture = (System.Globalization.CultureInfo)thisThread.CurrentCulture.Clone();

            try
            {
                thisThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                this.ActiveWorkbook = ExcelReaderWriterHelper.GetWorkbook(ExcelApplication, fileName, aReadOnly);
                if (ActiveWorkbook != null)
                {
                    switch (GetSheetMode)
                    {
                        case GetSheetModeEnum.Index:
                            ActiveWorksheet = (Excel.Worksheet)ActiveWorkbook.Worksheets[SheetIndex];
                            break;
                        case GetSheetModeEnum.Name:
                            ActiveWorksheet = ExcelReaderWriterHelper.GetWorksheet(ActiveWorkbook, SheetName);
                            break;
                        default:
                            break;
                    }

                    return true;
                }
                else
                    return false;
            }
            catch
            {
                thisThread.CurrentCulture = originalCulture;
                return false;
            }
            finally
            {
                thisThread.CurrentCulture = originalCulture;
            }
        }

        /// <summary>
        /// Đóng Excel file, thoát Excel Application, ngắt kết nối với COM+ và CG thu dọn rác
        /// </summary>
        public void CloseWorkbook()
        {
            //http://www.velocityreviews.com/forums/t75190-re-how-to-close-excel-object-with-c.html

            ActiveWorksheet = null;

            try
            {
                //Close and Quit Excel:
                if (ActiveWorkbook != null)
                    ActiveWorkbook.Close(false, string.Empty, null);
            }
            finally { }

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
                if (ActiveWorkbook != null)
                {
                    Marshal.ReleaseComObject(ActiveWorkbook);
                    ActiveWorkbook = null;
                }
            }
            finally { }

            try
            {
                if (_ExcelApplication != null)
                {
                    Marshal.ReleaseComObject(_ExcelApplication);
                    _ExcelApplication = null;
                    _ExcelProcessId = 0;
                }
            }
            finally { }

            GC.Collect(); // force final cleanup!
        }

        /// <summary>
        /// Close Process -> cuc ki ta dao!!!
        /// </summary>
        private void CloseWorkbookUsingKillProcess()
        {
            try
            {
                ActiveWorksheet = null;
                ActiveWorkbook = null;
                _ExcelApplication = null;

                //Check Is closed ProcessId
                if (_ExcelProcessId != IsClosedProcessId)
                {
                    //Get and kill Excel process
                    var process = Process.GetProcessById(_ExcelProcessId);
                    process.Kill();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion

        #region IDisposable Members

        // Track whether Dispose has been called.
        private bool disposed = false;

        /// <summary>
        /// Implement IDisposable.
        ///	Do not make this method virtual.
        ///	A derived class should not be able to override this method.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// </summary>
        /// <param name="disposing">        If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources can be disposed. 
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    CloseWorkbook();
                }

                //Cái này không tốt!!!
                CloseWorkbookUsingKillProcess();

                // Note disposing has been done.
                disposed = true;
            }
        }

        /// <summary>
        /// Use C# destructor syntax for finalization code.
        /// This destructor will run only if the Dispose method
        /// does not get called.
        /// It gives your base class the opportunity to finalize.
        /// Do not provide destructors in types derived from this class.
        /// </summary>
        ~ExcelReaderWriter()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        /// <summary>
        /// Lưu trữ Id của excel process.
        /// Vì một lý do nào đó trong quá trình làm việc Excel mở Workbook nhưng chưa thể đóng.
        /// Ngay lúc này khi dispose, sẽ không thể gọi CloseWorkbook được vì COM+ chỉ sử dụng trong cùng một Thread.
        /// Do đó phải sử dụng Process.Kill() để khắc phục tình trạng này.
        /// </summary>
        private int _ExcelProcessId = IsClosedProcessId;
        /// <summary>
        /// Hằng số mặc định khi chưa mở Workbook -> Default = 0
        /// </summary>
        private const int IsClosedProcessId = 0;

        /// <summary>
        /// Lấy ProcessId của hWnd tương ứng
        /// </summary>
        /// <param name="hWnd">hWnd</param>
        /// <param name="lpdwProcessId">ProcessId tương ứng</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(int hWnd, out int lpdwProcessId);

        /// <summary>
        /// Lấy excell ProcessId hiện tại tương ứng với ứng dụng Excel đang mở
        /// </summary>
        private void GetCurrrentExcelProcessId()
        {
            GetWindowThreadProcessId(_ExcelApplication.Hwnd, out _ExcelProcessId);
        }

        #endregion
    }
}
