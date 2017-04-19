using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImportData.Helpers
{
    public class MappingFile
    {
        public MappingFile()
        {
            if (Fields == null)
                Fields = new List<FieldInfo>();
        }

        public string TableName { get; set; }
        public List<FieldInfo> Fields { get; set; }

        public FieldInfo GetFieldInfo(string fieldName)
        {
            if (Fields == null)
                return null;

            return Fields.FirstOrDefault(fi => fi.Name == fieldName);
        }
    }
}
