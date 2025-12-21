import React from 'react';
import { Row, Col, Alert } from 'react-bootstrap';
import { FloorPlanDto, DeskDto } from '../FloorPlan';
import { DeskCard } from '../DeskCard/index.tsx';

interface DeskListViewProps {
  floorPlan: FloorPlanDto | null;
  onReserveClick?: (desk: DeskDto) => void;
  onCancelClick?: (desk: DeskDto, cancelType: 'single' | 'range') => void;
  loading?: boolean;
}

export const DeskListView: React.FC<DeskListViewProps> = ({
  floorPlan,
  onReserveClick,
  onCancelClick,
  loading = false
}) => {
  if (loading) {
    return (
      <div className="d-flex justify-content-center align-items-center h-100">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading desks...</span>
        </div>
      </div>
    );
  }

  if (!floorPlan || !floorPlan.floorPlanDesks) {
    return (
      <div className="d-flex justify-content-center align-items-center h-100">
        <Alert variant="info">Select a date to view available desks</Alert>
      </div>
    );
  }

  return (
    <Row className="g-3">
      {floorPlan.floorPlanDesks.map((desk) => (
        <Col key={desk.id} md={6} lg={4}>
          <DeskCard
            desk={desk}
            onReserveClick={onReserveClick}
            onCancelClick={onCancelClick}
          />
        </Col>
      ))}
    </Row>
  );
};
