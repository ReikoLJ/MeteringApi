using System.Text;
using FluentAssertions;
using MeteringApi.Database.Entities;
using MeteringApi.Database.Interfaces;
using MeteringApi.Services;
using Microsoft.AspNetCore.Http;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace MeteringApi.UnitTests.Services
{
    [TestFixture]
    public class MeteringServiceTests
    {
        private IMeteringDbContext _meteringDbContext;
        private MeteringService _target;

        private List<Entity_Account> _accountList;
        private List<Entity_MeterReading> _meterReadingList;

        [SetUp]
        public void Setup()
        {
            _meteringDbContext = Substitute.For<IMeteringDbContext>();
            _target = new MeteringService(_meteringDbContext);

            _accountList = [];
            _meterReadingList = [];

            var accountListDbSet = _accountList.AsQueryable().BuildMockDbSet();
            var meterReadingListDbSet = _meterReadingList.AsQueryable().BuildMockDbSet();

            _meteringDbContext.Accounts.Returns(accountListDbSet);
            _meteringDbContext.MeterReadings.Returns(meterReadingListDbSet);
        }

        [Test]
        public async Task SaveMeterReadingsAsync_SavesOnlyValidRecords()
        {
            AddAccount(1111);
            AddAccount(2222);
            AddAccount(3333);

            var csvContent = new StringBuilder();
            csvContent.AppendLine("AccountId,MeterReadingDateTime,MeterReadValue");
            csvContent.AppendLine("1111,21/06/2025 09:50,1002");
            csvContent.AppendLine("2222,25/04/2025 12:25,0323");
            csvContent.AppendLine("3333,20/04/2025 15:25,3440");
            csvContent.AppendLine("4444,01/04/2025 12:25,1234");    // Invalid account
            csvContent.AppendLine("3333,01/04/2025 12:25,9999999"); // Invalid value
            csvContent.AppendLine("1111,21/06/2025 09:50,1002");    // Duplicate reading

            var testCsv = GetMockCsv(csvContent);

            var result = await _target.SaveMeterReadingsAsync(testCsv);
            result.SuccessCount.Should().Be(3);
            result.FailureCount.Should().Be(3);
        }

        [Test]
        public async Task SaveMeterReadingsAsync_WhenAccountIdInvalid_ReturnsFailure()
        {
            // Arrange
            AddAccount(1111);

            var csvContent = new StringBuilder();
            csvContent.AppendLine("AccountId,MeterReadingDateTime,MeterReadValue");
            csvContent.AppendLine("2222,25/04/2025 12:25,0323");

            var testCsv = GetMockCsv(csvContent);
            
            // Act
            var result = await _target.SaveMeterReadingsAsync(testCsv);

            // Assert
            result.FailureCount.Should().Be(1);
            result.Errors.Single().Should().Contain("Invalid AccountId");
        }

        [Test]
        [TestCase(99999999)]
        [TestCase(-1)]
        [TestCase(123456)]
        public async Task SaveMeterReadingsAsync_WhenReadValueIsInvalid_ReturnsFailure(int readValue)
        {
            // Arrange
            var accountId = 1111;
            AddAccount(accountId);

            var csvContent = new StringBuilder();
            csvContent.AppendLine("AccountId,MeterReadingDateTime,MeterReadValue");
            csvContent.AppendLine($"{accountId},25/04/2025 12:25,{readValue}");

            var testCsv = GetMockCsv(csvContent);

            // Act
            var result = await _target.SaveMeterReadingsAsync(testCsv);

            // Assert
            result.FailureCount.Should().Be(1);
            result.Errors.Single().Should().Contain("Invalid meter read value");
        }

        [Test]
        public async Task SaveMeterReadingsAsync_WhenDuplicateInCSV_ReturnsFailure()
        {
            // Arrange
            AddAccount(1111);

            var csvContent = new StringBuilder();
            csvContent.AppendLine("AccountId,MeterReadingDateTime,MeterReadValue");
            csvContent.AppendLine("1111,25/04/2025 12:25,0323");
            csvContent.AppendLine("1111,25/04/2025 12:25,0550");

            var testCsv = GetMockCsv(csvContent);

            // Act
            var result = await _target.SaveMeterReadingsAsync(testCsv);

            // Assert
            result.FailureCount.Should().Be(1);
            result.Errors.Single().Should().Contain($"Duplicate entry");
        }

        [Test]
        public async Task SaveMeterReadingsAsync_WhenCSVInvalid_ThrowsException()
        {
            // Arrange
            var csvContent = new StringBuilder();
            csvContent.AppendLine("AccountId,MeterReadingDateTime,UnexpectedText");

            var testCsv = GetMockCsv(csvContent);

            // Act
            // Assert
            await _target.Invoking(x => x.SaveMeterReadingsAsync(testCsv))
                .Should().ThrowAsync<InvalidDataException>()
                .WithMessage("CSV could not be parsed, please check required format in documentation");
        }

        #region Private methods

        private void AddAccount(int accountId)
        {
            var entity = new Entity_Account
            {
                Id = accountId
            };

            _accountList.Add(entity);
        }

        private void AddMeterReading(int accountId, DateTime readingDate, int readValue)
        {
            var entity = new Entity_MeterReading
            {
                AccountId = accountId,
                ReadingDatetime = readingDate,
                ReadValue = readValue
            };

            _meterReadingList.Add(entity);
        }

        private IFormFile GetMockCsv(StringBuilder stringBuilder)
        {


            var bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
            var stream = new MemoryStream(bytes);

            return new FormFile(stream, 0, bytes.Length, name: "file", fileName: "test.csv")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/csv"
            };
        }

        #endregion
    }
}