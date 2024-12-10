using NUnit.Framework;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using CV_Manager.Controllers;
using CV_Manager.Domain.Interfaces;
using CV_Manager.Domain.Models;
using CV_Manager.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using CV_Manager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.DependencyInjection;
using CV_Manager.Infrastructure.Data;

namespace CV_Manager_Test
{
    [TestFixture]
    public class CVControllerTests
    {
        private Mock<IServiceScopeFactory> _mockScopeFactory;
        private Mock<IMapper> _mockMapper;
        private Mock<ILogger<CVController>> _mockLogger;
        private CVController _controller;

        [SetUp]
        public void Setup()
        {
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<CVController>>();
            _controller = new CVController(_mockScopeFactory.Object, _mockMapper.Object, _mockLogger.Object);
        }

        [Test]
        public async Task GetAllCVs_ReturnsOkResult()
        {
            // Arrange
            var mockCVRepo = new Mock<ICVRepository>();
            mockCVRepo.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(GetTestCVs());

            var mockPersonalRepo = new Mock<IRepository<PersonalInformation>>();
            mockPersonalRepo.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(GetTestPersonalInfo());

            _mockScopeFactory.Setup(factory => factory.CreateScope().ServiceProvider)
                .Returns(GetServiceProviderMock(mockCVRepo, mockPersonalRepo).Object);

            _mockMapper.Setup(mapper => mapper.Map<CVDTO>(It.IsAny<CV>()))
                .Returns(new CVDTO());
            _mockMapper.Setup(mapper => mapper.Map<PersonalInformationDTO>(It.IsAny<PersonalInformation>()))
                .Returns(new PersonalInformationDTO());
            _mockMapper.Setup(mapper => mapper.Map<List<ExperienceInformationDTO>>(It.IsAny<List<ExperienceInformation>>()))
                .Returns(new List<ExperienceInformationDTO>());

            // Act
            var result = await _controller.GetAllCVs();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public async Task GetCV_WithValidId_ReturnsOkResult()
        {
            // Arrange
            int testId = 1;
            var mockCVRepo = new Mock<ICVRepository>();
            mockCVRepo.Setup(repo => repo.GetByIdAsync(testId))
                .ReturnsAsync(GetTestCVs().FirstOrDefault(cv => cv.Id == testId));

            var mockPersonalRepo = new Mock<IRepository<PersonalInformation>>();
            mockPersonalRepo.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(GetTestPersonalInfo());

            _mockScopeFactory.Setup(factory => factory.CreateScope().ServiceProvider)
                .Returns(GetServiceProviderMock(mockCVRepo, mockPersonalRepo).Object);

            _mockMapper.Setup(mapper => mapper.Map<CVDTO>(It.IsAny<CV>()))
                .Returns(new CVDTO());
            _mockMapper.Setup(mapper => mapper.Map<PersonalInformationDTO>(It.IsAny<PersonalInformation>()))
                .Returns(new PersonalInformationDTO());
            _mockMapper.Setup(mapper => mapper.Map<List<ExperienceInformationDTO>>(It.IsAny<List<ExperienceInformation>>()))
                .Returns(new List<ExperienceInformationDTO>());

            // Act
            var result = await _controller.GetCV(testId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public async Task GetCV_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            int testId = 10;
            var mockCVRepo = new Mock<ICVRepository>();

            _mockScopeFactory.Setup(factory => factory.CreateScope().ServiceProvider)
                .Returns(GetServiceProviderMock(mockCVRepo, null).Object);

            // Act
            var result = await _controller.GetCV(testId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
        }

        // More tests for other controller actions...

        private List<CV> GetTestCVs()
        {
            var cvs = new List<CV>
            {
                new CV { Id = 1, Personal_Information_Id = 1 },
                new CV { Id = 2, Personal_Information_Id = 2 },
                new CV { Id = 3, Personal_Information_Id = 3 }
            };
            return cvs;
        }

        private PersonalInformation GetTestPersonalInfo()
        {
            return new PersonalInformation { Id = 1 };
        }

        private Mock<IServiceProvider> GetServiceProviderMock(Mock<ICVRepository> mockCVRepo, Mock<IRepository<PersonalInformation>> mockPersonalRepo)
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(provider => provider.GetService(typeof(ICVRepository)))
                .Returns(mockCVRepo.Object);

            if (mockPersonalRepo != null)
            {
                mockServiceProvider.Setup(provider => provider.GetService(typeof(IRepository<PersonalInformation>)))
                    .Returns(mockPersonalRepo.Object);
            }

            return mockServiceProvider;
        }
    }

    internal static class DbContextMock
    {
        public static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => sourceList.Add(s));

            return dbSet.Object;
        }
    }
}