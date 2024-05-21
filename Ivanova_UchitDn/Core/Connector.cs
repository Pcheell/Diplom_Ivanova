using MySqlConnector;
using System;
using System.Threading.Tasks;

namespace Ivanova_UchitDn.Core
{
    class Connector
    {
        // изменить подключение
        private static string
           Link = "Server=195.93.252.96;" +
           "Port=3306;" +
           "UserID=is101_ivanova_diplom;" +
           "Password=vedom_iv!1;" +
           "Database=is101_db_ivanova_diplom;" +
           "CharacterSet=utf8mb4;" +
           "ConvertZeroDatetime=True;" +
           "AllowZeroDatetime=True;";

        public readonly MySqlConnection
           connection = new MySqlConnection(Link);

        public MySqlTransaction Con { get; internal set; }

        //подключение к БД
        public async Task GetOpen()
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки или другое действие
                Console.WriteLine($"Error opening connection: {ex.Message}");
                throw;
            }
        }

        //отключение от БД
        public async Task GetClose()
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Closed)
                {
                    await connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки или другое действие
                Console.WriteLine($"Error closing connection: {ex.Message}");
                throw;
            }
        }

        //возвращение подключения
        public MySqlConnection GetCon()
        {
            return connection;
        }
    }
}
