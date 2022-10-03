using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PGSBIM_updater.ViewModels.Base
{
    internal static class UpdaterNotifier
    {
        public static string Status { get; private set; }

        private static readonly string _pathDest = @"C:\Users\stroganov.vg\Documents\TEST\test.txt";
        private static readonly string _pathSource = @"C:\Users\stroganov.vg\Documents\TEST_source\test.txt";


        private static readonly string _pathTestOut = @"C:\Users\stroganov.vg\Documents\TEST_source\testOut.txt";


        //public static void Run(CancellationToken cancellationToken)
        public static void Run()
        {

            //while (!cancellationToken.IsCancellationRequested)
            while (true)
            {
                Thread.Sleep(30000);
                if (!File.Exists(_pathSource))
                {
                    Status = "Source отсутствует";
                    MessageBox.Show($"{Status}", "PGS-BIM обновление");

                }
                else
                {
                    DateTime dateSource = File.GetLastWriteTime(_pathSource);
                    DateTime dateDest = File.GetLastWriteTime(_pathDest);
                    if (dateSource > dateDest)
                    {
                        Status = "Есть обновление для плагина. Можете закрыть Revit и обновить плагин вручную.";
                        MessageBox.Show($"{Status}", "PGS-BIM обновление");
                    }
                    else
                    {
                        Status = "У Вас самая последняя версия плагина";
                        MessageBox.Show($"{Status}", "PGS-BIM обновление");

                    }
                }
                TestWriteOut();
            }
        }

        private static void TestWriteOut()
        {
            File.AppendAllText(_pathTestOut, Status);
        }
    }
}
