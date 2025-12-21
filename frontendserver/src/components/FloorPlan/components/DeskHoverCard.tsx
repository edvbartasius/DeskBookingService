import React from 'react';
import { DeskDto } from '../types/floorPlan.types.ts';
import { getStatusBadgeVariant, getDeskStatusLabel, getDeskTypeLabel } from '../utils/deskHelpers.ts';
import { DeskStatusContent } from '../../DeskStatusContent/index.tsx';

interface DeskHoverCardProps {
  desk: DeskDto;
  cellSize: number;
  onReserveClick?: (desk: DeskDto) => void;
  onCancelClick?: (desk: DeskDto, cancelType: 'single' | 'range') => void;
  onClose?: () => void;
}

const DeskHoverCard: React.FC<DeskHoverCardProps> = ({
  desk,
  cellSize,
  onReserveClick,
  onCancelClick,
  onClose
}) => {

  // Calculate position: place card to the right and slightly above the desk
  const cardX = desk.positionX * cellSize + cellSize + 10; // 10px offset from desk
  const cardY = desk.positionY * cellSize - 10; // Slight vertical offset

  const handleReserveClick = () => {
    onReserveClick?.(desk);
  };

  const handleCancelClick = (cancelType: 'single' | 'range') => {
    onCancelClick?.(desk, cancelType);
  };

  return (
    <foreignObject
      x={cardX}
      y={cardY}
      width="280"
      height="350"
      style={{ overflow: 'visible', pointerEvents: 'auto' }}
    >
      <div className="card shadow" style={{ minWidth: '250px', pointerEvents: 'auto' }}>
        <div className="card-body p-3">
          {/* Close button */}
          {onClose && (
            <button
              onClick={onClose}
              style={{
                position: 'absolute',
                top: '8px',
                right: '8px',
                border: 'none',
                background: 'transparent',
                cursor: 'pointer',
                fontSize: '20px',
                lineHeight: '20px',
                padding: '0',
                width: '24px',
                height: '24px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#6c757d'
              }}
              title="Close"
            >
              &times;
            </button>
          )}

          {/* Desk Name/Description */}
          <h6 className="card-title fw-bold mb-2">
            {desk.description || `Desk ${desk.id}`}
          </h6>

          {/* Desk Details */}
          <div className="mb-2">
            <small className="text-muted d-block">
              <strong>Type:</strong> {getDeskTypeLabel(desk.type)}
            </small>
          </div>

          {/* Status Badge */}
          <div className="mb-2">
            <span className={`badge bg-${getStatusBadgeVariant(desk.status, desk.isReservedByCaller)}`}>
              {getDeskStatusLabel(desk.status)}
            </span>
          </div>

          {/* Divider */}
          <hr className="my-2" />

          {/* Status-specific content */}
          <DeskStatusContent
            desk={desk}
            variant="full"
            showButton={true}
            onReserveClick={handleReserveClick}
            onCancelClick={handleCancelClick}
          />
        </div>
      </div>
    </foreignObject>
  );
};

export default DeskHoverCard;
