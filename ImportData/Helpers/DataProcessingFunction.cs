using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace ImportData.Helpers
{
    public class DataProcessingFunction
    {
        enum FunctionID { SkipEmpty = 0, Lookup, Translate, Proper, Lower, Upper, SubString, Now, Trim, SubStringTrim, Date, InsertString, Preprocess };
        public delegate object fDelegate(string FunctionArgs, object input);
        private string _FunctionArgs = string.Empty;
        private DataTable _CachedData;
        private string FieldNameCompare;
        private Type TypeOfFieldNameCompare;
        private Dictionary<int, fDelegate> FunctionTable;

        public DataProcessingFunction()
        {
            FunctionTable = new Dictionary<int, fDelegate>();
            FunctionTable.Add((int)FunctionID.Lookup, (fDelegate)fLookup);
            FunctionTable.Add((int)FunctionID.Translate, (fDelegate)fTranslate);
            FunctionTable.Add((int)FunctionID.Proper, (fDelegate)fProper);
            FunctionTable.Add((int)FunctionID.Lower, (fDelegate)fLower);
            FunctionTable.Add((int)FunctionID.Upper, (fDelegate)fUpper);
            FunctionTable.Add((int)FunctionID.SubString, (fDelegate)fSubString);
            FunctionTable.Add((int)FunctionID.Now, (fDelegate)fNow);
            FunctionTable.Add((int)FunctionID.Trim, (fDelegate)fTrim);
            FunctionTable.Add((int)FunctionID.SubStringTrim, (fDelegate)fSubStringTrim);
            FunctionTable.Add((int)FunctionID.Date, (fDelegate)fDate);
            FunctionTable.Add((int)FunctionID.InsertString, (fDelegate)fInsertString);
            FunctionTable.Add((int)FunctionID.Preprocess, (fDelegate)fPreprocess);
        }

        public object Process(string FunctionIDs, string FunctionArgs, object value)
        {
            int functionID;
            object result = value;
            fDelegate f = null;

            if (Int32.TryParse(FunctionIDs, out functionID))
            {
                if (FunctionTable.TryGetValue(functionID, out f))
                {
                    if (f != null) result = f(FunctionArgs, value);
                    result = result == null ? null : result.ToString();
                }
            }

            return result;
        }

        #region functions

        #region fLookup
        /// <summary>
        /// Build query "select result from tablename" into _CachedData
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public void fLookupPreload(string FunctionArgs)
        {
            if ((_CachedData == null) || (FunctionArgs != null && FunctionArgs.CompareTo(_FunctionArgs) != 0))
            {
                _FunctionArgs = FunctionArgs;

                #region Build query
                Debug.Assert(!string.IsNullOrEmpty(FunctionArgs));
                string[] args = FunctionArgs.Split(',').Select(s => s.Trim()).ToArray();
                Debug.Assert(args.Length == 3);
                string sTable = args[0];
                string sFieldNameCompare = args[1];
                string sFieldNameReturn = args[2];
                string sQuery = string.Format("SELECT {0},{1} FROM {2}", sFieldNameCompare, sFieldNameReturn, sTable);
                #endregion

                #region Connect and get data
                using (SqlConnection connection =
                           new SqlConnection(DataBaseInfo.ConnectionString))
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = sQuery;
                    try
                    {
                        connection.Open();
                        SqlDataAdapter adpt = new SqlDataAdapter(command);
                        _CachedData = new DataTable();
                        adpt.Fill(_CachedData);
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
#if debug
                    MessageBox.Show(ex.Message);
#endif
                    }
                }
                #endregion
            }
        }

        public Type GetType(string sTable, string sField)
        {
            using (SqlConnection connection =
                          new SqlConnection(DataBaseInfo.ConnectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(string.Format("select {0} from {1}", sField, sTable), connection);
                SqlDataReader rdr = cmd.ExecuteReader();
                DataTable schema = rdr.GetSchemaTable();
                var type = schema.Rows.Count == 0 ? null : schema.Rows[0]["DataType"] as Type;
                rdr.Close();
                return type;
            }
        }
        /// <summary>
        /// Check 
        /// if FunctionArgs is loaded -> 
        ///     find in _CachedData
        /// Else
        ///     Build query "select result from tablename where fieldname = value"
        /// </summary>
        /// <param name="arg">LookupName,CompareField,ReturnField, (optional)PreprocessFullCommand</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public object fLookup(string FunctionArgs, object value)
        {

            #region Build query
            Debug.Assert(!string.IsNullOrEmpty(FunctionArgs));
            string[] args = FunctionArgs.Split(',').Select(s => s.Trim()).ToArray();
            //Debug.Assert(args.Length == 3);
            string sTable = args[0];
            string sFieldNameCompare = args[1];
            string sFieldNameReturn = args[2];
            string sQuery = string.Empty;

            #region Preprocess
            string preprocesscode = "Preprocess";
            if (args.Length > 3 && preprocesscode.CompareTo(args[3]) == 0)
            {
                string newArgs = null;

                int pos = FunctionArgs.IndexOf(preprocesscode);
                if (pos >= 0)
                {
                    newArgs = FunctionArgs.Substring(pos);
                    pos = newArgs.IndexOf(',');
                    if (pos >= 0) newArgs = newArgs.Substring(pos + 1);
                }
                if (!string.IsNullOrEmpty(newArgs))
                {
                    value = fPreprocess(newArgs, value);
                }
            }
            #endregion

            #region Init info
            if (FieldNameCompare != sFieldNameCompare || TypeOfFieldNameCompare == null)
            {
                FieldNameCompare = sFieldNameCompare;
                TypeOfFieldNameCompare = GetType(sTable, sFieldNameCompare);
            }
            #endregion
            Debug.Assert(TypeOfFieldNameCompare != null);
            Type type = TypeOfFieldNameCompare;
            if (type == typeof(string))
                sQuery = string.Format("select {0} from {1} where {2} = N'{3}'", sFieldNameReturn, sTable, sFieldNameCompare, value);
            else//Number and other
                sQuery = string.Format("select {0} from {1} where {2} = {3}", sFieldNameReturn, sTable, sFieldNameCompare, value);
            #endregion

            #region Check CachedData
            if (FunctionArgs.CompareTo(_FunctionArgs) == 0 && _CachedData != null)
            {
                string sQueryCache = string.Empty;

                int iValue = 0;
                if (type == typeof(string) || (type == typeof(int) && Int32.TryParse((string)value, out iValue)))
                {
                    if (type == typeof(string))
                        sQueryCache = string.Format("{0} = '{1}'", sFieldNameCompare, value);
                    else//Number and other
                        sQueryCache = string.Format("{0} = {1}", sFieldNameCompare, iValue);

                    DataRow[] rows = _CachedData.Select(sQueryCache);
                    //Debug.Assert(rows.Length <= 1);
                    if (rows.Length >= 1)
                        return rows[0][1];
                }
                //else //run under code
            }
            #endregion
            #region Connect and get data
            object valueReturn = null;
            using (SqlConnection connection =
                       new SqlConnection(DataBaseInfo.ConnectionString))
            {
                SqlCommand command = connection.CreateCommand();
                command.CommandText = sQuery;
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        valueReturn = reader[0];
                        valueReturn = valueReturn == null ? null : valueReturn.ToString();
                        break;//just once
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
#if debug
                MessageBox.Show(ex.Message);
#endif
                }
            }
            #endregion
            return valueReturn;
        }
        #endregion //fLookup

        /// <summary>
        /// fTranslate
        ///     Receive a list of matching pairs in format "String1, 1, String2, 2, Stringn, n"
        ///     Compare the string value in DB with the first value in the list
        ///     Return the matched value in string format, outside function will parse to correct type
        ///     Return null if not found
        /// </summary>
        /// <param name="FunctionArgs"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public object fTranslate(string FunctionArgs, object input)
        {
            object result = null;
            char[] charSeparators = new char[] { ',' };
            string sResult = input != null ? input.ToString() : "NULL";
            string[] listArgs;

            if (FunctionArgs != null)
            {
                listArgs = FunctionArgs.Split(charSeparators, StringSplitOptions.None);
                for (int i = 0; i < listArgs.Length; i += 2)
                {
                    if (sResult == listArgs[i] && listArgs.Length > i + 1)
                    {
                        result = listArgs[i + 1];
                        break;
                    }
                }
            }
            return result;
        }

        public object fProper(string FunctionArgs, object input)
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;

            object result = input != null ? textInfo.ToTitleCase(input.ToString()) : null;
            return result;
        }

        public object fLower(string FunctionArgs, object input)
        {
            object result = input != null ? input.ToString().ToLower() : null;
            return result;
        }

        public object fUpper(string FunctionArgs, object input)
        {
            object result = input != null ? input.ToString().ToUpper() : null;
            return result;
        }

        public object fSubString(string FunctionArgs, object input)
        {
            char[] charSeparators = new char[] { ',' };
            string[] listArgs;
            int startIndex, length;

            object result = null;

            if (FunctionArgs != null)
            {
                listArgs = FunctionArgs.Split(charSeparators, StringSplitOptions.None);
                if (listArgs.Length >= 2 && int.TryParse(listArgs[0], out startIndex) && int.TryParse(listArgs[1], out length))
                {
                    result = input != null ? input.ToString().Substring(startIndex, length) : null;
                }
            }
            return result;
        }

        public object fNow(string FunctionArgs, object input)
        {
            object result = DateTime.Now;
            return result;
        }

        /// <summary>
        /// Trim the input string, return the trimed string 
        /// 1. If FunctionArgs is Null, trim space on both left and right. Else requires 3 characters.
        /// 2. If FunctionArgs[0] is a valid character, trim left this character. If '', skip.
        /// 3. If FunctionArgs[1] is a valid character, trim right this character. If '', skip.
        /// 4. If FunctionArgs[2] is a space ' ', will trim space first before doing both #2, and #3
        /// </summary>
        /// <param name="FunctionArgs"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public object fTrim(string FunctionArgs, object input)
        {
            object result = null;
            if (input == null || string.IsNullOrEmpty((string)input)) return null;

            if (String.IsNullOrEmpty(FunctionArgs))
            {
                result = input.ToString().Trim();
            }
            else
            {
                char[] charSeparators = new char[] { ',' };
                string[] listArgs;
                listArgs = FunctionArgs.Split(charSeparators, StringSplitOptions.None);

                if (listArgs != null && listArgs.Length >= 3)
                {
                    string cLeft = listArgs[0];
                    string cRight = listArgs[1];
                    string cTrim = listArgs[2];
                    string sResult = input.ToString();

                    if (cTrim == " ") sResult = sResult.Trim();
                    if (cLeft.Length >= 1) sResult = sResult.TrimStart(cLeft.ToCharArray());
                    if (cRight.Length >= 1) sResult = sResult.TrimEnd(cRight.ToCharArray());

                    result = sResult;
                }
            }

            return result;
        }

        /// <summary>
        /// Get a substring in string, and Trim 
        /// </summary>
        /// <param name="FunctionArgs">start, length, left, right, trim, toproper</param>
        /// <param name="input"></param>
        /// <returns></returns>
        public object fSubStringTrim(string FunctionArgs, object input)
        {
            object result = null;

            char[] charSeparators = new char[] { ',' };
            string[] listArgs;

            if (FunctionArgs != null)
            {
                listArgs = FunctionArgs.Split(charSeparators, StringSplitOptions.None);
                if (listArgs.Length == 2)
                {
                    result = fSubString(FunctionArgs, input);
                }
                else if (listArgs.Length >= 6)
                {
                    result = fSubString(FunctionArgs, input);
                    result = fTrim(listArgs[2] + "," + listArgs[3] + "," + listArgs[4], result);

                    string toproper = listArgs[5];
                    if (!string.IsNullOrEmpty(toproper))
                    {
                        if (toproper.ToLower() == "proper") result = fProper("", result);
                        if (toproper.ToLower() == "lower") result = fLower("", result);
                        if (toproper.ToLower() == "upper") result = fUpper("", result);
                    }
                }
            }
            return result;
        }

        public object fDate(string FunctionArgs, object input)
        {
            object result = null;
            DateTime date;
            if (input != null && !string.IsNullOrEmpty(input.ToString()))
            {
                if (DateTime.TryParse(input.ToString().Trim(), out date))
                {
                    result = date;
                }
            }
            return result;
        }

        /// <summary>
        /// Insert a string into value string
        /// </summary>
        /// <param name="FunctionArgs">position, stringToBeInserted, (optional) righttoleft</param>
        /// <param name="input"></param>
        /// <returns>updated String</returns>
        public object fInsertString(string FunctionArgs, object input)
        {
            object result = null;
            char[] charSeparators = new char[] { ',' };
            string[] listArgs;

            if (FunctionArgs != null && input != null)
            {
                listArgs = FunctionArgs.Split(charSeparators, StringSplitOptions.None);
                int pos;
                string value = input.ToString();
                string insert;

                result = input;

                if (listArgs.Length >= 2 && int.TryParse(listArgs[0], out pos) && !string.IsNullOrEmpty(listArgs[1]) && !string.IsNullOrEmpty(value))
                {
                    insert = listArgs[1];

                    if (pos >= 0 && pos < value.Length)
                    {
                        // By default, pos is the position counted from left to right
                        // If args[2] is not null, pos is the position counted from left to right
                        if (listArgs.Length >= 3 && string.IsNullOrEmpty(listArgs[2]))
                        {
                            pos = value.Length - pos;
                        }

                        result = value.Substring(0, pos) + insert + value.Substring(pos, value.Length - pos);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Preprocess 
        /// </summary>
        /// <param name="FunctionArgs">functionid, functionargs</param>
        /// <param name="input">value</param>
        /// <returns>preprocessed value</returns>
        public object fPreprocess(string FunctionArgs, object input)
        {
            int functionID;
            fDelegate f = null;

            object result = null;
            char[] charSeparators = new char[] { ',' };
            string[] listArgs;

            if (FunctionArgs != null && input != null)
            {
                listArgs = FunctionArgs.Split(charSeparators, StringSplitOptions.None);

                if (listArgs.Length > 0 && Int32.TryParse(listArgs[0], out functionID))
                {
                    if (FunctionTable.TryGetValue(functionID, out f))
                    {
                        string newFunctionArgs = null;
                        if (listArgs.Length > 2)
                        {
                            int pos = FunctionArgs.IndexOf(',');
                            if (pos >= 0)
                            {
                                newFunctionArgs = FunctionArgs.Substring(pos + 1);
                            }
                        }
                        if (f != null) result = f(newFunctionArgs, input);
                        result = result == null ? null : result.ToString();
                    }
                }
            }
            return result;
        }
        #endregion
    }
}
