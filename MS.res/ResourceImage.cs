using System.Windows.Media.Imaging;

namespace MS.res
{
    /// <summary>
    /// Встроенные изображения из сборки на основе входного имени с расширением.
    /// Вспомогательные методы.
    /// </summary>
    public class ResourceImage
    {
        /// <summary>
        /// Возвращает иконку из ResourceAssembly
        /// </summary>
        /// <param name="name">Название изображения</param>
        /// <returns>Icon</returns>
        public static BitmapImage GetIcon(string name)
        {
            // Create resource reader stream
            var stream = ResourceAssembly.GetAssembly().GetManifestResourceStream(ResourceAssembly.GetNamespace() + "Images.Icons." + name);

            var image = new BitmapImage();

            // Construct and return image.
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();

            //Return constructed BitmapImage.
            return image;
        }
    }
}
