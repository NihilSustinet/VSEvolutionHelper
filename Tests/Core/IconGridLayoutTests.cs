using Xunit;
using VSItemTooltips.Core.Services;

namespace VSEvolutionHelper.Tests.Core
{
    /// <summary>
    /// Tests for icon grid layout calculations.
    /// Validates layout math for evolution formulas, arcana affects grids, etc.
    /// </summary>
    public class IconGridLayoutTests
    {
        #region CalculateIconsPerRow Tests

        [Fact]
        public void CalculateIconsPerRow_ExactFit_ReturnsCorrectCount()
        {
            // Arrange: 38px icons, 6px spacing, 440px width
            // Should fit: (440 + 6) / (38 + 6) = 446 / 44 = 10.13 -> 10 icons
            var layout = new IconGridLayout(iconSize: 38f, spacing: 6f);

            // Act
            int count = layout.CalculateIconsPerRow(440f);

            // Assert
            Assert.Equal(10, count);
        }

        [Fact]
        public void CalculateIconsPerRow_NarrowWidth_ReturnsOne()
        {
            // Arrange: 48px icons, 8px spacing, only 30px width available
            var layout = new IconGridLayout(iconSize: 48f, spacing: 8f);

            // Act
            int count = layout.CalculateIconsPerRow(30f);

            // Assert: Should return at least 1
            Assert.Equal(1, count);
        }

        [Fact]
        public void CalculateIconsPerRow_ZeroWidth_ReturnsOne()
        {
            // Arrange
            var layout = new IconGridLayout(iconSize: 48f, spacing: 8f);

            // Act
            int count = layout.CalculateIconsPerRow(0f);

            // Assert: Should handle gracefully
            Assert.Equal(1, count);
        }

        [Fact]
        public void CalculateIconsPerRow_NegativeWidth_ReturnsOne()
        {
            // Arrange
            var layout = new IconGridLayout(iconSize: 48f, spacing: 8f);

            // Act
            int count = layout.CalculateIconsPerRow(-100f);

            // Assert: Should handle gracefully
            Assert.Equal(1, count);
        }

        [Fact]
        public void CalculateIconsPerRow_VeryWideWidth_ReturnsManyIcons()
        {
            // Arrange: 38px icons, 6px spacing, 1920px width (full HD screen)
            var layout = new IconGridLayout(iconSize: 38f, spacing: 6f);

            // Act
            int count = layout.CalculateIconsPerRow(1920f);

            // Assert: Should fit many icons
            Assert.True(count >= 40); // (1920 + 6) / (38 + 6) = 43
        }

        [Fact]
        public void CalculateIconsPerRow_CommonArcanaCase_ReturnsCorrectCount()
        {
            // Arrange: Real scenario from arcana "Affects" grid
            // 38px icons, 6px spacing, 396px available width (420 - 24 padding)
            var layout = new IconGridLayout(iconSize: 38f, spacing: 6f);

            // Act
            int count = layout.CalculateIconsPerRow(396f);

            // Assert: (396 + 6) / (38 + 6) = 9.13 -> 9 icons
            Assert.Equal(9, count);
        }

        #endregion

        #region CalculateGrid Tests

        [Fact]
        public void CalculateGrid_SingleIcon_ReturnsSingleCell()
        {
            // Arrange
            var layout = new IconGridLayout(iconSize: 48f, spacing: 8f);

            // Act
            var (rows, cols) = layout.CalculateGrid(iconCount: 1, availableWidth: 400f);

            // Assert
            Assert.Equal(1, rows);
            Assert.True(cols >= 1);
        }

        [Fact]
        public void CalculateGrid_IconsFitInOneRow_ReturnsOneRow()
        {
            // Arrange: 5 icons, wide enough for 10 per row
            var layout = new IconGridLayout(iconSize: 38f, spacing: 6f);

            // Act
            var (rows, cols) = layout.CalculateGrid(iconCount: 5, availableWidth: 500f);

            // Assert
            Assert.Equal(1, rows);
            Assert.True(cols >= 5);
        }

        [Fact]
        public void CalculateGrid_IconsNeedTwoRows_ReturnsTwoRows()
        {
            // Arrange: 15 icons, width fits 10 per row
            var layout = new IconGridLayout(iconSize: 38f, spacing: 6f);
            int iconsPerRow = layout.CalculateIconsPerRow(440f); // Should be 10

            // Act
            var (rows, cols) = layout.CalculateGrid(iconCount: 15, availableWidth: 440f);

            // Assert
            Assert.Equal(2, rows); // 15 icons / 10 per row = 2 rows
            Assert.Equal(iconsPerRow, cols);
        }

