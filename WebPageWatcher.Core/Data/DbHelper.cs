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
    public static class DbHelper
    {
        public static string DbPath => Path.Combine(Config.DataPath, "db.db");
        public const string WebPagesTableName = "WebPages";
        public const string ScriptsTableName = "Scripts";
        public const string TriggersTableName = "Triggers";
        public const string WebPageUpdatesTableName = "WebPageUpdates";
        public const string LogTableName = "Logs";
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
                    using var fzDb = SQLiteDatabaseHelper.OpenOrCreate(DbPath);
                    fzDb.CreateTable(WebPagesTableName, "ID",
                       new SQLiteColumn(nameof(WebPage.Name), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(WebPage.Url), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(WebPage.Enabled), SQLiteDataType.Integer),
                       new SQLiteColumn(nameof(WebPage.LastUpdateTime), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(WebPage.LastCheckTime), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(WebPage.Interval), SQLiteDataType.Integer),
                       //new SQLiteColumn(nameof(WebPage.LatestContent), SQLiteDataType.Blob),
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
                       new SQLiteColumn(nameof(WebPage.Request_Expect100Continue), SQLiteDataType.Integer),
                       new SQLiteColumn(nameof(WebPage.Request_KeepAlive), SQLiteDataType.Integer),
                       new SQLiteColumn(nameof(WebPage.Request_AllowAutoRedirect), SQLiteDataType.Integer),

                       new SQLiteColumn(nameof(WebPage.Response_Type), SQLiteDataType.Integer)
                       );
                    fzDb.CreateTable(ScriptsTableName, "ID",
                       new SQLiteColumn(nameof(Script.Name), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(Script.Code), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(Script.Enabled), SQLiteDataType.Integer),
                       new SQLiteColumn(nameof(Script.Interval), SQLiteDataType.Integer),
                       new SQLiteColumn(nameof(Script.LastExcuteTime), SQLiteDataType.Text)
                       );
                    fzDb.CreateTable(WebPageUpdatesTableName, "ID",
                       new SQLiteColumn(nameof(WebPageUpdate.WebPage_ID), SQLiteDataType.Integer),
                       new SQLiteColumn(nameof(WebPageUpdate.Time), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(WebPageUpdate.Content), SQLiteDataType.Blob)
                       );
                    fzDb.CreateTable(TriggersTableName, "ID",
                       new SQLiteColumn(nameof(Trigger.Name), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(Trigger.Enabled), SQLiteDataType.Integer),
                       new SQLiteColumn(nameof(Trigger.LastExcuteTime), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(Trigger.Event), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(Trigger.Event_ID), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(Trigger.Operation), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(Trigger.Operation_ID), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(Trigger.Operation_Command), SQLiteDataType.Text)
                       ); 
                    fzDb.CreateTable(LogTableName, "ID",
                       new SQLiteColumn(nameof(Log.Type), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(Log.Message), SQLiteDataType.Text),
                       new SQLiteColumn(nameof(Log.Item_ID), SQLiteDataType.Integer),
                       new SQLiteColumn(nameof(Log.Time), SQLiteDataType.Text)
                       );
                }
            }

            db = new SQLiteConnection($"data source = {DbPath};version=3;");
        }

        public static void Export(string targetPath)
        {
            EnsureDb();
            Dispose();
            File.Copy(DbPath, targetPath);
        }       
        public static void Import(string sourcePath)
        {
            Dispose();
            File.Copy(sourcePath, DbPath,true);
            EnsureDb();
        }
        public async static Task<IEnumerable<WebPage>> GetWebPagesAsync()
        {
            EnsureDb();
            return await db.QueryAsync<WebPage>($"select * from {WebPagesTableName}");
        }
        public async static Task<IEnumerable<WebPageUpdate>> GetWebPageUpdatesAsync(WebPage webPage)
        {
            EnsureDb();
            return await db.QueryAsync<WebPageUpdate>($"select * from {WebPageUpdatesTableName} where {nameof(WebPageUpdate.WebPage_ID)}={webPage.ID}");
        }
        public async static Task<IEnumerable<WebPageUpdate>> GetWebPageUpdatesAsync()
        {
            EnsureDb();
            return await db.QueryAsync<WebPageUpdate>($"select * from {WebPageUpdatesTableName} ");
        }
        public static async Task<byte[]> GetLatestContentAsync(this WebPage webPage)
        {
            WebPageUpdate update = await GetLatestUpdateAsync(webPage);
            if (update == null || update.Content == null || update.Content.Length == 0)
            {
                return null;
            }
            return update.Content;
        }
        public static byte[] GetLatestContent(this WebPage webPage)
        {
            WebPageUpdate update = GetLatestUpdate(webPage);
            if (update == null || update.Content == null || update.Content.Length == 0)
            {
                return null;
            }
            return update.Content;
        }

        public static WebPageUpdate GetLatestUpdate(WebPage webPage)
        {
            EnsureDb();
            return db.QueryFirstOrDefault<WebPageUpdate>($"select * from {WebPageUpdatesTableName} where {nameof(WebPageUpdate.WebPage_ID)}={webPage.ID} order by ID desc limit 1");
        }
        public async static Task<WebPageUpdate> GetLatestUpdateAsync(WebPage webPage)
        {
            EnsureDb();
            return await db.QueryFirstOrDefaultAsync<WebPageUpdate>($"select * from {WebPageUpdatesTableName} where {nameof(WebPageUpdate.WebPage_ID)}={webPage.ID} order by ID desc limit 1");
        }
        public async static Task<IEnumerable<Trigger>> GetTriggersAsync()
        {
            EnsureDb();
            return await db.QueryAsync<Trigger>($"select * from {TriggersTableName}");
        }
        public async static Task<IEnumerable<Script>> GetScriptsAsync()
        {
            EnsureDb();
            return await db.QueryAsync<Script>($"select * from {ScriptsTableName}");
        }

        public async static Task<T> InsertAsync<T>() where T : class, IDbModel, new()
        {
            EnsureDb();
            var item = new T();
            await db.InsertAsync(item);
            return item;
        }
        public async static Task<T> InsertAsync<T>(T item) where T : class, IDbModel, new()
        {
            EnsureDb();
            await db.InsertAsync(item);
            return item;
        } 
        public async static Task AddLogAsync(Log log) 
        {
            EnsureDb();
            await db.InsertAsync(log);
        }

        public async static Task UpdateAsync<T>(T item) where T : class, IDbModel, new()
        {
            EnsureDb();
            await db.UpdateAsync(item);
        }
        public async static Task<T> CloneAsync<T>(T item) where T : class, IDbModel, new()
        {
            EnsureDb();
            item = item.Clone() as T;
            await db.InsertAsync(item);
            return item;
        }
        public async static Task DeleteAsync<T>(T item) where T : class, IDbModel, new()
        {
            EnsureDb();
            await db.DeleteAsync(item);
        }

        public static void Dispose()
        {
            if (db != null)
            {
                db.Dispose();
                db = null;
            }
        }
    }

}
