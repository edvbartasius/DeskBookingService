import { GRID_CELL_SIZE } from '../config/constants.ts';

/**
 * Convert grid coordinates to pixel coordinates
 */
export const gridToPixel = (gridPosition: number): number => {
  return gridPosition * GRID_CELL_SIZE;
};

/**
 * Convert pixel coordinates to grid coordinates
 */
export const pixelToGrid = (pixelPosition: number): number => {
  return Math.floor(pixelPosition / GRID_CELL_SIZE);
};

/**
 * Get pixel coordinates for a desk based on grid position
 */
export const getDeskPixelPosition = (gridX: number, gridY: number) => {
  return {
    x: gridToPixel(gridX),
    y: gridToPixel(gridY)
  };
};

/**
 * Calculate total pixel dimensions from grid dimensions
 */
export const getFloorPlanPixelDimensions = (gridWidth: number, gridHeight: number) => {
  return {
    width: gridToPixel(gridWidth),
    height: gridToPixel(gridHeight)
  };
};
