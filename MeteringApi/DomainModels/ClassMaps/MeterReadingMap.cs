using CsvHelper.Configuration;

namespace MeteringApi.DomainModels.ClassMaps
{
    public sealed class MeterReadingMap : ClassMap<MeterReading>
    {
        public MeterReadingMap()
        {
            Map(m => m.AccountId).Name("AccountId");
            Map(m => m.MeterReadingDateTime)
                .Name("MeterReadingDateTime")
                .TypeConverterOption.Format("dd/MM/yyyy HH:mm");
            Map(m => m.MeterReadValue).Name("MeterReadValue");
        }
    }
}