        [Fact]
        public void CalculateGrid_ExactMultipleRows_ReturnsCorrectGrid()
        {
            // Arrange: 20 icons, exactly 10 per row = 2 full rows
            var layout = new IconGridLayout(iconSize: 38f, spacing: 6f);

            // Act
            var (rows, cols) = layout.CalculateGrid(iconCount: 20, availableWidth: 440f);

            // Assert
            Assert.Equal(2, rows);
            Assert.Equal(10, cols);
        }

        [Fact]
        public void CalculateGrid_PartialLastRow_ReturnsCorrectGrid()
        {
            // Arrange: 23 icons, 10 per row = 3 rows (last row has 3 icons)
            var layout = new IconGridLayout(iconSize: 38f, spacing: 6f);

            // Act
            var (rows, cols) = layout.CalculateGrid(iconCount: 23, availableWidth: 440f);

            // Assert
            Assert.Equal(3, rows); // Ceiling(23 / 10) = 3
            Assert.Equal(10, cols);
        }

        [Fact]
        public void CalculateGrid_ZeroIcons_ReturnsZeroGrid()
        {
            // Arrange
            var layout = new IconGridLayout(iconSize: 48f, spacing: 8f);

            // Act
            var (rows, cols) = layout.CalculateGrid(iconCount: 0, availableWidth: 400f);

            // Assert
            Assert.Equal(0, rows);
            Assert.Equal(0, cols);
        }

        [Fact]
        public void CalculateGrid_NarrowWidth_ForcesOneColumnPerRow()
        {
            // Arrange: 10 icons, but only 50px wide (fits 1 per row)
            var layout = new IconGridLayout(iconSize: 48f, spacing: 8f);

            // Act
            var (rows, cols) = layout.CalculateGrid(iconCount: 10, availableWidth: 50f);

            // Assert
            Assert.Equal(10, rows); // 10 icons in 1-column layout = 10 rows
            Assert.Equal(1, cols);
        }

        #endregion

        #region GetIconPosition Tests

        [Fact]
        public void GetIconPosition_FirstIcon_ReturnsOrigin()
        {
            // Arrange
            var layout = new IconGridLayout(iconSize: 48f, spacing: 8f);

            // Act
            var (x, y) = layout.GetIconPosition(index: 0, cols: 5);

            // Assert
            Assert.Equal(0f, x);
            Assert.Equal(0f, y);
        }

        [Fact]
        public void GetIconPosition_SecondIconInFirstRow_ReturnsCorrectX()
        {
            // Arrange: 48px icon + 8px spacing = 56px offset
            var layout = new IconGridLayout(iconSize: 48f, spacing: 8f);

            // Act
            var (x, y) = layout.GetIconPosition(index: 1, cols: 5);

            // Assert
            Assert.Equal(56f, x); // 1 * (48 + 8) = 56
            Assert.Equal(0f, y); // Still in first row
        }

        [Fact]
        public void GetIconPosition_FirstIconInSecondRow_ReturnsCorrectY()
        {
            // Arrange: 48px icon + 8px spacing = 56px offset
            var layout = new IconGridLayout(iconSize: 48f, spacing: 8f);

            // Act
            var (x, y) = layout.GetIconPosition(index: 5, cols: 5);

            // Assert
            Assert.Equal(0f, x); // First column
            Assert.Equal(-56f, y); // Second row: -(1 * (48 + 8)) = -56
        }

        [Fact]
        public void GetIconPosition_MiddleIcon_ReturnsCorrectPosition()
        {
            // Arrange: 38px icons, 6px spacing, 10 columns, icon at index 12 (row 1, col 2)
            var layout = new IconGridLayout(iconSize: 38f, spacing: 6f);

            // Act
            var (x, y) = layout.GetIconPosition(index: 12, cols: 10);

            // Assert: Row 1 (index 12 / 10 = 1), Col 2 (index 12 % 10 = 2)
            Assert.Equal(2 * (38f + 6f), x); // Col 2: 2 * 44 = 88
            Assert.Equal(-1 * (38f + 6f), y); // Row 1: -1 * 44 = -44
        }

