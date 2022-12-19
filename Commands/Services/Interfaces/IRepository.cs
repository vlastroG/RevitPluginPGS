using MS.Commands.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.Services.Interfaces
{
    /// <summary>
    /// Репозиторий для хранения сущностей, реализующих интерфейс <see cref="IEntity"/>
    /// </summary>
    /// <typeparam name="T">Тип объектов, хранимых в репозитории</typeparam>
    public interface IRepository<T> where T : IEntity
    {
        /// <summary>
        /// Добавить элемент в репозиторий
        /// </summary>
        /// <param name="item">Добавляемый объект</param>
        /// <returns>True, если добавление произошло успешно, иначе false</returns>
        bool Add(T item);

        /// <summary>
        /// Получить перечисление всех элементов репозитория
        /// </summary>
        /// <returns>Перечисление всех объектов репозитория</returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Получить объект из репозитория по идентификатору
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Найденный объект</returns>
        T Get(int id);

        /// <summary>
        /// Удалить обхект из репозитория
        /// </summary>
        /// <param name="item">Объект для удаления</param>
        /// <returns>True, если удаление прошло успешно, иначе false</returns>
        bool Delete(T item);

        /// <summary>
        /// Обновить объект в репозитории
        /// </summary>
        /// <param name="id">Идентификатор существующего объекта в репозитории</param>
        /// <param name="item">Новый объект в репозитории</param>
        /// <returns>True, если обновление произошло успешно, иначе false</returns>
        bool Update(int id, T item);
    }
}
