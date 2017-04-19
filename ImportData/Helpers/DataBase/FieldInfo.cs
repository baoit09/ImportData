using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ImportData.Helpers
{
    public class FieldInfo
    {
        public string DataTypeName { get; set; }
        public bool IsNullable { get; set; }
        public string MaxLength { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsAutoIncrement { get; set; }
        public bool IsIdentity { get; set; }
        public bool ReplaceEmptyByDefaultValue { get; set; }
        public bool ReplaceEmptyByDBDefaultValue { get; set; }
        public string DBDefaulValueOrBinding { get; set; }

        [XmlIgnore]
        public Type DataType { get; set; }

        public bool IsUnique { get; set; }
        public string Name { get; set; }
        public int? ExcelColumnIndex { get; set; }
        public string DefaultValue { get; set; }
        public string FunctionIDs { get; set; }
        public string FunctionArgs { get; set; }

        public FieldInfo()
        {
            ReplaceEmptyByDefaultValue = true;
            ReplaceEmptyByDBDefaultValue = true;
        }

        /// <summary>
        /// Check if this field need to be used to import.
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool IsSelectedField(FieldInfo field)
        {
            return field != null
                && !field.DataTypeName.ToLower().Equals("byte[]")
                &&
                (
                    field.ExcelColumnIndex.HasValue && field.ExcelColumnIndex.Value > 0 // Is mapped to any excel column.
                    || !string.IsNullOrEmpty(field.DefaultValue) // Has default value.
                    || !string.IsNullOrEmpty(field.FunctionIDs) && field.FunctionIDs != "0"  // Has function.
                );
        }

        /// <summary>
        /// Check if this field has function.
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool HasFunctionField(FieldInfo field)
        {
            return field != null
                && !field.DataTypeName.ToLower().Equals("byte[]")
                &&
                (
                    !string.IsNullOrEmpty(field.FunctionIDs) && field.FunctionIDs != "0"
                );
        }

        public string GetDBDefaulValueOrBinding()
        {
            if (!string.IsNullOrEmpty(DBDefaulValueOrBinding))
            {
                if (this.DataTypeName.Contains("char"))
                {
                    return DBDefaulValueOrBinding.Replace("(N'", string.Empty).Replace("')", string.Empty);
                }
                else
                {
                    return DBDefaulValueOrBinding.Replace("((", string.Empty).Replace("))", string.Empty);
                }
            }
            return null;
        }
    }
}
