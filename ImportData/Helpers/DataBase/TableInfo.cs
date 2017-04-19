using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace ImportData.Helpers
{
    public class TableInfo
    {
        #region Table Information
        /// <summary>
        /// The name of table in DB
        /// </summary>
        public string Name { get; set; }
        #endregion

        public IList<FieldInfo> GetFieldInfos()
        {
            List<FieldInfo> fieldInfos = new List<FieldInfo>();
            string connectionString = DataBaseInfo.ConnectionString;
            DataTable tables = new DataTable("Tables");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                #region Get all of fields

                SqlCommand command = connection.CreateCommand();
                command.CommandText = string.Format(
                @"SELECT COLUMN_NAME, DATA_TYPE, COLUMN_DEFAULT, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE,NUMERIC_PRECISION, NUMERIC_SCALE, COLUMNPROPERTY(object_id(TABLE_NAME), COLUMN_NAME, 'IsIdentity') as [IS_IDENTITY] FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}'", this.Name);
                connection.Open();
                SqlDataReader dr = command.ExecuteReader();

                
                try
                {
                    while (dr.Read())
                    {
                        FieldInfo fieldInfo = new FieldInfo()
                        {
                            Name = (string)dr["COLUMN_NAME"],
                            DataTypeName = (string)dr["DATA_TYPE"],
                            IsNullable = StringToBoolean((string)dr["IS_NULLABLE"]),
                            IsIdentity = StringToBoolean(dr["IS_IDENTITY"].ToString()),
                            DBDefaulValueOrBinding = DBValueToString(dr["COLUMN_DEFAULT"])
                        };

                        if (fieldInfo.DataTypeName == "decimal")
                        {
                            fieldInfo.MaxLength = string.Format("({0},{1})", dr["NUMERIC_PRECISION"], dr["NUMERIC_SCALE"]);
                        }
                        else if (fieldInfo.DataTypeName.Contains("char"))
                        {
                            object value = dr["CHARACTER_MAXIMUM_LENGTH"];
                            fieldInfo.MaxLength = value != null ? value.ToString() : string.Empty;
                            if (fieldInfo.MaxLength == "-1")
                            {
                                fieldInfo.MaxLength = "MAX";
                            }
                        }

                        fieldInfos.Add(fieldInfo);
                    }

                    dr.Close();
                }
                catch
                {
                    fieldInfos.Clear();
                }
                finally
                {
                    dr.Close();
                }

                #endregion

                #region Get primary key field

                string sPrimaryKeyCol = string.Empty;
                command.CommandText = "sp_pkeys";
                command.CommandType = CommandType.StoredProcedure; 
                command.Parameters.Add("@table_name", SqlDbType.NVarChar).Value = this.Name;
                dr = command.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                {
                    sPrimaryKeyCol = (string)dr["COLUMN_NAME"];
                }
                dr.Close();

                FieldInfo fi = fieldInfos.FirstOrDefault(e => e.Name == sPrimaryKeyCol);
                if (fi != null)
                {
                    fi.IsPrimaryKey = true;
                }

                #endregion

              
            }

            return fieldInfos;
        }

        public static TableInfo[] GetTableInfos()
        {
            List<TableInfo> tableInfos = new List<TableInfo>();
            string connectionString = DataBaseInfo.ConnectionString;
            DataTable tables = new DataTable("Tables");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = connection.CreateCommand();
                command.CommandText = "select table_name as Name from INFORMATION_SCHEMA.Tables where TABLE_TYPE = 'BASE TABLE'";
                connection.Open();
                SqlDataReader dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);

                try
                {
                    while (dataReader.Read())
                    {
                        tableInfos.Add(new TableInfo()
                            {
                                Name = (string)dataReader[0]
                            });
                    }

                    dataReader.Close();
                }
                catch
                {
                    dataReader.Close();
                }
            }

            return tableInfos.OrderBy(ti => ti.Name).ToArray();
        }

        private static string GetRealTypeOfNullable(Type type)
        {
            if (type.Equals(typeof(Nullable<bool>))) return "Boolean";
            else if (type.Equals(typeof(Nullable<byte>))) return "Int8";
            else if (type.Equals(typeof(Nullable<char>))) return "Character";
            else if (type.Equals(typeof(Nullable<decimal>))) return "Decimal";
            else if (type.Equals(typeof(Nullable<double>))) return "Double";
            else if (type.Equals(typeof(Nullable<float>))) return "Float";
            else if (type.Equals(typeof(Nullable<short>))) return "Int16";
            else if (type.Equals(typeof(Nullable<int>))) return "Int32";
            else if (type.Equals(typeof(Nullable<long>))) return "Int64";
            else if (type.Equals(typeof(Nullable<DateTime>))) return "DateTime";
            return string.Empty;
        }

        public static bool StringToBoolean(string sText)
        {
            return !string.IsNullOrEmpty(sText) && (sText.ToLower() == "yes" || sText.ToLower() == "y" || sText == "1");
        }

        public static string DBValueToString(object obj)
        {
            if (obj != null && obj != DBNull.Value)
            {
                return obj.ToString();
            }
            return "NULL";
        }
    }
}
