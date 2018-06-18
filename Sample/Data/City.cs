using Sample.DataAccessLayer;
using SQLite;

namespace Sample.Data
{
    public class City : IEntity
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [Identifier]
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [ForeignKey(typeof(Country))]
        public int CountryID { get; set; }

        [Ignore]
        public Country Country => DBTable.Get<Country>(CountryID);
    }
}
