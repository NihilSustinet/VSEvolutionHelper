using Xunit;
using VSItemTooltips.Core.Services;
using VSEvolutionHelper.Tests.Mocks;

namespace VSEvolutionHelper.Tests.Services
{
    /// <summary>
    /// Tests for the EvolutionCalculator service.
    /// Demonstrates how to test business logic in isolation from Unity/IL2CPP.
    /// </summary>
    public class EvolutionCalculatorTests
    {
        private readonly MockGameDataProvider _gameData;
        private readonly MockPlayerStateProvider _playerState;
        private readonly MockLogger _logger;
        private readonly EvolutionCalculator _calculator;

        public EvolutionCalculatorTests()
        {
            _gameData = new MockGameDataProvider();
            _playerState = new MockPlayerStateProvider();
            _logger = new MockLogger();
            _calculator = new EvolutionCalculator(_gameData, _playerState, _logger);
        }

        [Fact]
        public void CalculatePopupPosition_MouseMode_PositionsNearCursor()
        {
            // Arrange
            float anchorX = 100f;
            float anchorY = 200f;
            float popupWidth = 420f;
            float popupHeight = 300f;
            float screenWidth = 1920f;
            float screenHeight = 1080f;

            // Act
            var (x, y) = _calculator.CalculatePopupPosition(
                anchorX, anchorY, popupWidth, popupHeight, screenWidth, screenHeight, usingController: false);

            // Assert
            Assert.Equal(85f, x); // anchorX - 15
            Assert.Equal(240f, y); // anchorY + 40
        }

        [Fact]
        public void CalculatePopupPosition_ControllerMode_PositionsToLeft()
        {
            // Arrange
            float anchorX = 500f;
            float anchorY = 300f;
            float popupWidth = 420f;
            float popupHeight = 300f;
            float screenWidth = 1920f;
            float screenHeight = 1080f;

            // Act
            var (x, y) = _calculator.CalculatePopupPosition(
                anchorX, anchorY, popupWidth, popupHeight, screenWidth, screenHeight, usingController: true);

            // Assert
            Assert.Equal(290f, x); // anchorX - (popupWidth * 0.5)
            Assert.Equal(315f, y); // anchorY + 15
        }

        [Fact]
        public void CalculatePopupPosition_NearRightEdge_ClampsToScreen()
        {
            // Arrange - popup would go off right edge
            float anchorX = 900f;
            float anchorY = 200f;
            float popupWidth = 420f;
            float popupHeight = 300f;
            float screenWidth = 1920f;
            float screenHeight = 1080f;

            // Act
            var (x, y) = _calculator.CalculatePopupPosition(
                anchorX, anchorY, popupWidth, popupHeight, screenWidth, screenHeight, usingController: false);

            // Assert - should be clamped to fit on screen
            float halfWidth = screenWidth / 2; // 960
            Assert.True(x + popupWidth <= halfWidth, "Popup should not exceed right edge");
            Assert.Equal(540f, x); // halfWidth - popupWidth
        }

        [Fact]
        public void CalculatePopupPosition_NearLeftEdge_ClampsToScreen()
        {
            // Arrange - popup would go off left edge
            float anchorX = -900f;
            float anchorY = 200f;
            float popupWidth = 420f;
            float popupHeight = 300f;
            float screenWidth = 1920f;
            float screenHeight = 1080f;

            // Act
            var (x, y) = _calculator.CalculatePopupPosition(
                anchorX, anchorY, popupWidth, popupHeight, screenWidth, screenHeight, usingController: false);

            // Assert
            float halfWidth = screenWidth / 2; // 960
            Assert.True(x >= -halfWidth, "Popup should not go past left edge");
            Assert.Equal(-960f, x); // -halfWidth
        }

        [Fact]
        public void CalculatePopupPosition_NearBottomEdge_ClampsToScreen()
        {
            // Arrange - popup would go off bottom edge
            float anchorX = 100f;
            float anchorY = -500f;
            float popupWidth = 420f;
            float popupHeight = 300f;
            float screenWidth = 1920f;
            float screenHeight = 1080f;

            // Act
            var (x, y) = _calculator.CalculatePopupPosition(
                anchorX, anchorY, popupWidth, popupHeight, screenWidth, screenHeight, usingController: false);

            // Assert
            float halfHeight = screenHeight / 2; // 540
            Assert.True(y - popupHeight >= -halfHeight, "Popup should not go past bottom edge");
            Assert.Equal(-240f, y); // -halfHeight + popupHeight
        }

        [Fact]
        public void CalculatePopupPosition_NearTopEdge_ClampsToScreen()
        {
            // Arrange - popup would go off top edge
            float anchorX = 100f;
            float anchorY = 600f;
            float popupWidth = 420f;
            float popupHeight = 300f;
            float screenWidth = 1920f;
            float screenHeight = 1080f;

            // Act
            var (x, y) = _calculator.CalculatePopupPosition(
                anchorX, anchorY, popupWidth, popupHeight, screenWidth, screenHeight, usingController: false);

            // Assert
            float halfHeight = screenHeight / 2; // 540
            Assert.True(y <= halfHeight, "Popup should not exceed top edge");
            Assert.Equal(540f, y); // halfHeight
        }

        // NOTE: Evolution formula tests would require working with IL2CPP types,
        // which is challenging. We'd need to either:
        // 1. Create DTOs that don't depend on IL2CPP
        // 2. Use a different abstraction layer
        // 3. Focus testing on the pure logic parts (like popup positioning)
    }
}
