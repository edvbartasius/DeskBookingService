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
          className="d-block text-center"
          onClick={handleReserveClick}
          style={{ pointerEvents: 'auto' }}
        >
          <strong>Reserve this desk</strong>
        </Button>
      );
    }
    return (
      <small className="text-success d-block">
        <strong>Reserve this desk</strong>
      </small>
    );
  };

  const renderBookedContent = () => (
    <div className="text-center">
      {!desk.isReservedByCaller && <small className="text-muted d-block">
        <strong>Reserved by:</strong> {desk.reservedByFullName || 'Unknown'}
      </small>
      }
      {desk.isReservedByCaller && (
        <>
          <small className="text-primary d-block mt-1">
            <strong>This is your reservation</strong>
          </small>
          {showButton && variant === 'full' && onCancelClick && (
            <div className="mt-2 d-flex gap-2">
              <Button
                variant="outline-danger"
                size="sm"
                onClick={handleCancelSingleDay}
                style={{ pointerEvents: 'auto' }}
              >
                Cancel This Day
              </Button>
              <Button
                variant="danger"
                size="sm"
                onClick={handleCancelRange}
                style={{ pointerEvents: 'auto' }}
              >
                Cancel All Dates
              </Button>
            </div>
          )}
        </>
      )}
    </div>
  );

  const renderUnavailableContent = () => (
    <div className="text-center">
      {desk.maintenanceReason && (
        <small className="text-muted d-block">
          <strong>Reason:</strong> {desk.maintenanceReason}
        </small>
      )}
      <small className="text-danger d-block mt-1">
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