        [Fact]
        public void GetIconPosition_LastIconInRow_ReturnsCorrectX()
        {
            // Arrange: 3 columns, icon at index 2 (last in first row)
            var layout = new IconGridLayout(iconSize: 40f, spacing: 10f);

            // Act
            var (x, y) = layout.GetIconPosition(index: 2, cols: 3);

            // Assert
            Assert.Equal(2 * (40f + 10f), x); // 2 * 50 = 100
            Assert.Equal(0f, y); // Still first row
        }

        [Fact]
        public void GetIconPosition_SingleColumnLayout_StacksVertically()
        {
            // Arrange: Single column grid
            var layout = new IconGridLayout(iconSize: 48f, spacing: 8f);

            // Act
            var pos0 = layout.GetIconPosition(index: 0, cols: 1);
            var pos1 = layout.GetIconPosition(index: 1, cols: 1);
            var pos2 = layout.GetIconPosition(index: 2, cols: 1);

            // Assert: All same X, different Y
            Assert.Equal(0f, pos0.x);
            Assert.Equal(0f, pos1.x);
            Assert.Equal(0f, pos2.x);
            Assert.Equal(0f, pos0.y);
            Assert.Equal(-56f, pos1.y);
            Assert.Equal(-112f, pos2.y);
        }

        #endregion

        #region CalculateGridHeight Tests

        [Fact]
        public void CalculateGridHeight_SingleRow_ReturnsIconSize()
        {
            // Arrange
            var layout = new IconGridLayout(iconSize: 48f, spacing: 8f);

            // Act
            float height = layout.CalculateGridHeight(rows: 1);

            // Assert: 1 row = just icon size (no spacing)
            Assert.Equal(48f, height);
        }

        [Fact]
        public void CalculateGridHeight_TwoRows_ReturnsCorrectHeight()
        {
            // Arrange
            var layout = new IconGridLayout(iconSize: 48f, spacing: 8f);

            // Act
            float height = layout.CalculateGridHeight(rows: 2);

            // Assert: 2 * 48 + 1 * 8 = 104
            Assert.Equal(104f, height);
        }

        [Fact]
        public void CalculateGridHeight_ThreeRows_ReturnsCorrectHeight()
        {
            // Arrange: 38px icons, 6px spacing
            var layout = new IconGridLayout(iconSize: 38f, spacing: 6f);

            // Act
            float height = layout.CalculateGridHeight(rows: 3);

            // Assert: 3 * 38 + 2 * 6 = 114 + 12 = 126
            Assert.Equal(126f, height);
        }

        [Fact]
        public void CalculateGridHeight_ZeroRows_ReturnsZero()
        {
            // Arrange
            var layout = new IconGridLayout(iconSize: 48f, spacing: 8f);

            // Act
            float height = layout.CalculateGridHeight(rows: 0);

            // Assert
            Assert.Equal(0f, height);
        }

        [Fact]
        public void CalculateGridHeight_ManyRows_ReturnsCorrectHeight()
        {
            // Arrange
            var layout = new IconGridLayout(iconSize: 40f, spacing: 10f);

            // Act
            float height = layout.CalculateGridHeight(rows: 10);

            // Assert: 10 * 40 + 9 * 10 = 400 + 90 = 490
            Assert.Equal(490f, height);
        }

        #endregion

        #region CalculateRowWidth Tests

        [Fact]
        public void CalculateRowWidth_SingleColumn_ReturnsIconSize()
        {
            // Arrange
            var layout = new IconGridLayout(iconSize: 48f, spacing: 8f);

            // Act
            float width = layout.CalculateRowWidth(cols: 1);

            // Assert
            Assert.Equal(48f, width);
        }

        [Fact]
        public void CalculateRowWidth_TwoColumns_ReturnsCorrectWidth()
        {
            // Arrange
            var layout = new IconGridLayout(iconSize: 48f, spacing: 8f);

            // Act
            float width = layout.CalculateRowWidth(cols: 2);

            // Assert: 2 * 48 + 1 * 8 = 104
            Assert.Equal(104f, width);
        }

