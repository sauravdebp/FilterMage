﻿using Microsoft.Phone.Info;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace FilterMage.Utils
{
    public class Utilities
    {

        private static Timer timer = null;

        public static void BeginRecording()
        {

            // start a timer to report memory conditions every 3 seconds 
            // 
            timer = new Timer(state =>
            {
                string c = "unassigned";
                try
                {
                    //  
                }
                catch (ArgumentOutOfRangeException ar)
                {
                    var c1 = ar.Message;

                }
                catch
                {
                    c = "unassigned";
                }


                string report = "";
                report += Environment.NewLine +
                   "Current: " + (DeviceStatus.ApplicationCurrentMemoryUsage / 1000000).ToString() + "MB\n" +
                   "Peak: " + (DeviceStatus.ApplicationPeakMemoryUsage / 1000000).ToString() + "MB\n" +
                   "Memory Limit: " + (DeviceStatus.ApplicationMemoryUsageLimit / 1000000).ToString() + "MB\n\n" +
                   "Device Total Memory: " + (DeviceStatus.DeviceTotalMemory / 1000000).ToString() + "MB\n" +
                   "Working Limit: " + Convert.ToInt32((Convert.ToDouble(DeviceExtendedProperties.GetValue("ApplicationWorkingSetLimit")) / 1000000)).ToString() + "MB";

                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Debug.WriteLine(report);
                });

            },
                null,
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(3));
        }
    }
}
