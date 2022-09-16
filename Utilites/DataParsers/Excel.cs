using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using _Excel = Microsoft.Office.Interop.Excel;

namespace MS.Utilites.DataParsers
{
    public class Excel : IDisposable
    {
        public string Path { get; private set; }
        private readonly _Application excel = new _Excel.Application();
        private readonly Workbook wb;
        private readonly Worksheet ws;

        public Excel(string Path)
        {
            this.Path = Path;
            wb = excel.Workbooks.Open(Path);
            ws = wb.Worksheets[1];
        }

        /// <summary>
        /// Возвращает значение ячейки по номеру строки и столбца. Индексация начинается с 1!
        /// Допускается использовать вместо индекса столбца его название.
        /// </summary>
        /// <param name="row">Номер строки</param>
        /// <param name="column">Номер/название столбца</param>
        /// <returns>Строковое значение ячейки</returns>
        public string ReadCell(int row, object column)
        {
            return ws.Cells[row, column].Value2 ?? String.Empty;
        }

        public void Dispose()
        {
            wb.Close();
            excel.Quit();
        }
    }
}
