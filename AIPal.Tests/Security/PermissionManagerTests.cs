using AIPal.Security;
using System;
using Xunit;

namespace AIPal.Tests.Security
{
    public class PermissionManagerTests
    {
        private readonly PermissionManager _permissionManager;

        public PermissionManagerTests()
        {
            _permissionManager = new PermissionManager();
        }

        [Fact]
        public void ValidateCommand_NullOrEmptyCommand_ReturnsFalse()
        {
            // Arrange
            string? nullCommand = null;
            string emptyCommand = string.Empty;
            string whitespaceCommand = "   ";

            // Act & Assert
            Assert.False(_permissionManager.ValidateCommand(nullCommand));
            Assert.False(_permissionManager.ValidateCommand(emptyCommand));
            Assert.False(_permissionManager.ValidateCommand(whitespaceCommand));
        }

        [Theory]
        [InlineData("rm -rf /")]
        [InlineData("format c:")]
        [InlineData("del /q /f")]
        [InlineData("rd /s /q")]
        [InlineData("shutdown /s")]
        [InlineData("taskkill /f")]
        [InlineData("net user administrator")]
        [InlineData("net localgroup administrators")]
        [InlineData("reg delete HKLM")]
        [InlineData("reg add HKCU")]
        [InlineData("powershell -e")]
        [InlineData("cmd.exe /c")]
        [InlineData("wscript test.vbs")]
        [InlineData("cscript test.js")]
        public void ValidateCommand_DangerousCommands_ReturnsFalse(string dangerousCommand)
        {
            // Act
            bool result = _permissionManager.ValidateCommand(dangerousCommand);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("command1; command2")]
        [InlineData("command1 | command2")]
        [InlineData("command1 & command2")]
        public void ValidateCommand_CommandsWithInjectionAttempts_ReturnsFalse(string commandWithInjection)
        {
            // Act
            bool result = _permissionManager.ValidateCommand(commandWithInjection);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateCommand_CommandTooLong_ReturnsFalse()
        {
            // Arrange
            string longCommand = new string('a', 501);

            // Act
            bool result = _permissionManager.ValidateCommand(longCommand);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("open notepad")]
        [InlineData("volume up 10")]
        [InlineData("launch calculator")]
        [InlineData("settings open")]
        public void ValidateCommand_ValidCommands_ReturnsTrue(string validCommand)
        {
            // Act
            bool result = _permissionManager.ValidateCommand(validCommand);

            // Assert
            Assert.True(result);
        }
    }
}
