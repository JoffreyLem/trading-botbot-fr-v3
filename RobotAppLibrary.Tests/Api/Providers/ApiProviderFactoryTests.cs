using FluentAssertions;
using Moq;
using RobotAppLibrary.Api.Providers;
using RobotAppLibrary.Api.Providers.Xtb;
using Serilog;

namespace RobotAppLibrary.Tests.Api.Providers;

public class ApiProviderFactoryTests
{
    [Fact]
    public void GetApiHandler_ShouldReturnXtbApiHandler_WhenApiIsXtb()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var apiProvider = ApiProviderEnum.Xtb;

        // Act
        var result = ApiProviderFactory.GetApiHandler(apiProvider, loggerMock.Object);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<XtbApiProvider>();
    }

    [Fact]
    public void GetApiHandler_ShouldThrowArgumentException_WhenApiIsNotHandled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var apiProvider = (ApiProviderEnum)999; // Unhandled enum value

        // Act
        Action act = () => ApiProviderFactory.GetApiHandler(apiProvider, loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("999 not handled");
    }
}