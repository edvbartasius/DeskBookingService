import React from 'react';
import { DeskDto } from '../types/floorPlan.types.ts';

interface DeskHoverCardProps {
  desk: DeskDto;
  cellSize: number;
}

const DeskHoverCard: React.FC<DeskHoverCardProps> = ({ desk, cellSize }) => {
  const getStatusBadgeVariant = (status?: number) => {
    switch (status) {
      case 0:
        return 'success';
      case 1:
        return 'warning';
      case 2:
        return 'secondary';
      default:
        return 'secondary';
    }
  };

  const getStatusLabel = (status?: number) => {
    switch (status) {
      case 0:
        return 'Available';
      case 1:
        return 'Booked';
      case 2:
        return 'Unavailable';
      default:
        return 'Unknown';
    }
  };

  const getDeskTypeLabel = (type: number) => {
    return type === 0 ? 'Standard' : 'Conference';
  };

  // Calculate position: place card to the right and slightly above the desk
  const cardX = desk.positionX * cellSize + cellSize + 10; // 10px offset from desk
  const cardY = desk.positionY * cellSize - 10; // Slight vertical offset

  return (
    <foreignObject
      x={cardX}
      y={cardY}
      width="280"
      height="350"
      style={{ overflow: 'visible', pointerEvents: 'none' }}
    >
      <div className="card shadow" style={{ minWidth: '250px', pointerEvents: 'none' }}>
        <div className="card-body p-3">
          {/* Desk Name/Description */}
          <h6 className="card-title fw-bold mb-2">
            {desk.description || `Desk ${desk.id}`}
          </h6>

          {/* Desk Details */}
          <div className="mb-2">
            <small className="text-muted d-block">
              <strong>Position:</strong> ({desk.positionX}, {desk.positionY})
            </small>
            <small className="text-muted d-block">
              <strong>Type:</strong> {getDeskTypeLabel(desk.type)}
            </small>
          </div>

          {/* Status Badge */}
          {desk.status !== undefined && (
            <div className="mb-2">
              <span className={`badge bg-${getStatusBadgeVariant(desk.status)}`}>
                {getStatusLabel(desk.status)}
              </span>
            </div>
          )}
        </div>
      </div>
    </foreignObject>
  );
};

export default DeskHoverCard;
