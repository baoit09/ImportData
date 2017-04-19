using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImportData.Helpers
{
    public class XceedHelper
    {
        public static void Register()
        {
            Xceed.Wpf.DataGrid.Licenser.LicenseKey = "DGP35-MU2N5-LPM88-9K2A";
            Xceed.Wpf.Controls.Licenser.LicenseKey = "DGP35-MU2N5-LPM88-9K2A";
        }
    }
}
