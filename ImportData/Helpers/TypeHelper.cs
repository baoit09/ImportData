using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ImportData.Helpers
{
    public class TypeHelper
    {
        private static DataTable _dataTableCache = new DataTable();
        private static bool _HasGotTable = false;

        private static DataTable _schemaCache = new DataTable();
        private static bool _HasGotSchema = false;

        public static Type GetType(FieldInfo fieldInfo, TableInfo tableInfo)
        {
            if (!_HasGotTable)
            {
                _HasGotTable = true;

                string SQLSelectCommand = string.Format("SELECT * FROM {0} WHERE 1 = 2", tableInfo.Name);
                using (SqlConnection connection = new SqlConnection(DataBaseInfo.ConnectionString))
                {
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = new SqlCommand(SQLSelectCommand, connection);

                    DataSet ds = new DataSet();
                    da.Fill(_dataTableCache);
                }
            }

            if (_dataTableCache != null)
            {
                return _dataTableCache.Columns[fieldInfo.Name] != null ? _dataTableCache.Columns[fieldInfo.Name].DataType : null;
            }
            else
            {
                return null;
            }
        }

        public static object GetDBDefaultValue(FieldInfo fieldInfo, TableInfo tableInfo)
        {
            if (!_HasGotSchema)
            {
                _HasGotSchema = true;

                SqlConnection connection = new SqlConnection(DataBaseInfo.ConnectionString);
                try
                {
                    connection.Open();
                    _schemaCache = connection.GetSchema("Columns", new string[4] { connection.Database, null, tableInfo.Name, null });
                }
                finally
                {
                    connection.Close();
                }
            }

            if (_HasGotSchema != null)
            {
                foreach (DataRow row in _schemaCache.Rows)
                {
                    if (row["COLUMN_NAME"].ToString() == fieldInfo.Name)
                    {
                        return row["COLUMN_DEFAULT"];
                    }
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        public static bool GetValue(string inValue, Type colType, FieldInfo field, out object outValue)
        {
            outValue = null;
            // Start to parse string
            if (colType == typeof(Int32) || colType == typeof(Nullable<Int32>))
            {
                int intValue = 0;
                if (Int32.TryParse(inValue, out intValue))
                {
                    outValue = intValue;
                    return true;
                }
                return false;
            }
            else if (colType == typeof(String) || colType == typeof(string))
            {
                outValue = inValue;
                return true;
            }
            else if (colType == typeof(Decimal) || colType == typeof(Nullable<Decimal>))
            {
                Decimal dValue = Decimal.Zero;
                if (Decimal.TryParse(inValue, out dValue))
                {
                    outValue = dValue;
                    return true;
                }
                return false;
            }
            else if (colType == typeof(Double) || colType == typeof(Nullable<Double>))
            {
                Double dValue = 0.0;
                if (Double.TryParse(inValue, out dValue))
                {
                    outValue = dValue;
                    return true;
                }
                return false;
            }
            else if (colType == typeof(char) || colType == typeof(Nullable<char>))
            {
                if (field.MaxLength.CompareTo("1") == 0)
                {
                    outValue = inValue.ToCharArray()[0];
                    return true;
                }
                else
                {
                    outValue = inValue.ToCharArray();
                    return true;
                }
            }
            else if (colType == typeof(bool) || colType == typeof(Nullable<bool>))
            {
                if (new string[] { "0", "1", "t", "f", "true", "false" }.Contains(inValue.Trim().ToLower()))
                {
                    if (inValue.Equals("0") || inValue.Equals("f") || inValue.Equals("false"))
                    {
                        outValue = false;
                    }
                    else
                    {
                        outValue = true;
                    }
                    return true;
                }

                return false;
            }
            else if (colType == typeof(DateTime) || colType == typeof(Nullable<DateTime>))
            {
                DateTime tempDate;
                double dVal = 0;

                if (double.TryParse(inValue, out dVal))
                {
                    tempDate = DateTime.FromOADate(dVal);
                    if (dVal > 0)
                    {
                        outValue = tempDate;
                        return true;
                    }
                }
                else if (DateTime.TryParse(inValue, out tempDate))
                {
                    outValue = tempDate;
                    return true;
                }
                return false;
            }
            else
            {
                try
                {
                    return GetValue((string)inValue, colType, out outValue);
                }
                catch
                {
                }
            }

            return false;
        }

        private static bool GetValue(string sValue, Type type, out object objValue)
        {
            objValue = null;
            try
            {
                objValue = Convert.ChangeType(sValue, type);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
