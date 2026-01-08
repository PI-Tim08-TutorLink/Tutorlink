using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Implementations;
using TutorLinkApp.Services.Interfaces;

namespace TutorLinkAppTest
{
    public class LoggingTutorServiceDecoratorTests
    {
        private readonly Mock<ITutorService> _mockInnerService;
        private readonly Mock<ILogger> _mockLogger;
        private readonly LoggingTutorServiceDecorator _decorator;

        public LoggingTutorServiceDecoratorTests()
        {
            _mockInnerService = new Mock<ITutorService>();
            _mockLogger = new Mock<ILogger>();
            _decorator = new LoggingTutorServiceDecorator(
                _mockInnerService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var decorator = new LoggingTutorServiceDecorator(
                _mockInnerService.Object,
                _mockLogger.Object
            );

            Assert.NotNull(decorator);
        }
        [Fact]
        public async Task GetAllSkills_LogsInfoMessage()
        {
            var expectedSkills = new List<string> { "Math", "Physics" };
            _mockInnerService
                .Setup(s => s.GetAllSkills())
                .ReturnsAsync(expectedSkills);

            await _decorator.GetAllSkills();

            _mockLogger.Verify(
                l => l.LogInfo("GetAllSkills called"),
                Times.Once
            );
        }

        [Fact]
        public async Task GetAllSkills_CallsInnerService()
        {
            var expectedSkills = new List<string> { "Math", "Physics" };
            _mockInnerService
                .Setup(s => s.GetAllSkills())
                .ReturnsAsync(expectedSkills);

            await _decorator.GetAllSkills();

            _mockInnerService.Verify(
                s => s.GetAllSkills(),
                Times.Once
            );
        }

        [Fact]
        public async Task GetAllSkills_ReturnsResultFromInnerService()
        {
            var expectedSkills = new List<string> { "Math", "Physics", "English" };
            _mockInnerService
                .Setup(s => s.GetAllSkills())
                .ReturnsAsync(expectedSkills);

            var result = await _decorator.GetAllSkills();

            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(expectedSkills, result);
        }

        [Fact]
        public async Task GetAllSkills_LogsBeforeCallingInnerService()
        {
            var callOrder = new List<string>();

            _mockLogger
                .Setup(l => l.LogInfo(It.IsAny<string>()))
                .Callback(() => callOrder.Add("log"));

            _mockInnerService
                .Setup(s => s.GetAllSkills())
                .ReturnsAsync(new List<string>())
                .Callback(() => callOrder.Add("service"));

            await _decorator.GetAllSkills();

            Assert.Equal(2, callOrder.Count);
            Assert.Equal("log", callOrder[0]);
            Assert.Equal("service", callOrder[1]);
        }

        [Fact]
        public async Task GetAllSkills_EmptyList_ReturnsEmptyList()
        {
            _mockInnerService
                .Setup(s => s.GetAllSkills())
                .ReturnsAsync(new List<string>());

            var result = await _decorator.GetAllSkills();

            Assert.NotNull(result);
            Assert.Empty(result);
            _mockLogger.Verify(l => l.LogInfo("GetAllSkills called"), Times.Once);
        }

        [Fact]
        public async Task GetTutorDetails_LogsInfoMessage()
        {
            var tutorId = 1;
            var expectedTutor = new TutorCardViewModel { Id = tutorId };

            _mockInnerService
                .Setup(s => s.GetTutorDetails(tutorId))
                .ReturnsAsync(expectedTutor);

            await _decorator.GetTutorDetails(tutorId);

            _mockLogger.Verify(
                l => l.LogInfo("GetTutorDetails called"),
                Times.Once
            );
        }

        [Fact]
        public async Task GetTutorDetails_CallsInnerServiceWithCorrectId()
        {
            var tutorId = 5;
            _mockInnerService
                .Setup(s => s.GetTutorDetails(tutorId))
                .ReturnsAsync(new TutorCardViewModel { Id = tutorId });

            await _decorator.GetTutorDetails(tutorId);

            _mockInnerService.Verify(
                s => s.GetTutorDetails(tutorId),
                Times.Once
            );
        }

        [Fact]
        public async Task GetTutorDetails_ReturnsResultFromInnerService()
        {
            var tutorId = 1;
            var expectedTutor = new TutorCardViewModel
            {
                Id = tutorId,
                FullName = "John Doe",
                Email = "john@test.com",
                Skills = new List<string> { "Math", "Physics" }
            };

            _mockInnerService
                .Setup(s => s.GetTutorDetails(tutorId))
                .ReturnsAsync(expectedTutor);

            var result = await _decorator.GetTutorDetails(tutorId);

            Assert.NotNull(result);
            Assert.Equal(tutorId, result.Id);
            Assert.Equal("John Doe", result.FullName);
            Assert.Equal("john@test.com", result.Email);
            Assert.Equal(2, result.Skills.Count);
        }

        [Fact]
        public async Task GetTutorDetails_TutorNotFound_ReturnsNull()
        {
            _mockInnerService
                .Setup(s => s.GetTutorDetails(999))
                .ReturnsAsync((TutorCardViewModel?)null);

            var result = await _decorator.GetTutorDetails(999);

            Assert.Null(result);
            _mockLogger.Verify(l => l.LogInfo("GetTutorDetails called"), Times.Once);
        }

        [Fact]
        public async Task GetTutorDetails_LogsBeforeCallingInnerService()
        {
            var callOrder = new List<string>();

            _mockLogger
                .Setup(l => l.LogInfo(It.IsAny<string>()))
                .Callback(() => callOrder.Add("log"));

            _mockInnerService
                .Setup(s => s.GetTutorDetails(It.IsAny<int>()))
                .ReturnsAsync(new TutorCardViewModel())
                .Callback(() => callOrder.Add("service"));

            await _decorator.GetTutorDetails(1);

            Assert.Equal(2, callOrder.Count);
            Assert.Equal("log", callOrder[0]);
            Assert.Equal("service", callOrder[1]);
        }

        [Fact]
        public async Task GetTutorDetails_MultipleIds_LogsForEach()
        {
            _mockInnerService
                .Setup(s => s.GetTutorDetails(It.IsAny<int>()))
                .ReturnsAsync(new TutorCardViewModel());

            await _decorator.GetTutorDetails(1);
            await _decorator.GetTutorDetails(2);
            await _decorator.GetTutorDetails(3);

            _mockLogger.Verify(
                l => l.LogInfo("GetTutorDetails called"),
                Times.Exactly(3)
            );
        }

        [Fact]
        public async Task SearchTutors_LogsInfoMessage()
        {
            var filters = new TutorSearchViewModel { SearchSkill = "Math" };
            var expectedResult = new TutorSearchViewModel
            {
                Tutors = new List<TutorCardViewModel>()
            };

            _mockInnerService
                .Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()))
                .ReturnsAsync(expectedResult);

