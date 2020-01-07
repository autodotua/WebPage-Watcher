using Dapper;
using Dapper.Contrib.Extensions;
using FzLib.DataStorage.SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WebPageWatcher.Data
{
    public class DbHelper
    {
        public static string DbPath =>Path.Combine(Config.DataPath,"db.db");
        public const string WebPagesTableName = "WebPages";
        public const string CookiesTableName = "Cookies";
        private static IDbConnection db;


        private static void EnsureDb()
        {
            if (db == null)
            {
                if (!File.Exists(DbPath))
                {

                    if (!Directory.Exists(Config.DataPath))
                    {
                        Directory.CreateDirectory(Config.DataPath);
                    }
                    var fzDb = SQLiteDatabaseHelper.OpenOrCreate(DbPath);
                    fzDb.CreateTable(WebPagesTableName, "ID",
                       new SQLiteColumn(nameof(WebPage.Name), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(WebPage.Url), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(WebPage.LastUpdateTime), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(WebPage.LastCheckTime), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(WebPage.Interval), SQLiteDataType.Integer),
                       new SQLiteColumn(nameof(WebPage.LatestDocument), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(WebPage.BlackWhiteListJson), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(WebPage.BlackWhiteListMode), SQLiteDataType.Integer),
                       new SQLiteColumn(nameof(WebPage.InnerTextOnly), SQLiteDataType.Integer),
                       new SQLiteColumn(nameof(WebPage.IgnoreWhiteSpace), SQLiteDataType.Integer),

                       new SQLiteColumn(nameof(WebPage.Request_CookiesJson), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(WebPage.Request_Accept), SQLiteDataType.Integer),
                       new SQLiteColumn(nameof(WebPage.Request_Method), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(WebPage.Request_Origin), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(WebPage.Request_Referer), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(WebPage.Request_ContentType), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(WebPage.Request_UserAgent), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(WebPage.Request_Body), SQLiteDataType.Text),

                       new SQLiteColumn(nameof(WebPage.Response_Type), SQLiteDataType.Text)
                       );
                }
            }

            db = new SQLiteConnection($"data source = {DbPath};version=3;");
        }





        public async static Task<IEnumerable<WebPage>> GetWebPagesAsync()
        {
            EnsureDb();
            return await db.QueryAsync<WebPage>($"select * from {WebPagesTableName}");
        }

        public async static Task<WebPage> AddWebPageAsync()
        {
            string a = nameof(WebPage.Name);
            EnsureDb();
            var webpage = new WebPage();
            //string sql = $"insert into {WebPagesTableName}(Name) " +
            //    $"values(@Name)";
            //await db.ExecuteAsync(sql, webpage);
            int id = await db.InsertAsync(webpage);
            webpage.ID = id;
            return webpage;
        }

        public async static Task UpdateAsync(WebPage webPage)
        {
            EnsureDb();
            await db.UpdateAsync(webPage);
        }
        public async static Task DeleteAsync(WebPage webPage)
        {
            EnsureDb();
            await db.DeleteAsync(webPage);
        }

        public static void Dispose()
        {
            if (db != null)
            {
                db.Dispose();
            }
        }
    }

}
