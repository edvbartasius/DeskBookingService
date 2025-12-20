import React, { useRef, useState, useEffect } from 'react';
import DeskTile from './DeskTile.tsx';
import { FloorPlanCanvasProps, ViewBox } from '../types/floorPlan.types.ts';
import { useFloorPlanZoom } from '../hooks/useFloorPlanZoom.ts';
import { useFloorPlanPan } from '../hooks/useFloorPlanPan.ts';
import { useContainerSize } from '../hooks/useContainerSize.ts';
import { getFloorPlanPixelDimensions } from '../utils/gridHelpers.ts';
import {
  GRID_CELL_SIZE,
  FLOOR_PLAN_PADDING,
  GRID_STROKE_COLOR,
  GRID_STROKE_WIDTH,
  FLOOR_PLAN_STROKE_COLOR,
  FLOOR_PLAN_STROKE_WIDTH
} from '../config/constants.ts';

const FloorPlanCanvas: React.FC<FloorPlanCanvasProps> = ({
  floorPlan,
  onDeskClick,
  selectedDeskId
}) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const svgRef = useRef<SVGSVGElement>(null);

  // Convert grid dimensions to pixel dimensions
  const floorPlanPixels = floorPlan
    ? getFloorPlanPixelDimensions(floorPlan.floorPlanWidth, floorPlan.floorPlanHeight)
    : { width: 0, height: 0 };

  const [viewBox, setViewBox] = useState<ViewBox>({ x: 0, y: 0, width: 0, height: 0 });

  // Custom hooks
  const containerSize = useContainerSize(containerRef);
  const { scale, setScale, handleZoom } = useFloorPlanZoom({
    viewBox,
    setViewBox,
    svgRef
  });
  const {
    isPanning,
    handleMouseDown,
    handleMouseMove,
    handleMouseUp,
    handleMouseLeave,
    handleTouchStart,
    handleTouchMove
  } = useFloorPlanPan({ viewBox, setViewBox, containerSize });

  // Initialize viewBox based on floor plan dimensions (in pixels)
  useEffect(() => {
    if (floorPlan && containerSize.width > 0) {
      setViewBox({
        x: -FLOOR_PLAN_PADDING,
        y: -FLOOR_PLAN_PADDING,
        width: floorPlanPixels.width + FLOOR_PLAN_PADDING * 2,
        height: floorPlanPixels.height + FLOOR_PLAN_PADDING * 2
      });
    }
  }, [floorPlan, containerSize, floorPlanPixels.width, floorPlanPixels.height]);

  // Reset zoom and pan
  const resetView = () => {
    setScale(1);
    setViewBox({
      x: -FLOOR_PLAN_PADDING,
      y: -FLOOR_PLAN_PADDING,
      width: floorPlanPixels.width + FLOOR_PLAN_PADDING * 2,
      height: floorPlanPixels.height + FLOOR_PLAN_PADDING * 2
    });
  };

  // Fit to container
  const fitToContainer = () => {
    if (containerRef.current && floorPlan) {
      const containerAspect = containerSize.width / containerSize.height;
      const floorPlanAspect = floorPlanPixels.width / floorPlanPixels.height;

      let newScale = 1;

      if (containerAspect > floorPlanAspect) {
        newScale = containerSize.height / (floorPlanPixels.height + FLOOR_PLAN_PADDING * 2);
      } else {
        newScale = containerSize.width / (floorPlanPixels.width + FLOOR_PLAN_PADDING * 2);
      }

      setScale(newScale);
      setViewBox({
        x: -FLOOR_PLAN_PADDING,
        y: -FLOOR_PLAN_PADDING,
        width: floorPlanPixels.width + FLOOR_PLAN_PADDING * 2,
        height: floorPlanPixels.height + FLOOR_PLAN_PADDING * 2
      });
    }
  };

  useEffect(() => {
    fitToContainer();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [floorPlan, containerSize]);

  if (!floorPlan) {
    return (
      <div className="floor-plan-canvas-empty">
        <p>No floor plan data available</p>
      </div>
    );
  }

  return (
    <div
      ref={containerRef}
      className="floor-plan-canvas-container"
      style={{
        width: '100%',
        height: '100%',
        position: 'relative',
        overflow: 'hidden',
        backgroundColor: '#f5f5f5',
        borderRadius: '8px'
      }}
    >
      {/* Controls */}
      <div
        style={{
          position: 'absolute',
          top: '16px',
          right: '16px',
          zIndex: 10,
          display: 'flex',
          flexDirection: 'column',
          gap: '8px',
          backgroundColor: 'white',
          padding: '8px',
          borderRadius: '8px',
          boxShadow: '0 2px 8px rgba(0,0,0,0.15)'
        }}
      >
        <button
          onClick={() => handleZoom(1)}
          style={{
            padding: '8px 12px',
            border: '1px solid #ddd',
            borderRadius: '4px',
            backgroundColor: 'white',
            cursor: 'pointer',
            fontSize: '16px'
          }}
          title="Zoom In"
        >
          +
        </button>
        <button
          onClick={() => handleZoom(-1)}
          style={{
            padding: '8px 12px',
            border: '1px solid #ddd',
            borderRadius: '4px',
            backgroundColor: 'white',
            cursor: 'pointer',
            fontSize: '16px'
          }}
          title="Zoom Out"
        >
          -
        </button>
        <button
          onClick={resetView}
          style={{
            padding: '8px 12px',
            border: '1px solid #ddd',
            borderRadius: '4px',
            backgroundColor: 'white',
            cursor: 'pointer',
            fontSize: '12px'
          }}
          title="Reset View"
        >
          Reset
        </button>
        <button
          onClick={fitToContainer}
          style={{
            padding: '8px 12px',
            border: '1px solid #ddd',
            borderRadius: '4px',
            backgroundColor: 'white',
            cursor: 'pointer',
            fontSize: '12px'
          }}
          title="Fit to Screen"
        >
          Fit
        </button>
      </div>

      {/* Zoom level indicator */}
      <div
        style={{
          position: 'absolute',
          bottom: '16px',
          right: '16px',
          zIndex: 10,
          backgroundColor: 'white',
          padding: '8px 12px',
          borderRadius: '4px',
          boxShadow: '0 2px 8px rgba(0,0,0,0.15)',
          fontSize: '14px'
        }}
      >
        {Math.round(scale * 100)}%
      </div>

      {/* Building name */}
      {floorPlan.buildingName && (
        <div
          style={{
            position: 'absolute',
            top: '16px',
            left: '16px',
            zIndex: 10,
            backgroundColor: 'white',
            padding: '8px 16px',
            borderRadius: '4px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.15)',
            fontSize: '16px',
            fontWeight: '600'
          }}
        >
          {floorPlan.buildingName}
        </div>
      )}

      {/* SVG Floor Plan */}
      <svg
        ref={svgRef}
        viewBox={`${viewBox.x} ${viewBox.y} ${viewBox.width} ${viewBox.height}`}
        style={{
          width: '100%',
          height: '100%',
          cursor: isPanning ? 'grabbing' : 'grab'
        }}
        onMouseDown={handleMouseDown}
        onMouseMove={handleMouseMove}
        onMouseUp={handleMouseUp}
        onMouseLeave={handleMouseLeave}
        onTouchStart={handleTouchStart}
        onTouchMove={handleTouchMove}
        onTouchEnd={handleMouseUp}
      >
        {/* Grid background */}
        <defs>
          <pattern
            id="grid"
            width={GRID_CELL_SIZE}
            height={GRID_CELL_SIZE}
            patternUnits="userSpaceOnUse"
          >
            <path
              d={`M ${GRID_CELL_SIZE} 0 L 0 0 0 ${GRID_CELL_SIZE}`}
              fill="none"
              stroke={GRID_STROKE_COLOR}
              strokeWidth={GRID_STROKE_WIDTH}
            />
          </pattern>
        </defs>

        {/* Floor plan boundary */}
        <rect
          x={0}
          y={0}
          width={floorPlanPixels.width}
          height={floorPlanPixels.height}
          fill="url(#grid)"
          stroke={FLOOR_PLAN_STROKE_COLOR}
          strokeWidth={FLOOR_PLAN_STROKE_WIDTH}
        />

        {/* Render desks */}
        {floorPlan.floorPlanDesks?.map((desk) => (
          <DeskTile
            key={desk.id}
            desk={desk}
            onClick={() => onDeskClick?.(desk)}
            isSelected={selectedDeskId === desk.id}
            cellSize={GRID_CELL_SIZE}
          />
        ))}
      </svg>
    </div>
  );
};

export default FloorPlanCanvas;
