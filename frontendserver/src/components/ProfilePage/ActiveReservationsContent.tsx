import { useState } from 'react';
import { Card, ListGroup, Badge, Button, Spinner, Alert } from 'react-bootstrap';
import { GroupedReservation } from '../../types/reservation.types.ts';
import { format, parseISO } from 'date-fns';
import api from '../../services/api.ts';
import ConfirmationModal from '../ConfirmationModal.tsx';
import { DatesDisplay } from './DatesDisplay.tsx';

interface ActiveReservationsContentProps {
  reservations: GroupedReservation[];
  loading: boolean;
  error: string | null;
  userId: string;
  onRefresh: () => void;
}

const ActiveReservationsContent = ({
  reservations,
  loading,
  error,
  userId,
  onRefresh
}: ActiveReservationsContentProps) => {
  const [showConfirmModal, setShowConfirmModal] = useState(false);
  const [pendingCancellation, setPendingCancellation] = useState<{
    reservationGroupId: string;
    deskDescription: string;
  } | null>(null);

  const handleCancelClick = (reservationGroupId: string, deskDescription: string) => {
    setPendingCancellation({ reservationGroupId, deskDescription });
    setShowConfirmModal(true);
  };

  const handleConfirmCancel = async () => {
    if (!pendingCancellation) return;

    try {
      const response = await api.patch(
        `reservations/my-reservations/cancel-booking-group/${pendingCancellation.reservationGroupId}/${userId}`
      );

      if (response.status === 200) {
        // Refresh the reservations list
        onRefresh();
      }
    } catch (err: any) {
      console.error('Failed to cancel reservation group:', err);
      const errorMessage = err.response?.data?.error || err.response?.data || err.message || 'Failed to cancel reservation';
      alert(`Error: ${errorMessage}`);
    } finally {
      setPendingCancellation(null);
    }
  };



  if (loading) {
    return (
      <Card.Body className="text-center py-5">
        <Spinner animation="border" variant="primary" />
        <p className="mt-3 text-muted">Loading reservations...</p>
      </Card.Body>
    );
  }

  if (error) {
    return (
      <Card.Body>
        <Alert variant="danger">
          <Alert.Heading>Error Loading Reservations</Alert.Heading>
          <p>{error}</p>
          <Button variant="outline-danger" size="sm" onClick={onRefresh}>
            Try Again
          </Button>
        </Alert>
      </Card.Body>
    );
  }

  if (reservations.length === 0) {
    return (
      <Card.Body className="text-center py-5">
        <p className="text-muted">No active reservations</p>
        <p className="small">Book a desk to see your reservations here</p>
      </Card.Body>
    );
  }

  // Backend already filters to only upcoming reservations
  return (
    <Card.Body>
      {reservations.length === 0 ? (
        <div className="text-center py-5">
          <p className="text-muted">No active reservations</p>
          <p className="small">Book a desk to see your reservations here</p>
        </div>
      ) : (
        <ListGroup variant="flush">
          {reservations.map((group) => (
            <ListGroup.Item key={group.reservationGroupId} className="px-0">
              <div>
                <h6 className="mb-1 fw-semibold fs-5">
                  {group.deskDescription || `Desk ${group.deskId}`}
                  {group.hasToday && (
                    <Badge bg="success" className="ms-2 align-text-bottom" pill>
                      Today
                    </Badge>
                  )}
                </h6>

                <span className="mb-1 text-muted">
                  <p className="fs-6">{group.buildingName || 'Unknown Building'}</p>
                </span>

                <div className="mb-4">
                  {/* Reservation count – its own row */}
                  <p className="mb-0 fs-6 fw-medium">
                    {group.reservationCount} {group.reservationCount === 1 ? 'day' : 'days'} booked:{" "}
                  </p>

                  {/* Dates – its own row */}
                  <DatesDisplay dates={group.dates} />
                </div>

                <p className="mb-0 text-muted">
                  <span className="fs-6 fw-medium">

                  </span>
                  <span className="fs-6">
                    {format(parseISO(group.createdAt), 'MMM dd, yyyy HH:mm')}
                  </span>
                  {group.daysUntilFirst === 0 && <span className="fw-medium"> • Starting today</span>}
                  {group.daysUntilFirst === 1 && <span className="fw-medium"> • Starting tomorrow</span>}
                  {group.daysUntilFirst > 1 && <span className="fw-medium"> • Starting in {group.daysUntilFirst} days</span>}
                </p>
              </div>
              <Button
                variant="outline-danger"
                size="sm"
                className="w-100"
                onClick={() => handleCancelClick(group.reservationGroupId, group.deskDescription || `Desk ${group.deskId}`)}
              >
                Cancel
              </Button>
            </ListGroup.Item>
          ))}
        </ListGroup>
      )
      }

      <ConfirmationModal
        show={showConfirmModal}
        onHide={() => setShowConfirmModal(false)}
        onConfirm={handleConfirmCancel}
        title="Cancel Reservation"
        message={`Are you sure you want to cancel reservation for ${pendingCancellation?.deskDescription || 'this desk'}?`}
        confirmText="Yes, Cancel"
        cancelText="No, Keep It"
        variant="danger"
      />
    </Card.Body >
  );
};

export default ActiveReservationsContent;

