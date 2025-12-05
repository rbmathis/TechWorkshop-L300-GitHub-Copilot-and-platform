using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ZavaStorefront.Controllers;
using ZavaStorefront.Services;
using ZavaStorefront.Features;
using ZavaStorefront.Models;
using System.Threading.Tasks;
using System.Text.Json;
using System.Reflection;

namespace ZavaStorefront.Tests.Controllers
{
    public class ChatControllerTests
    {
        private readonly Mock<ILogger<ChatController>> _logger;
        private readonly Mock<IChatService> _chatService;
        private readonly Mock<ISessionManager> _sessionManager;
        private readonly Mock<IFeatureFlagService> _featureFlagService;
        private readonly ChatController _controller;

        public ChatControllerTests()
        {
            _logger = new Mock<ILogger<ChatController>>();
            _chatService = new Mock<IChatService>();
            _sessionManager = new Mock<ISessionManager>();
            _featureFlagService = new Mock<IFeatureFlagService>();
            
            _controller = new ChatController(
                _logger.Object,
                _chatService.Object,
                _sessionManager.Object,
                _featureFlagService.Object);
        }

        private object? GetPropertyValue(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            return property?.GetValue(obj);
        }

        [Fact]
        public async Task Index_ReturnsNotFound_WhenFeatureDisabled()
        {
            // Arrange
            _featureFlagService
                .Setup(f => f.IsFeatureEnabledAsync(FeatureFlags.AiChat))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Index_ReturnsView_WhenFeatureEnabled()
        {
            // Arrange
            _featureFlagService
                .Setup(f => f.IsFeatureEnabledAsync(FeatureFlags.AiChat))
                .ReturnsAsync(true);
            _sessionManager
                .Setup(s => s.GetString("ChatHistory"))
                .Returns((string?)null);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ChatViewModel>(viewResult.Model);
            Assert.NotNull(model.Messages);
        }

        [Fact]
        public async Task SendMessage_ReturnsError_WhenFeatureDisabled()
        {
            // Arrange
            _featureFlagService
                .Setup(f => f.IsFeatureEnabledAsync(FeatureFlags.AiChat))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.SendMessage("Hello");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            var success = GetPropertyValue(jsonResult.Value, "success");
            Assert.NotNull(success);
            Assert.False((bool)success);
        }

        [Fact]
        public async Task SendMessage_ProcessesMessage_WhenFeatureEnabled()
        {
            // Arrange
            _featureFlagService
                .Setup(f => f.IsFeatureEnabledAsync(FeatureFlags.AiChat))
                .ReturnsAsync(true);
            _sessionManager
                .Setup(s => s.GetString("ChatHistory"))
                .Returns((string?)null);
            _chatService
                .Setup(c => c.SendMessageAsync(It.IsAny<string>(), It.IsAny<List<ChatMessage>>()))
                .ReturnsAsync("AI response");

            // Act
            var result = await _controller.SendMessage("Hello");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            var success = GetPropertyValue(jsonResult.Value, "success");
            Assert.NotNull(success);
            Assert.True((bool)success);
        }

        [Fact]
        public async Task SendMessage_ReturnsError_WhenMessageEmpty()
        {
            // Arrange
            _featureFlagService
                .Setup(f => f.IsFeatureEnabledAsync(FeatureFlags.AiChat))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.SendMessage("");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            var success = GetPropertyValue(jsonResult.Value, "success");
            var error = GetPropertyValue(jsonResult.Value, "error");
            Assert.NotNull(success);
            Assert.False((bool)success);
            Assert.NotNull(error);
            Assert.Contains("empty", error.ToString()!, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ClearHistory_ReturnsError_WhenFeatureDisabled()
        {
            // Arrange
            _featureFlagService
                .Setup(f => f.IsFeatureEnabledAsync(FeatureFlags.AiChat))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.ClearHistory();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            var success = GetPropertyValue(jsonResult.Value, "success");
            Assert.NotNull(success);
            Assert.False((bool)success);
        }

        [Fact]
        public async Task ClearHistory_ClearsSession_WhenFeatureEnabled()
        {
            // Arrange
            _featureFlagService
                .Setup(f => f.IsFeatureEnabledAsync(FeatureFlags.AiChat))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.ClearHistory();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            var success = GetPropertyValue(jsonResult.Value, "success");
            Assert.NotNull(success);
            Assert.True((bool)success);
            _sessionManager.Verify(s => s.Remove("ChatHistory"), Times.Once);
        }
    }
}
