using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sample.DataAccessLayer
{
    public static class DBTable
    {
        public static IEnumerable<string> GetColumnNames<T>() where T : class, IEntity, new()
        {
            return DBAccess.GetMapping<T>().Columns.Select(c => c.Name);
        }

        public static IEnumerable<string> GetColumnNames(Type type) => DBAccess.GetMapping(type).Columns.Select(c => c.Name);

        public static int GetCount<T>() where T : class, IEntity, new()
        {
            return DBAccess.Connection.Table<T>().Count();
        }

        public static int GetCount(Type type)
        {
            var query = "SELECT count(*) FROM " + type.Name;
            return DBAccess.Connection.ExecuteScalar<int>(query);
        }

        public static void Create<T>(bool overwrite = false) where T : class, IEntity, new()
        {
            if (overwrite)
            {
                DBAccess.Connection.DropTable<T>();
            }

            DBAccess.Connection.CreateTable<T>();
        }

        public static void Create(Type type, bool overwrite = false)
        {
            if (overwrite)
            {
                DBAccess.Connection.DropTable(DBAccess.GetMapping(type));
            }

            DBAccess.Connection.CreateTable(type);
        }

        public static void Insert(object item) => DBAccess.Connection.Insert(item);

        public static void InsertAll(IEnumerable<object> items) => DBAccess.Connection.InsertAll(items);

        public static void Update(object item) => DBAccess.Connection.Update(item);

        public static void UpdateAll(IEnumerable<object> items) => DBAccess.Connection.UpdateAll(items);

        public static T Get<T>(int id) where T : class, IEntity, new()
        {
            return DBAccess.Connection.Get<T>(id);
        }

        public static object Get(Type type, int id) => DBAccess.Connection.Get(id, DBAccess.GetMapping(type));

        public static T GetFirstOrDefault<T>(System.Linq.Expressions.Expression<Func<T, bool>> predExpr) where T : class, IEntity, new()
        {
            return DBAccess.Connection.Table<T>().Where(predExpr).FirstOrDefault();
        }

        public static T Get<T>(System.Linq.Expressions.Expression<Func<T, bool>> predExpr) where T : class, IEntity, new()
        {
            return DBAccess.Connection.Get(predExpr);
        }

        public static TableQuery<T> GetAll<T>() where T : class, IEntity, new()
        {
            return DBAccess.Connection.Table<T>();
        }

        public static List<object> GetAll(Type type)
        {
            var query = "SELECT * FROM " + type.Name;
            return DBAccess.Connection.Query(DBAccess.GetMapping(type), query);
        }

        public static TableQuery<T> GetAll<T>(System.Linq.Expressions.Expression<Func<T, bool>> predExpr) where T : class, IEntity, new()
        {
            return DBAccess.Connection.Table<T>().Where(predExpr);
        }

        public static List<int> GetAllIDs<T>() where T : class, IEntity, new()
        {
            var type = typeof(T);

            var query = "SELECT * FROM " + type.Name;

            var rows = DBAccess.Connection.Query<T>(query);
            return rows.Select(r => r.ID).ToList();
        }

        public static IEnumerable<int> GetAllIDs<T>(string foreignKeyName, IEnumerable<int> foreignKeys) where T : class, IEntity, new()
        {
            var type = typeof(T);

            string query = "SELECT * FROM " + type.Name + " where " + foreignKeyName + " in (" + string.Join(",", foreignKeys) + ")";

            var rows = DBAccess.Connection.Query<T>(query);
            return rows.Select(r => r.ID);
        }

        public static void Delete(int id, Type type)
        {
            var tableMapping = new TableMapping(type);
            DBAccess.Connection.Delete(id, tableMapping);
        }

        public static void Delete(object item) => DBAccess.Connection.Delete(item);

        public static void DeleteAll<T>(IEnumerable<int> ids) where T : class, IEntity, new()
        {
            if (ids.Any())
            {
                var type = typeof(T);

                string query = "DELETE from " + type.Name + " where ID in (" + string.Join(",", ids) + ")";

                DBAccess.Connection.Execute(query);
            }
        }

        public static void DeleteAll(IEnumerable<int> ids, Type type)
        {
            if (ids.Any())
            {
                string query = "DELETE from " + type.Name + " where ID in (" + string.Join(",", ids) + ")";
                DBAccess.Connection.Execute(query);
            }
        }

        public static void DeleteAll<T, U>(string foreignKeyName, IEnumerable<int> foreignKeys) where T : class, IEntity, new() where U : IEntity
        {
            var typeT = typeof(T);
            var typeU = typeof(U);

            string query = "DELETE FROM " + typeT.Name
                + " WHERE " + foreignKeyName + " IN (SELECT ID FROM " + typeU.Name
                + " WHERE ID IN (" + string.Join(",", foreignKeys) + ")"
                + ")";

            DBAccess.Connection.Execute(query);
        }

        public static void InsertUpdateDelete<T>(IEnumerable<T> items) where T : class, IEntity, new()
        {
            var type = typeof(T);

            var updateEntries = new List<T>();
            var insertEntries = new List<T>();
            var deleteIDs = new List<int>();

            var identifierProperty = type.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<IdentifierAttribute>() != null);
            var foreignKeyProperties = type.GetProperties().Where(p => p.GetCustomAttribute<ForeignKeyAttribute>() != null);

            foreach (var item in items)
            {
                var queryBuilder = new StringBuilder("SELECT * FROM " + type.Name);

                var constraints = new List<string>();

                if (identifierProperty != null)
                {
                    constraints.Add(identifierProperty.Name + " = '" + identifierProperty.GetValue(item) + "'");
                }

                foreach (var property in foreignKeyProperties)
                {
                    constraints.Add(property.Name + " = " + (int)property.GetValue(item));
                }

                for (var i = 0; i < constraints.Count; i++)
                {
                    queryBuilder.Append((i == 0) ? " WHERE " : " AND ");
                    queryBuilder.Append(constraints[i]);
                }

                var existingID = DBAccess.Connection.ExecuteScalar<int>(queryBuilder.ToString());
                if (existingID > 0)
                {
                    // Since the item does already exist, this should be an update
                    item.ID = existingID;
                    updateEntries.Add(item);
                }
                else
                {
                    // Since the item does not already exist, this should be an insert
                    insertEntries.Add(item);
                }
            }

            deleteIDs.AddRange(DBAccess.Connection.Table<T>().ToList().Select(t => t.ID).Except(updateEntries.Select(i => i.ID)));

            DBAccess.Connection.UpdateAll(updateEntries);
            DBAccess.Connection.InsertAll(insertEntries);
            DeleteAll<T>(deleteIDs);
        }

        public static void InsertUpdateDelete<T>(IEnumerable<T> items, IEnumerable<T> existingItems) where T : class, IEntity, new()
        {
            var type = typeof(T);

            var updateEntries = new List<T>();
            var insertEntries = new List<T>();
            var deleteIDs = new List<int>();

            var identifierProperty = type.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<IdentifierAttribute>() != null);
            var foreignKeyProperties = type.GetProperties().Where(p => p.GetCustomAttribute<ForeignKeyAttribute>() != null);

            foreach (var item in items)
            {
                var queryBuilder = new StringBuilder("SELECT * FROM " + type.Name);

                var constraints = new List<string>();

                if (identifierProperty != null)
                {
                    constraints.Add(identifierProperty.Name + " = '" + identifierProperty.GetValue(item) + "'");
                }

                foreach (var property in foreignKeyProperties)
                {
                    constraints.Add(property.Name + " = " + (int)property.GetValue(item));
                }

                for (var i = 0; i < constraints.Count; i++)
                {
                    queryBuilder.Append((i == 0) ? " WHERE " : " AND ");
                    queryBuilder.Append(constraints[i]);
                }

                var existingID = DBAccess.Connection.ExecuteScalar<int>(queryBuilder.ToString());
                if (existingID > 0)
                {
                    // Since the item does already exist, this should be an update
                    item.ID = existingID;
                    updateEntries.Add(item);
                }
                else
                {
                    // Since the item does not already exist, this should be an insert
                    insertEntries.Add(item);
                }
            }

            deleteIDs.AddRange(existingItems.Select(t => t.ID).Except(updateEntries.Select(i => i.ID)));

            DBAccess.Connection.UpdateAll(updateEntries);
            DBAccess.Connection.InsertAll(insertEntries);
            DeleteAll<T>(deleteIDs);
        }

        public static void Clear<T>() => DBAccess.Connection.DeleteAll<T>();

        public static void Drop<T>() => DBAccess.Connection.DropTable<T>();
    }
}