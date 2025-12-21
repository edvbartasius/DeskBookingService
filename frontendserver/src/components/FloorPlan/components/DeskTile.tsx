import React, { useState } from 'react';
import { DeskTileProps, DeskType } from '../types/floorPlan.types.ts';
import { getDeskColor } from '../utils/deskHelpers.ts';
import { getDeskPixelPosition } from '../utils/gridHelpers.ts';
import {
  DESK_PADDING,
  DESK_STROKE_WIDTH,
  SELECTION_PADDING,
  SELECTION_STROKE_WIDTH,
  SELECTION_COLOR
} from '../config/constants.ts';

const DeskTile: React.FC<DeskTileProps> = ({ desk, onClick, onHover, isSelected, cellSize }) => {
  const [isHovered, setIsHovered] = useState(false);

  // All desks are the same size - one grid cell
  const width = cellSize;
  const height = cellSize;
  const colors = getDeskColor(desk.status, desk.isReservedByCaller);

  // Convert grid coordinates to pixel coordinates
  const pixelPosition = getDeskPixelPosition(desk.positionX, desk.positionY);

  // Handle mouse enter
  const handleMouseEnter = () => {
    setIsHovered(true);
    onHover?.(desk);
  };

  // Handle mouse leave
  const handleMouseLeave = () => {
    setIsHovered(false);
    onHover?.(null);
  };

  // Get desk icon/shape based on type - all desks are uniform size
  const renderDeskShape = (type: DeskType) => {
    switch (type) {
      case DeskType.Conference:
        // Ellipse/rounded for conference room
        return (
          <ellipse
            cx={width / 2}
            cy={height / 2}
            rx={width / 2 - DESK_PADDING}
            ry={height / 2 - DESK_PADDING}
            fill={colors.fill}
            stroke={colors.stroke}
            strokeWidth={DESK_STROKE_WIDTH}
          />
        );

      default:
        // Square for standard desks
        return (
          <rect
            x={DESK_PADDING}
            y={DESK_PADDING}
            width={width - DESK_PADDING * 2}
            height={height - DESK_PADDING * 2}
            fill={colors.fill}
            stroke={colors.stroke}
            strokeWidth={DESK_STROKE_WIDTH}
          />
        );
    }
  };



  return (
    <g
      transform={`translate(${pixelPosition.x}, ${pixelPosition.y})`}
      onClick={onClick}
      onMouseEnter={handleMouseEnter}
      onMouseLeave={handleMouseLeave}
      style={{ cursor: onClick ? 'pointer' : 'default' }}
      className="desk-tile"
    >
      {/* Hover effect background */}
      {isHovered && (
        <rect
          x="0"
          y="0"
          width={width}
          height={height}
          fill="rgba(0, 0, 0, 0.1)"
          pointerEvents="none"
        />
      )}

      {/* Desk shape */}
      {renderDeskShape(desk.type)}

      {/* Selection indicator */}
      {isSelected && (
        <rect
          x={-SELECTION_PADDING}
          y={-SELECTION_PADDING}
          width={width + SELECTION_PADDING * 2}
          height={height + SELECTION_PADDING * 2}
          rx="6"
          ry="6"
          fill="none"
          stroke={SELECTION_COLOR}
          strokeWidth={SELECTION_STROKE_WIDTH}
          strokeDasharray="5,5"
        >
          <animate
            attributeName="stroke-dashoffset"
            from="0"
            to="10"
            dur="1s"
            repeatCount="indefinite"
          />
        </rect>
      )}

      {/* Desk ID/Label
      <text
        x={width / 2}
        y={height / 2}
        textAnchor="middle"
        dominantBaseline="middle"
        fill="white"
        fontSize="12"
        fontWeight="600"
        pointerEvents="none"
        style={{
          userSelect: 'none',
          textShadow: '0 1px 2px rgba(0,0,0,0.3)'
        }}
      >
        {desk.description || `D${desk.id}`}
      </text> */}
    </g>
  );
};

export default DeskTile;
