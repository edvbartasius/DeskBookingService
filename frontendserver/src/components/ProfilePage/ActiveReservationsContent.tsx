import { useState } from 'react';
import { Card, ListGroup, Badge, Button, Spinner, Alert } from 'react-bootstrap';
import { GroupedReservation } from '../../types/reservation.types.ts';
import { format, parseISO } from 'date-fns';
import api from '../../services/api.ts';
import ConfirmationModal from '../ConfirmationModal.tsx';

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

  const formatDate = (dateString: string) => {
    try {
      return format(parseISO(dateString), 'MMM dd, yyyy');
    } catch {
      return dateString;
    }
  };

  const formatDateRange = (dates: string[]) => {
    if (dates.length === 0) return 'No dates';
    if (dates.length === 1) return formatDate(dates[0]);

    // Just show first and last date as range, or list if few dates
    const sortedDates = [...dates].sort();

    if (sortedDates.length <= 3) {
      return sortedDates.map(formatDate).join(', ');
    }

    // Show first 2 dates and count
    return `${formatDate(sortedDates[0])}, ${formatDate(sortedDates[1])}, +${sortedDates.length - 2} more`;
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

  const DatesDisplay = ({ dates }: { dates: string[] }) => {
    const [showAll, setShowAll] = useState(false);
    const sortedDates = [...dates].sort();
    const displayText = formatDateRange(dates);
    const hasMoreDates = sortedDates.length > 3;

    if (showAll && hasMoreDates) {
      return (
        <div>
          <p className="mb-1 small">
            <i className="bi bi-calendar-range me-1"></i>
            {sortedDates.map(formatDate).join(', ')}
          </p>
          <Button
            variant="link"
            size="sm"
            className="p-0 text-decoration-none small"
            onClick={() => setShowAll(false)}
          >
            Show less
          </Button>
        </div>
      );
    }

    return (
      <div>
        <p className="mb-1 small">
          <i className="bi bi-calendar-range me-1"></i>
          {displayText}
        </p>
        {hasMoreDates && (
          <Button
            variant="link"
            size="sm"
            className="p-0 text-decoration-none small"
            onClick={() => setShowAll(true)}
          >
            Show all dates
          </Button>
        )}
      </div>
    );
  };

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
                <h6 className="mb-1">
                  {group.deskDescription || `Desk ${group.deskId}`}
                  <Badge bg="secondary" className="ms-2">
                    {group.reservationCount} {group.reservationCount === 1 ? 'day' : 'days'}
                  </Badge>
                  {group.hasToday && (
                    <Badge bg="success" className="ms-1">Today</Badge>
                  )}
                </h6>
                <p className="mb-1 text-muted small">
                  <i className="bi bi-building me-1"></i>
                  {group.buildingName || 'Unknown Building'}
                </p>
                <DatesDisplay dates={group.dates} />
                <p className="mb-2 text-muted small">
                  Booked: {format(parseISO(group.createdAt), 'MMM dd, yyyy HH:mm')}
                  {group.daysUntilFirst === 0 && ' • Starting today'}
                  {group.daysUntilFirst === 1 && ' • Starting tomorrow'}
                  {group.daysUntilFirst > 1 && ` • Starting in ${group.daysUntilFirst} days`}
                </p>
                <Button
                  variant="outline-danger"
                  size="sm"
                  className="w-100"
                  onClick={() => handleCancelClick(group.reservationGroupId, group.deskDescription || `Desk ${group.deskId}`)}
                >
                  Cancel
                </Button>
              </div>
            </ListGroup.Item>
          ))}
        </ListGroup>
      )}

      <ConfirmationModal
        show={showConfirmModal}
        onHide={() => setShowConfirmModal(false)}
        onConfirm={handleConfirmCancel}
        title="Cancel Reservation"
        message={`Are you sure you want to cancel all reservations for ${pendingCancellation?.deskDescription || 'this desk'}?`}
        confirmText="Yes, Cancel"
        cancelText="No, Keep It"
        variant="danger"
      />
    </Card.Body>
  );
};

export default ActiveReservationsContent;

