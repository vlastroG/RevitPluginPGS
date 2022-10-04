using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace PGSBIM_updater.ViewModels.Base
{
    internal static class UpdaterNotifier
    {
        public static string Status { get; private set; }

        private static readonly string _pathSource =
            @"\\dsm\rvt\!Ресурсы\!PGS-BIM_Plugin\PGS-BIM\RevitPluginPGS\MS.dll";

        private static readonly string _pathDest =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            @"Autodesk\Revit\Addins\2022\RevitPluginPGS\MS.dll");

        private static readonly string _installerPath =
            @"\\dsm\rvt\!Ресурсы\!PGS-BIM_Plugin\Installer\Release\PGSBIM_installer.exe";

        private static readonly string _title = "PGS-BIM обновление";

        //public static void Run(CancellationToken cancellationToken)
        public static void Run()
        {
            //while (!cancellationToken.IsCancellationRequested)
            while (true)
            {
                try
                {
                    //if (!File.Exists(_pathSource))
                    //{
                    //    Status = "Плагин отсутствует на диске";
                    //    System.Windows.MessageBox.Show($"{Status}", _title);
                    //}
                    if (File.Exists(_pathSource))
                    {
                        if (!File.Exists(_pathDest))
                        {
                            Status = "Плагин PGS-BIM отсутствует на данном ПК, установить его сейчас?";
                            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show(
                                Status,
                                _title,
                                MessageBoxButtons.YesNo);
                            if (dialogResult == DialogResult.Yes)
                            {
                                if (File.Exists(_installerPath))
                                    System.Diagnostics.Process.Start(_installerPath);
                            }
                            continue;
                        }
                        else
                        {
                            DateTime dateSource = File.GetLastWriteTime(_pathSource);
                            DateTime dateDest = File.GetLastWriteTime(_pathDest);
                            if (dateSource > dateDest)
                            {
                                Status = "Есть обновление для плагина. Загрузить его сейчас?" +
                                    "\n(Вам нужно будет вручную закрыть Revit перед обновлением)";
                                DialogResult dialogResult = System.Windows.Forms.MessageBox.Show(
                                    Status,
                                    _title,
                                    MessageBoxButtons.YesNo);
                                if (dialogResult == DialogResult.Yes)
                                {
                                    if (File.Exists(_installerPath))
                                        System.Diagnostics.Process.Start(_installerPath);
                                }
                            }
                            //else
                            //{
                            //    Status = "У Вас самая последняя версия плагина";
                            //    System.Windows.MessageBox.Show($"{Status}", _title);
                            //}
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
                Thread.Sleep(3600000);
            }
        }
    }
}