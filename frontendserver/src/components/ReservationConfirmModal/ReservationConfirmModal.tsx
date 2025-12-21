import React, { useState, useEffect } from 'react';
import { Modal, Button, Alert } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { format } from 'date-fns';
import { DeskDto } from '../FloorPlan/types/floorPlan.types';
import { DateSelector } from '../DateSelector/index.tsx';

interface ReservationConfirmModalProps {
  show: boolean;
  onHide: () => void;
  desk: DeskDto | null;
  isSuccess: boolean;
  isLoading: boolean;
  error: string | null;
  onConfirm: (selectedDates: Date[]) => void;
  bookedDates?: Date[];
  closedDates?: Date[];
  initialDate?: Date | null;
}

export const ReservationConfirmModal: React.FC<ReservationConfirmModalProps> = ({
  show,
  onHide,
  desk,
  isSuccess,
  isLoading,
  error,
  onConfirm,
  bookedDates = [],
  closedDates = [],
  initialDate = null
}) => {
  const navigate = useNavigate();
  const [selectedDates, setSelectedDates] = useState<Date[]>([]);

  // Pre-select the initial date when modal opens
  useEffect(() => {
    if (show && !isSuccess && initialDate) {
      setSelectedDates([initialDate]);
    }
  }, [show, isSuccess, initialDate]);

  const handleViewReservations = () => {
    navigate('/profile');
    onHide();
  };

  const handleContinue = () => {
    setSelectedDates([]);
    onHide();
  };

  const handleConfirm = () => {
    if (selectedDates.length > 0) {
      onConfirm(selectedDates);
    }
  };

  if (!desk) {
    return null;
  }

  const getSuccessMessage = () => {
    if (selectedDates.length === 1) {
      return `Your desk has been reserved for ${format(selectedDates[0], 'MMM d, yyyy')}.`;
    }
    return `Your desk has been reserved for ${selectedDates.length} dates.`;
  };

  return (
    <Modal show={show} onHide={onHide} centered size="lg">
      <Modal.Header closeButton>
        <Modal.Title>
          {isSuccess ? 'Reservation Confirmed' : 'Reserve Desk'}
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>
        {isSuccess ? (
          <div>
            <Alert variant="success">
              <strong>Success!</strong> {getSuccessMessage()}
            </Alert>
            <div className="mb-3">
              <p className="mb-2"><strong>Desk:</strong> {desk.description}</p>
              <p className="mb-2">
                <strong>Dates:</strong>{' '}
                {selectedDates.map(d => format(d, 'MMM d, yyyy')).join(', ')}
              </p>
            </div>
          </div>
        ) : (
          <div>
            {error && (
              <Alert variant="danger" className="mb-3">
                <strong>Error:</strong> {error}
              </Alert>
            )}
            <div className="mb-3">
              <p className="mb-2"><strong>Desk:</strong> {desk.description}</p>
            </div>

            <div className="mb-3">
              <h6>Select Dates for Reservation</h6>
              <DateSelector
                selectedDate={null}
                onSelectedDateChange={() => {}}
                multiSelect={true}
                selectedDates={selectedDates}
                onSelectedDatesChange={setSelectedDates}
                bookedDates={bookedDates}
                closedDates={closedDates}
                disabled={isLoading}
              />
            </div>

            {selectedDates.length > 0 && (
              <Alert variant="info">
                <strong>Selected {selectedDates.length} date{selectedDates.length > 1 ? 's' : ''}:</strong>
                <div className="mt-2">
                  {selectedDates.map((date, idx) => (
                    <div key={idx}>{format(date, 'EEEE, MMMM d, yyyy')}</div>
                  ))}
                </div>
              </Alert>
            )}
          </div>
        )}
      </Modal.Body>
      <Modal.Footer>
        {isSuccess ? (
          <>
            <Button variant="secondary" onClick={handleContinue}>
              Continue Booking
            </Button>
            <Button variant="primary" onClick={handleViewReservations}>
              View Reservations
            </Button>
          </>
        ) : (
          <>
            <Button variant="secondary" onClick={onHide} disabled={isLoading}>
              Cancel
            </Button>
            <Button
              variant="primary"
              onClick={handleConfirm}
              disabled={isLoading || selectedDates.length === 0}
            >
              {isLoading ? 'Reserving...' : `Confirm ${selectedDates.length > 0 ? `(${selectedDates.length} date${selectedDates.length > 1 ? 's' : ''})` : 'Reservation'}`}
            </Button>
          </>
        )}
      </Modal.Footer>
    </Modal>
  );
};
