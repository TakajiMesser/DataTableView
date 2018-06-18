using Sample.Data;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sample.DataAccessLayer
{
    public static class DBAccess
    {
        // Change with sqlite3_limit(db,SQLITE_LIMIT_VARIABLE_NUMBER,size)
        public const int SQLITE_LIMIT_VARIABLE_NUMBER = 999;

        private static SQLiteConnection _connection = null;
        public static SQLiteConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "datatable-sample.db"));
                }

                return _connection;
            }
        }

        public static int LastInsertRowID => (int)SQLite3.LastInsertRowid(Connection.Handle);

        public static List<Type> EntityTypes => new List<Type>
            {
                typeof(Country),
                typeof(City)
            };

        public static Type ParseTableName(string tableName)
        {
            switch (tableName)
            {
                case "Country":
                    return typeof(Country);
                case "City":
                    return typeof(City);
            }

            throw new ArgumentException("Could not find table name " + tableName);
        }

        public static TableMapping GetMapping<T>() => Connection.GetMapping<T>();

        public static TableMapping GetMapping(Type type) => Connection.GetMapping(type);

        public static void InitializeTables()
        {
            DBTable.Create<Country>(false);
            DBTable.Create<City>(false);
        }

        public static void ResetTables()
        {
            DBTable.Create<Country>(true);
            DBTable.Create<City>(true);
        }

        public static void Execute(string query)
        {
            Connection.Execute(query);
        }
    }
}