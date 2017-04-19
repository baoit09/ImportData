using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImportData.Helpers
{
    public class DataBaseInfo
    {
        public static string ConnectionString 
        {
            get {
                return "Data Source=localhost;Initial Catalog=TestDB;Persist Security Info=True;User ID=sa;Password=z";
            }
        }
    }
}
