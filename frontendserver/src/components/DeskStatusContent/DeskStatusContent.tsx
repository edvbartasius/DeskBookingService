import React from 'react';
import { Button } from 'react-bootstrap';
import { DeskDto, DeskStatus } from '../FloorPlan/types/floorPlan.types.ts';

interface DeskStatusContentProps {
  desk: DeskDto;
  variant?: 'compact' | 'full';
  showButton?: boolean;
  onReserveClick?: () => void;
  onCancelClick?: (cancelType: 'single' | 'range') => void;
}

export const DeskStatusContent: React.FC<DeskStatusContentProps> = ({
  desk,
  variant = 'full',
  showButton = false,
  onReserveClick,
  onCancelClick
}) => {
  const handleReserveClick = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (onReserveClick) {
      onReserveClick();
    }
  };

  const handleCancelSingleDay = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (onCancelClick) {
      onCancelClick('single');
    }
  };

  const handleCancelRange = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (onCancelClick) {
      onCancelClick('range');
    }
  };

  const renderAvailableContent = () => {
    if (showButton && variant === 'full') {
      return (
        <Button
          className="w-100 text-center"
          size="sm"
          onClick={handleReserveClick}
          style={{ pointerEvents: 'auto', fontSize: '0.8rem', padding: '0.4rem 0.5rem' }}
        >
          <strong>Reserve</strong>
        </Button>
      );
    }
    return (
      <small className="text-success d-block text-center">
        <strong>Reserve this desk</strong>
      </small>
    );
  };

  const renderBookedContent = () => (
    <div className="text-center">
      {!desk.isReservedByCaller && <small className="text-muted d-block" style={{ fontSize: '0.8rem' }}>
        <strong>Reserved by:</strong> {desk.reservedByFullName || 'Unknown'}
      </small>
      }
      {desk.isReservedByCaller && (
        <div>
          <small className="text-primary d-block mb-1 pb-0 mb-0" style={{ fontSize: '0.75rem' }}>
            <strong className='mb-0'>Your reservation</strong>
          </small>
          {showButton && variant === 'full' && onCancelClick && (
            <div className="d-flex gap-1">
              <Button
                variant="outline-danger"
                size="sm"
                onClick={handleCancelSingleDay}
                style={{ pointerEvents: 'auto', fontSize: '0.65rem', whiteSpace: 'nowrap', flex: 1, padding: '0.25rem 0.3rem' }}
              >
                Cancel Day
              </Button>
              <Button
                variant="danger"
                size="sm"
                onClick={handleCancelRange}
                style={{ pointerEvents: 'auto', fontSize: '0.65rem', whiteSpace: 'nowrap', flex: 1, padding: '0.25rem 0.3rem' }}
              >
                Cancel All
              </Button>
            </div>
          )}
        </div>
      )}
    </div>
  );

  const renderUnavailableContent = () => (
    <div className="text-center">
      {desk.maintenanceReason && (
        <small className="text-muted d-block" style={{ fontSize: '0.8rem' }}>
          <strong>Reason:</strong> {desk.maintenanceReason}
        </small>
      )}
      <small className="text-danger d-block" style={{ fontSize: '0.8rem' }}>
        <strong>Cannot be reserved</strong>
      </small>
    </div>
  );

  switch (desk.status) {
    case DeskStatus.Available:
      return renderAvailableContent();
    case DeskStatus.Booked:
      return renderBookedContent();
    case DeskStatus.Unavailable:
      return renderUnavailableContent();
    default:
      return null;
  }
};
