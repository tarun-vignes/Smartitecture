using AIPal.Commands;
using AIPal.Security;
using AIPal.Services;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AIPal.Tests.Services
{
    public class CommandMapperTests
    {
        private readonly Mock<IPermissionManager> _mockPermissionManager;
        private readonly Dictionary<string, ISystemCommand> _commandDictionary;
        private readonly CommandMapper _commandMapper;

        public CommandMapperTests()
        {
            // Setup mock permission manager
            _mockPermissionManager = new Mock<IPermissionManager>();
            _mockPermissionManager.Setup(m => m.ValidateCommand(It.IsAny<string>())).Returns(true);

            // Setup mock commands
            var mockLaunchCommand = new Mock<ISystemCommand>();
            mockLaunchCommand.Setup(c => c.Execute(It.IsAny<string>())).Returns(Task.FromResult("Launched application"));

            var mockVolumeCommand = new Mock<ISystemCommand>();
            mockVolumeCommand.Setup(c => c.Execute(It.IsAny<string>())).Returns(Task.FromResult("Volume changed"));

            var mockSettingsCommand = new Mock<ISystemCommand>();
            mockSettingsCommand.Setup(c => c.Execute(It.IsAny<string>())).Returns(Task.FromResult("Settings opened"));

            var mockShutdownCommand = new Mock<ISystemCommand>();
            mockShutdownCommand.Setup(c => c.Execute(It.IsAny<string>())).Returns(Task.FromResult("Shutdown initiated"));

            // Create command dictionary
            _commandDictionary = new Dictionary<string, ISystemCommand>
            {
                { "launch", mockLaunchCommand.Object },
                { "volume", mockVolumeCommand.Object },
                { "settings", mockSettingsCommand.Object },
                { "shutdown", mockShutdownCommand.Object }
            };

            // Create command mapper
            _commandMapper = new CommandMapper(_commandDictionary, _mockPermissionManager.Object);
        }

        [Fact]
        public async Task MapCommand_WithValidLaunchCommand_ReturnsLaunchCommandResult()
        {
            // Arrange
            string input = "launch notepad";

            // Act
            var result = await _commandMapper.MapAndExecuteCommandAsync(input);

            // Assert
            Assert.Equal("Launched application", result);
            _mockPermissionManager.Verify(m => m.ValidateCommand(input), Times.Once);
        }

        [Fact]
        public async Task MapCommand_WithValidVolumeCommand_ReturnsVolumeCommandResult()
        {
            // Arrange
            string input = "volume up 10";

            // Act
            var result = await _commandMapper.MapAndExecuteCommandAsync(input);

            // Assert
            Assert.Equal("Volume changed", result);
            _mockPermissionManager.Verify(m => m.ValidateCommand(input), Times.Once);
        }

        [Fact]
        public async Task MapCommand_WithInvalidCommand_ReturnsErrorMessage()
        {
            // Arrange
            string input = "unknown command";
            _mockPermissionManager.Setup(m => m.ValidateCommand(input)).Returns(true);

            // Act
            var result = await _commandMapper.MapAndExecuteCommandAsync(input);

            // Assert
            Assert.Contains("I don't understand that command", result);
            _mockPermissionManager.Verify(m => m.ValidateCommand(input), Times.Once);
        }

        [Fact]
        public async Task MapCommand_WithInvalidPermission_ReturnsErrorMessage()
        {
            // Arrange
            string input = "launch cmd";
            _mockPermissionManager.Setup(m => m.ValidateCommand(input)).Returns(false);

            // Act
            var result = await _commandMapper.MapAndExecuteCommandAsync(input);

            // Assert
            Assert.Contains("This command is not allowed for security reasons", result);
            _mockPermissionManager.Verify(m => m.ValidateCommand(input), Times.Once);
        }
    }
}
