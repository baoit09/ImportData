using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using ImportData.Helpers;
using System.Windows.Threading;

namespace ImportData
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(currentDomain_UnhandledException);

            // Global exception handling  
            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);
        }

        void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            MessageBox.Show("MyHandler caught : " + e.Message.ToString());

        }

        void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            //Process unhandled exception
            MessageBox.Show(e.Exception.Message.ToString());

            // Prevent default unhandled exception processing
            e.Handled = true;
        }
    }
}
