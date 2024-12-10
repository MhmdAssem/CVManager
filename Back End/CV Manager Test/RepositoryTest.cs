using CV_Manager.Domain.Models;
using CV_Manager.Infrastructure.Data;
using CV_Manager.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CV_Manager.Tests.Repositories
{
    [TestFixture]
    public class RepositoryTests
    {
        private CVManagerContext _context;
        private Repository<PersonalInformation> _repository;
        private Mock<ILogger<Repository<PersonalInformation>>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CVManagerContext>()
                .UseInMemoryDatabase(databaseName: "TestDB")
                .Options;

            _context = new CVManagerContext(options);
            _mockLogger = new Mock<ILogger<Repository<PersonalInformation>>>();
            _repository = new Repository<PersonalInformation>(_context, _mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }

        [Test]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            // Act
            var actualEntity = await _repository.GetByIdAsync(1);

            // Assert
            actualEntity.Should().BeNull();
        }

        [Test]
        public async Task DeleteAsync_WithNonExistingId_DoesNotThrowException()
        {
            // Act & Assert
            await _repository.Invoking(r => r.DeleteAsync(1)).Should().NotThrowAsync();
        }

        [Test]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            // Act 
            var exists = await _repository.ExistsAsync(1);

            // Assert
            exists.Should().BeFalse();
        }
    }
}