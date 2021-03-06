﻿using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using SoundByte.UWP.Dialogs;
using SoundByte.UWP.Services;

namespace SoundByte.UWP.Helpers
{
    public static class CrashHelper
    {
        public static void HandleAppCrashes(Application currentApplication)
        {
            // Log when the app crashes
            CoreApplication.UnhandledErrorDetected += async (sender, args) =>
            {
                try
                {
                    args.UnhandledError.Propagate();
                }
                catch (Exception e)
                {
                    await HandleAppCrashAsync(e);
                }
            };

            // Log when the app crashes
            currentApplication.UnhandledException += async (sender, args) =>
            {
                // We have handled this exception
                args.Handled = true;                
                // Show the exception UI
                await HandleAppCrashAsync(args.Exception);
            };

            TaskScheduler.UnobservedTaskException += async (sender, args) =>
            {
                args.SetObserved();

                await HandleAppCrashAsync(args.Exception);
            };

            LoggingService.Log(LoggingService.LogType.Debug, "Now Handling App Crashes");
        }


        private static async Task HandleAppCrashAsync(Exception ex)
        {
            App.Telemetry.TrackException(ex, true);

            try
            {
                if (!DeviceHelper.IsBackground)
                {
                    await NavigationService.Current.CallDialogAsync<CrashDialog>(ex);
                }      
            }
            catch
            {
                // Do nothing
            }
        }
    }
}