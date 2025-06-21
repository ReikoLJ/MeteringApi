namespace MeteringApi.Database.Entities
{
    public class Entity_Account
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        public virtual ICollection<Entity_MeterReading> MeterReadings { get; set; } = new List<Entity_MeterReading>();

    }
}