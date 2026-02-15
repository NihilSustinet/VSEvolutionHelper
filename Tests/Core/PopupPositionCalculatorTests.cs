using Xunit;
using VSItemTooltips.Core.Models;

namespace VSEvolutionHelper.Tests.Core
{
    /// <summary>
    /// Tests for popup positioning logic - pure C# with NO game dependencies.
    /// These tests run perfectly in CI/CD!
    /// </summary>
    public class PopupPositionCalculatorTests
    {
        [Fact]
        public void CalculatePosition_MouseMode_PositionsNearCursor()
        {
            // Arrange
            var calculator = new PopupPositionCalculator(1920f, 1080f);
            
            // Act
            var (x, y) = calculator.CalculatePosition(
                anchorX: 100f,
                anchorY: 200f,
                popupWidth: 420f,
                popupHeight: 300f,
                usingController: false);

            // Assert
            Assert.Equal(85f, x); // anchorX - 15
            Assert.Equal(240f, y); // anchorY + 40
        }

        [Fact]
        public void CalculatePosition_ControllerMode_PositionsToLeft()
        {
            // Arrange
            var calculator = new PopupPositionCalculator(1920f, 1080f);
            
            // Act
            var (x, y) = calculator.CalculatePosition(
                anchorX: 500f,
                anchorY: 300f,
                popupWidth: 420f,
                popupHeight: 300f,
                usingController: true);

            // Assert
            Assert.Equal(290f, x); // anchorX - (popupWidth * 0.5)
            Assert.Equal(315f, y); // anchorY + 15
        }

        [Fact]
        public void CalculatePosition_NearRightEdge_ClampsToScreen()
        {
            // Arrange
            var calculator = new PopupPositionCalculator(1920f, 1080f);
            
            // Act
            var (x, y) = calculator.CalculatePosition(
                anchorX: 900f,
                anchorY: 200f,
                popupWidth: 420f,
                popupHeight: 300f,
                usingController: false);

            // Assert
            Assert.Equal(540f, x); // Clamped: halfWidth - popupWidth
        }

        [Fact]
        public void CalculatePosition_NearLeftEdge_ClampsToScreen()
        {
            // Arrange
            var calculator = new PopupPositionCalculator(1920f, 1080f);

            // Act
            var (x, y) = calculator.CalculatePosition(
                anchorX: -900f,
                anchorY: 200f,
                popupWidth: 420f,
                popupHeight: 300f,
                usingController: false);

            // Assert
            Assert.Equal(-915f, x); // anchorX - 15 = -900 - 15 = -915
        }

        [Fact]
        public void CalculatePosition_NearBottomEdge_ClampsToScreen()
        {
            // Arrange
            var calculator = new PopupPositionCalculator(1920f, 1080f);
            
            // Act
            var (x, y) = calculator.CalculatePosition(
                anchorX: 100f,
                anchorY: -500f,
                popupWidth: 420f,
                popupHeight: 300f,
                usingController: false);

            // Assert
            Assert.Equal(-240f, y); // Clamped: -halfHeight + popupHeight
        }

        [Fact]
        public void CalculatePosition_NearTopEdge_ClampsToScreen()
        {
            // Arrange
            var calculator = new PopupPositionCalculator(1920f, 1080f);
            
            // Act
            var (x, y) = calculator.CalculatePosition(
                anchorX: 100f,
                anchorY: 600f,
                popupWidth: 420f,
                popupHeight: 300f,
                usingController: false);

            // Assert
            Assert.Equal(540f, y); // Clamped: halfHeight
        }
    }
}
