using Sample.DataAccessLayer;
using SQLite;

namespace Sample.Data
{
    public class Country : IEntity
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [Identifier]
        public string Name { get; set; }

        [Ignore]
        public TableQuery<City> Cities => DBTable.GetAll<City>(c => c.CountryID == ID);
    }
}
