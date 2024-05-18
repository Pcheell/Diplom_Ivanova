using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivanova_UchitDn.Core
{
    class Connector
    {
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
            await connection.OpenAsync();
        }

        //отключение от БД
        public async Task GetClose()
        {
            await connection.CloseAsync();
        }

        //возвращение подключения
        public MySqlConnection GetCon()
        {
            return connection;
        }
    }
}