        [Fact]
        public void CalculateRowWidth_TenColumns_ReturnsCorrectWidth()
        {
            // Arrange: 38px icons, 6px spacing
            var layout = new IconGridLayout(iconSize: 38f, spacing: 6f);

            // Act
            float width = layout.CalculateRowWidth(cols: 10);

            // Assert: 10 * 38 + 9 * 6 = 380 + 54 = 434
            Assert.Equal(434f, width);
        }

        [Fact]
        public void CalculateRowWidth_ZeroColumns_ReturnsZero()
        {
            // Arrange
            var layout = new IconGridLayout(iconSize: 48f, spacing: 8f);

            // Act
            float width = layout.CalculateRowWidth(cols: 0);

            // Assert
            Assert.Equal(0f, width);
        }

        #endregion

        #region CalculateTotalHeight Tests

        [Fact]
        public void CalculateTotalHeight_CombinesGridCalculation()
        {
            // Arrange: 15 icons, fits 10 per row = 2 rows
            // Row height: 2 * 38 + 1 * 6 = 82
            var layout = new IconGridLayout(iconSize: 38f, spacing: 6f);

            // Act
            float height = layout.CalculateTotalHeight(iconCount: 15, availableWidth: 440f);

            // Assert
            Assert.Equal(82f, height);
        }

        [Fact]
        public void CalculateTotalHeight_SingleIcon_ReturnsSingleIconHeight()
        {
            // Arrange
            var layout = new IconGridLayout(iconSize: 48f, spacing: 8f);

            // Act
            float height = layout.CalculateTotalHeight(iconCount: 1, availableWidth: 400f);

            // Assert
            Assert.Equal(48f, height);
        }

        [Fact]
        public void CalculateTotalHeight_ZeroIcons_ReturnsZero()
        {
            // Arrange
            var layout = new IconGridLayout(iconSize: 48f, spacing: 8f);

            // Act
            float height = layout.CalculateTotalHeight(iconCount: 0, availableWidth: 400f);

            // Assert
            Assert.Equal(0f, height);
        }

        #endregion

        #region Real-World Scenario Tests

        [Fact]
        public void RealWorld_ArcanaAffectsGrid_15Items()
        {
            // Arrange: Real arcana popup scenario
            // 38px icons, 6px spacing, 396px available (420 - 24 padding)
            // 15 affected items should make a nice grid
            var layout = new IconGridLayout(iconSize: 38f, spacing: 6f);

            // Act
            var (rows, cols) = layout.CalculateGrid(iconCount: 15, availableWidth: 396f);
            float height = layout.CalculateGridHeight(rows);

            // Assert: Should fit 9 per row = 2 rows
            Assert.Equal(2, rows);
            Assert.Equal(9, cols);
            Assert.Equal(82f, height); // 2 * 38 + 1 * 6
        }

        [Fact]
        public void RealWorld_EvolutionFormula_4Passives()
        {
            // Arrange: Evolution formula with 4 passive icons in a row
            // 36px icons, 3px spacing, 400px available
            var layout = new IconGridLayout(iconSize: 36f, spacing: 3f);

            // Act
            var (rows, cols) = layout.CalculateGrid(iconCount: 4, availableWidth: 400f);

            // Assert: Should fit in 1 row
            Assert.Equal(1, rows);
            Assert.True(cols >= 4);

            // Verify positions are correct
            var pos0 = layout.GetIconPosition(0, cols);
            var pos1 = layout.GetIconPosition(1, cols);
            var pos2 = layout.GetIconPosition(2, cols);
            var pos3 = layout.GetIconPosition(3, cols);

            Assert.Equal(0f, pos0.x);
            Assert.Equal(39f, pos1.x); // 36 + 3
            Assert.Equal(78f, pos2.x); // 2 * 39
            Assert.Equal(117f, pos3.x); // 3 * 39
        }

        [Fact]
        public void RealWorld_CollectionGrid_60Items()
        {
            // Arrange: Collection screen with many items
            // 40px icons, 8px spacing, 960px available (half screen)
            var layout = new IconGridLayout(iconSize: 40f, spacing: 8f);

            // Act
            var (rows, cols) = layout.CalculateGrid(iconCount: 60, availableWidth: 960f);
            float height = layout.CalculateTotalHeight(60, 960f);

            // Assert: Should fit 20 per row = 3 rows
            Assert.Equal(3, rows);
            Assert.Equal(20, cols);
            Assert.Equal(136f, height); // 3 * 40 + 2 * 8 = 120 + 16
        }

        #endregion
    }
}
