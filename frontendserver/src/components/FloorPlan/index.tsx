// Main components
export { default as FloorPlanCanvas } from './components/FloorPlanCanvas.tsx';
export { default as DeskTile } from './components/DeskTile.tsx';

// Types
export * from './types/floorPlan.types.ts';

// Utils
export * from './utils/deskHelpers.ts';
export * from './utils/gridHelpers.ts';

// Config
export * from './config/constants.ts';

// Hooks
export { useFloorPlanZoom } from './hooks/useFloorPlanZoom.ts';
export { useFloorPlanPan } from './hooks/useFloorPlanPan.ts';
export { useContainerSize } from './hooks/useContainerSize.ts';
