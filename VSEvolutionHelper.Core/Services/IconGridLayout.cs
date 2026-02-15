namespace VSItemTooltips.Core.Services
{
    /// <summary>
    /// Calculates grid layout for icon grids (like evolution formulas, arcana affects, etc.).
    /// Pure math service - no Unity/IL2CPP dependencies.
    /// </summary>
    public class IconGridLayout
    {
        private readonly float _iconSize;
        private readonly float _spacing;

        /// <summary>
        /// Creates a grid layout calculator with the given icon size and spacing.
        /// </summary>
        /// <param name="iconSize">Size of each icon (width and height)</param>
        /// <param name="spacing">Spacing between icons</param>
        public IconGridLayout(float iconSize, float spacing)
        {
            _iconSize = iconSize;
            _spacing = spacing;
        }

        /// <summary>
        /// Calculates how many icons fit per row given available width.
        /// </summary>
        /// <param name="availableWidth">Available width for the grid</param>
        /// <returns>Number of icons that fit in one row (minimum 1)</returns>
        public int CalculateIconsPerRow(float availableWidth)
        {
            if (availableWidth <= 0)
                return 1;

            // Each icon takes up: iconSize + spacing (except the last one doesn't need spacing)
            // So: width = (iconSize + spacing) * count - spacing
            // Solving for count: count = (width + spacing) / (iconSize + spacing)
            
            float totalPerIcon = _iconSize + _spacing;
            int count = (int)((availableWidth + _spacing) / totalPerIcon);
            
            // Always return at least 1
            return count < 1 ? 1 : count;
        }

        /// <summary>
        /// Calculates grid dimensions (rows and columns) for a given number of icons.
        /// </summary>
        /// <param name="iconCount">Total number of icons to lay out</param>
        /// <param name="availableWidth">Available width for the grid</param>
        /// <returns>Tuple of (rows, columns)</returns>
        public (int rows, int cols) CalculateGrid(int iconCount, float availableWidth)
        {
            if (iconCount <= 0)
                return (0, 0);

            int cols = CalculateIconsPerRow(availableWidth);
            int rows = (iconCount + cols - 1) / cols; // Ceiling division

            return (rows, cols);
        }

        /// <summary>
        /// Gets the X,Y position for an icon at the given index in the grid.
        /// Position is relative to top-left (0,0) with Y growing downward (Unity UI style).
        /// </summary>
        /// <param name="index">Icon index (0-based)</param>
        /// <param name="cols">Number of columns in the grid</param>
        /// <returns>Tuple of (x, y) position</returns>
        public (float x, float y) GetIconPosition(int index, int cols)
        {
            if (cols <= 0)
                cols = 1;

            int row = index / cols;
            int col = index % cols;

            float x = col * (_iconSize + _spacing);
            float y = -(row * (_iconSize + _spacing)); // Negative because Y grows upward in math but downward in UI

            return (x, y);
        }

        /// <summary>
        /// Calculates the total height needed for a grid with the given number of rows.
        /// </summary>
        /// <param name="rows">Number of rows</param>
        /// <returns>Total height in pixels</returns>
        public float CalculateGridHeight(int rows)
        {
            if (rows <= 0)
                return 0f;

            // Height = rows * iconSize + (rows - 1) * spacing
            return rows * _iconSize + (rows - 1) * _spacing;
        }

        /// <summary>
        /// Calculates the total width needed for a row with the given number of columns.
        /// </summary>
        /// <param name="cols">Number of columns</param>
        /// <returns>Total width in pixels</returns>
        public float CalculateRowWidth(int cols)
        {
            if (cols <= 0)
                return 0f;

            // Width = cols * iconSize + (cols - 1) * spacing
            return cols * _iconSize + (cols - 1) * _spacing;
        }

        /// <summary>
        /// Calculates the total grid height for a given number of icons and available width.
        /// Convenience method that combines CalculateGrid + CalculateGridHeight.
        /// </summary>
        /// <param name="iconCount">Total number of icons</param>
        /// <param name="availableWidth">Available width for the grid</param>
        /// <returns>Total height needed</returns>
        public float CalculateTotalHeight(int iconCount, float availableWidth)
        {
            var (rows, _) = CalculateGrid(iconCount, availableWidth);
            return CalculateGridHeight(rows);
        }
    }
}
