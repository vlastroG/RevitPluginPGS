using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MS.Utilites
{
    internal static class UserInput
    {
        /// <summary>
        /// Возвращает строку от пользователя, введенную в диалоговом окне.
        /// </summary>
        /// <param name="header">Заголовок диалогового окна</param>
        /// <param name="message">Сообщение для пользователя</param>
        /// <param name="defaultValue">Значение по умолчанию</param>
        /// <returns>Строка от пользователя в lower case</returns>
        internal static string GetStringFromUser(string header, string message,  string defaultValue)
        {
            var stringFromUser = Interaction.InputBox(
                message,
                header,
                defaultValue)
                .ToLower();

            return stringFromUser;
        }

        /// <summary>
        /// Возвращает DialogResult от пользователя
        /// </summary>
        /// <param name="header">Заголовок диалогового окна</param>
        /// <param name="message">Сообщение пользователю</param>
        /// <returns>Возвращаемый результат: Yes/No/Cancel</returns>
        internal static DialogResult YesNoCancelInput(string header, string message)
        {
            DialogResult dialogResult = MessageBox.Show(
                message, 
                header, 
                MessageBoxButtons.YesNoCancel);

            return dialogResult;
        }
    }
}
