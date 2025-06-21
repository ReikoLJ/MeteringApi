namespace MeteringApi.Database.Entities
{
    public class Entity_MeterReading
    {
        public int AccountId { get; set; }
        public DateTime ReadingDatetime { get; set; }
        public int ReadValue { get; set; }

        public virtual Entity_Account Account { get; set; } = null!;
    }
}