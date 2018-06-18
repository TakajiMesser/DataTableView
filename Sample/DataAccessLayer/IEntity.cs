using SQLite;

namespace Sample.DataAccessLayer
{
    public interface IEntity
    {
        [PrimaryKey, AutoIncrement]
        int ID { get; set; }
    }
}
