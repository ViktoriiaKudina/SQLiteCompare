using System;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using AutomaticUpdates;

namespace SQLiteTurbo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                _mutex = Mutex.OpenExisting("SQLiteCompare");
                MessageBox.Show("Another instance of SQLiteCompare is already active.\r\n" +
                    "Please close it first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Exception ex)
            {
                _mutex = new Mutex(false, "SQLiteCompare");
            }

            // Configure log4net programmatically (no external XML configuration file)
            ConfigureLogging();

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

            // Issue a log message that contains the version of the application.
            _log.Info("===========================================================================");
            _log.Info(" SQLite Compare [" + Utils.GetSoftwareVersion() + " build " + Utils.GetSoftwareBuild() + "]");
            _log.Info("===========================================================================");

            // Remove any stale table change files
            TableChanges.RemoveStaleChangeFiles();

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                _mainForm = new MainForm();
                Application.Run(_mainForm);
                _mainForm = null;
            }
            catch (Exception ex)
            {
                _mainForm = null;
                _log.Error("Got exception from main loop", ex);
                ShowUnexpectedErrorDialog(ex);
            }
            finally
            {
                // Remove all active change files
                TableChanges.RemoveActiveChangeFiles();
            } // finally

            // If there are pending software updates - apply them now
            UpdateEngine.ApplyPendingUpdates();
        }

        /// <summary>
        /// Configures log4net entirely in code so that the application never loads
        /// (or depends on) an external XML configuration file.
        /// </summary>
        private static void ConfigureLogging()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.Level = Level.Debug;
            hierarchy.Root.AddAppender(new LocalUserFileAppender());
            hierarchy.Configured = true;
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            _log.Error("thread exception", e.Exception);

            // Show error dialog
            ShowUnexpectedErrorDialog(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _log.Error("Unhandled exception ("+(e.IsTerminating?"terminating":"non-terminating")+")", 
                (Exception)e.ExceptionObject);

            ShowUnexpectedErrorDialog((Exception)e.ExceptionObject);
        }

        private static void ShowUnexpectedErrorDialog(Exception error)
        {
            // Prevent multiple unexpected-error-dialogs
            lock (typeof(Program))
            {
                if (_mainForm != null)
                {
                    if (_mainForm.InvokeRequired)
                    {
                        _mainForm.Invoke(new MethodInvoker(delegate
                        {
                            UnexpectedErrorDialog dlg = new UnexpectedErrorDialog();
                            dlg.Error = error;
                            dlg.ShowDialog(_mainForm);
                        }));
                    }
                    else
                    {
                        UnexpectedErrorDialog dlg = new UnexpectedErrorDialog();
                        dlg.Error = error;
                        dlg.ShowDialog(_mainForm);
                    } // else
                }
                else
                {
                    UnexpectedErrorDialog dlg = new UnexpectedErrorDialog();
                    dlg.Error = error;
                    Application.Run(dlg);
                } // else

                Environment.Exit(1);
            } // lock
        }

        private static Mutex _mutex = null;
        private static Form _mainForm = null;
        private static ILog _log = LogManager.GetLogger("Program");
    }
}