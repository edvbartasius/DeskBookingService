import { Card, ListGroup, Badge, Spinner, Alert, Button } from 'react-bootstrap';
import { ReservationDto, ReservationStatus } from '../../types/reservation.types.ts';
import { format, parseISO } from 'date-fns';

interface ReservationHistoryContentProps {
  history: ReservationDto[];
  loading: boolean;
  error: string | null;
  onRefresh: () => void;
}

const ReservationHistoryContent = ({
  history,
  loading,
  error,
  onRefresh
}: ReservationHistoryContentProps) => {

  const formatDate = (dateString: string) => {
    try {
      return format(parseISO(dateString), 'MMM dd, yyyy');
    } catch {
      return dateString;
    }
  };

  const getStatusBadge = (status: ReservationStatus) => {
    // Backend already provides the effective status
    switch (status) {
      case ReservationStatus.Cancelled:
        return <Badge bg="danger ms-2 align-text-bottom">Cancelled</Badge>;
      case ReservationStatus.Completed:
        return <Badge bg="success ms-2 align-text-bottom">Completed</Badge>;
      case ReservationStatus.Active:
        return <Badge bg="primary ms-2 align-text-bottom">Active</Badge>;
      default:
        return <Badge bg="secondary ms-2 align-text-bottom">Unknown</Badge>;
    }
  };
//ms-2 align-text-bottom
  if (loading) {
    return (
      <Card.Body className="text-center py-5">
        <Spinner animation="border" variant="primary" />
        <p className="mt-3 text-muted">Loading history...</p>
      </Card.Body>
    );
  }

  if (error) {
    return (
      <Card.Body>
        <Alert variant="danger">
          <Alert.Heading>Error Loading History</Alert.Heading>
          <p>{error}</p>
          <Button variant="outline-danger" size="sm" onClick={onRefresh}>
            Try Again
          </Button>
        </Alert>
      </Card.Body>
    );
  }

  if (history.length === 0) {
    return (
      <Card.Body className="text-center py-5">
        <p className="text-muted">No reservation history</p>
        <p className="small">Past and cancelled reservations will appear here</p>
      </Card.Body>
    );
  }

  return (
    <Card.Body>
      <ListGroup variant="flush">
        {history.map((reservation) => (
          <ListGroup.Item key={reservation.id} className="px-0 py-3 border-bottom">
            <div className="d-flex justify-content-between align-items-start gap-4">
              {/* Left: Main info */}
              <div className="flex-grow-1">
                {/* Desk title */}
                <h6 className="mb-2 fs-5 fw-semibold">
                  {reservation.desk?.description || `Desk ${reservation.deskId}`}
                </h6>

                {/* Date with icon */}
                <p className="mb-1 fs-6 text-muted">
                  {/* <i className="bi bi-calendar me-2 text-primary"></i> */}
                  {formatDate(reservation.reservationDate)}
                </p>

                {/* Cancelled info */}
                {reservation.canceledAt && (
                  <p className="mb-0 fs-6 text-muted">
                    Cancelled on {format(parseISO(reservation.canceledAt), 'MMM dd, yyyy HH:mm')}
                  </p>
                )}
              </div>

              {/* Right: Status badge */}
              <div className="flex-shrink-0">
                {getStatusBadge(reservation.status)}
              </div>
            </div>
          </ListGroup.Item>
        ))}
      </ListGroup>
    </Card.Body>
  );
};

export default ReservationHistoryContent;

