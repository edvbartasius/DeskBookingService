import React from 'react';
import { Card, Badge } from 'react-bootstrap';
import { DeskDto, getDeskTypeLabel, getDeskStatusLabel, getStatusBadgeVariant } from '../FloorPlan/index.tsx';
import { DeskStatusContent } from '../DeskStatusContent/index.tsx';

interface DeskCardProps {
  desk: DeskDto;
  onReserveClick?: (desk: DeskDto) => void;
  onCancelClick?: (desk: DeskDto, cancelType: 'single' | 'range') => void;
}

export const DeskCard: React.FC<DeskCardProps> = ({
  desk,
  onReserveClick,
  onCancelClick
}) => {
  const [showOverlay, setShowOverlay] = React.useState(false);

  const handleReserveClick = () => {
    if (onReserveClick) {
      onReserveClick(desk);
    }
  };

  const handleCancelClick = (cancelType: 'single' | 'range') => {
    if (onCancelClick) {
      onCancelClick(desk, cancelType);
    }
  };

  const handleCardClick = (e: React.MouseEvent) => {
    // On mobile/touch devices, first click shows overlay
    if (!showOverlay) {
      e.stopPropagation();
      setShowOverlay(true);
    }
  };

  return (
    <div style={{ position: 'relative' }}>
      <Card
        className="h-100"
        style={{ cursor: 'pointer', overflow: 'hidden', position: 'relative' }}
        onClick={handleCardClick}
        onMouseEnter={() => setShowOverlay(true)}
        onMouseLeave={() => setShowOverlay(false)}
      >
        <Card.Body>
          <div className="d-flex justify-content-between align-items-start mb-2">
            <div>
              <h5 className="mb-0 fw-bold">#{desk.deskNumber}</h5>
              {desk.description && <small className="text-muted">{desk.description}</small>}
            </div>
            <Badge bg={getStatusBadgeVariant(desk.status, desk.isReservedByCaller)}>
              {getDeskStatusLabel(desk.status)}
            </Badge>
          </div>
          <div className="text-muted small">
            <div>{getDeskTypeLabel(desk.type)}</div>
          </div>
        </Card.Body>

        {/* Overlay that appears on hover */}
        <div
          className="desk-overlay"
          style={{
            position: 'absolute',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: 'rgba(255, 255, 255, 0.98)',
            display: showOverlay ? 'flex' : 'none',
            flexDirection: 'column',
            justifyContent: 'center',
            alignItems: 'center',
            padding: '1rem',
            borderRadius: '0.375rem',
            pointerEvents: showOverlay ? 'auto' : 'none',
            zIndex: 10
          }}
        >
          <h6 className="fw-bold mb-1">#{desk.deskNumber}</h6>
          {desk.description && <small className="text-muted d-block mb-1">{desk.description}</small>}
          <DeskStatusContent
            desk={desk}
            variant="full"
            showButton
            onReserveClick={handleReserveClick}
            onCancelClick={handleCancelClick}
          />
        </div>
      </Card>
    </div>
  );
};