            await _decorator.SearchTutors(filters);

            _mockLogger.Verify(
                l => l.LogInfo("SearchTutors called"),
                Times.Once
            );
        }

        [Fact]
        public async Task SearchTutors_CallsInnerServiceWithFilters()
        {
            var filters = new TutorSearchViewModel
            {
                SearchSkill = "Math",
                MinPrice = 30,
                MaxPrice = 60
            };

            var expectedResult = new TutorSearchViewModel
            {
                Tutors = new List<TutorCardViewModel>()
            };

            _mockInnerService
                .Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()))
                .ReturnsAsync(expectedResult);

            await _decorator.SearchTutors(filters);

            _mockInnerService.Verify(
                s => s.SearchTutors(It.Is<TutorSearchViewModel>(f =>
                    f.SearchSkill == "Math" &&
                    f.MinPrice == 30 &&
                    f.MaxPrice == 60
                )),
                Times.Once
            );
        }

        [Fact]
        public async Task SearchTutors_ReturnsResultFromInnerService()
        {
            var filters = new TutorSearchViewModel();
            var expectedResult = new TutorSearchViewModel
            {
                Tutors = new List<TutorCardViewModel>
                {
                    new TutorCardViewModel { Id = 1, FullName = "John Doe" },
                    new TutorCardViewModel { Id = 2, FullName = "Jane Smith" }
                }
            };

            _mockInnerService
                .Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()))
                .ReturnsAsync(expectedResult);

            var result = await _decorator.SearchTutors(filters);

            Assert.NotNull(result);
            Assert.Equal(2, result.Tutors.Count);
            Assert.Equal("John Doe", result.Tutors[0].FullName);
            Assert.Equal("Jane Smith", result.Tutors[1].FullName);
        }

        [Fact]
        public async Task SearchTutors_EmptyFilters_CallsInnerService()
        {
            var filters = new TutorSearchViewModel();
            var expectedResult = new TutorSearchViewModel
            {
                Tutors = new List<TutorCardViewModel>()
            };

            _mockInnerService
                .Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()))
                .ReturnsAsync(expectedResult);

            var result = await _decorator.SearchTutors(filters);

            Assert.NotNull(result);
            _mockInnerService.Verify(s => s.SearchTutors(filters), Times.Once);
        }

        [Fact]
        public async Task SearchTutors_LogsBeforeCallingInnerService()
        {
            var callOrder = new List<string>();

            _mockLogger
                .Setup(l => l.LogInfo(It.IsAny<string>()))
                .Callback(() => callOrder.Add("log"));

            _mockInnerService
                .Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()))
                .ReturnsAsync(new TutorSearchViewModel())
                .Callback(() => callOrder.Add("service"));

            await _decorator.SearchTutors(new TutorSearchViewModel());

            Assert.Equal(2, callOrder.Count);
            Assert.Equal("log", callOrder[0]);
            Assert.Equal("service", callOrder[1]);
        }

        [Fact]
        public async Task SearchTutors_NoResults_ReturnsEmptyList()
        {
            var filters = new TutorSearchViewModel { SearchSkill = "NonExistent" };
            var expectedResult = new TutorSearchViewModel
            {
                Tutors = new List<TutorCardViewModel>()
            };

            _mockInnerService
                .Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()))
                .ReturnsAsync(expectedResult);

            var result = await _decorator.SearchTutors(filters);

            Assert.NotNull(result);
            Assert.Empty(result.Tutors);
            _mockLogger.Verify(l => l.LogInfo("SearchTutors called"), Times.Once);
        }

        [Fact]
        public async Task AllMethods_LogCorrectly()
        {
            _mockInnerService
                .Setup(s => s.GetAllSkills())
                .ReturnsAsync(new List<string>());

            _mockInnerService
                .Setup(s => s.GetTutorDetails(It.IsAny<int>()))
                .ReturnsAsync(new TutorCardViewModel());

            _mockInnerService
                .Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()))
                .ReturnsAsync(new TutorSearchViewModel());

            await _decorator.GetAllSkills();
            await _decorator.GetTutorDetails(1);
            await _decorator.SearchTutors(new TutorSearchViewModel());

            _mockLogger.Verify(l => l.LogInfo("GetAllSkills called"), Times.Once);
            _mockLogger.Verify(l => l.LogInfo("GetTutorDetails called"), Times.Once);
            _mockLogger.Verify(l => l.LogInfo("SearchTutors called"), Times.Once);
        }

        [Fact]
        public async Task Decorator_DoesNotModifyResults()
        {
            var skills = new List<string> { "Math", "Physics" };
            var tutor = new TutorCardViewModel { Id = 1, FullName = "John" };
            var searchResult = new TutorSearchViewModel
            {
                Tutors = new List<TutorCardViewModel> { tutor }
            };

            _mockInnerService.Setup(s => s.GetAllSkills()).ReturnsAsync(skills);
            _mockInnerService.Setup(s => s.GetTutorDetails(1)).ReturnsAsync(tutor);
            _mockInnerService.Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>())).ReturnsAsync(searchResult);

            var resultSkills = await _decorator.GetAllSkills();
            var resultTutor = await _decorator.GetTutorDetails(1);
            var resultSearch = await _decorator.SearchTutors(new TutorSearchViewModel());

            Assert.Same(skills, resultSkills);
            Assert.Same(tutor, resultTutor);
            Assert.Same(searchResult, resultSearch);
        }
    }
}

