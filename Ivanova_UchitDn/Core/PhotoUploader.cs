using System;
using System.IO;


namespace Ivanova_UchitDn.Core
{
    public class PhotoUploader
    {
        public static string UploadPhotoAndSavePath(string selectedFilePath)
        {
            try
            {
                // Папка, в которую будем сохранять фотографии
                string destinationFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

                // Если папка не существует, создаем ее
                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                // Имя файла на основе выбранного пути
                string fileName = Path.GetFileName(selectedFilePath);

                // Полный путь к файлу в папке Images
                string destinationFilePath = Path.Combine(destinationFolder, fileName);

                // Копируем выбранный файл в папку Images
                File.Copy(selectedFilePath, destinationFilePath, true);

                // Возвращаем полный путь к файлу
                return destinationFilePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при загрузке фотографии и сохранении пути: " + ex.Message);
                return null;
            }
        }
    }
}