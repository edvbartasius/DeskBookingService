// Grid and sizing constants
export const GRID_CELL_SIZE = 50;
export const FLOOR_PLAN_PADDING = 50;

// Zoom constraints
export const MIN_ZOOM_SCALE = 0.1;
export const MAX_ZOOM_SCALE = 5;
export const ZOOM_FACTOR = 0.1;

// Desk visual constants
export const DESK_PADDING = 2; // Small padding inside the cell
export const DESK_BORDER_RADIUS = 4;
export const DESK_STROKE_WIDTH = 2;

// Selection indicator
export const SELECTION_PADDING = 4;
export const SELECTION_STROKE_WIDTH = 3;
export const SELECTION_COLOR = '#FF9800';

// Desk status colors
export const DESK_COLORS = {
  available: {
    fill: '#4CAF50',
    stroke: '#388E3C',
    hover: '#66BB6A'
  },
  booked: {
    fill: '#F44336',
    stroke: '#D32F2F',
    hover: '#EF5350'
  },
  unavailable: {
    fill: '#9E9E9E',
    stroke: '#616161',
    hover: '#BDBDBD'
  },
  default: {
    fill: '#2196F3',
    stroke: '#1976D2',
    hover: '#42A5F5'
  }
};

// Grid styling
export const GRID_STROKE_COLOR = '#e0e0e0';
export const GRID_STROKE_WIDTH = 1;

// Floor plan boundary
export const FLOOR_PLAN_STROKE_COLOR = '#999';
export const FLOOR_PLAN_STROKE_WIDTH = 2;
