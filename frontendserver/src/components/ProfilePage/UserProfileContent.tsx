import { Card, Spinner, Alert, Button, Badge } from 'react-bootstrap';
import { UserDto } from '../../types/reservation.types.ts';

interface UserProfileContentProps {
  user: UserDto | null;
  loading: boolean;
  error: string | null;
  onRefresh: () => void;
}

const UserProfileContent = ({ user, loading, error, onRefresh }: UserProfileContentProps) => {
  if (loading) {
    return (
      <Card.Body className="text-center py-5">
        <Spinner animation="border" variant="primary" />
        <p className="mt-3 text-muted">Loading profile...</p>
      </Card.Body>
    );
  }

  if (error) {
    return (
      <Card.Body>
        <Alert variant="danger">
          <Alert.Heading>Error Loading Profile</Alert.Heading>
          <p>{error}</p>
          <Button variant="outline-danger" size="sm" onClick={onRefresh}>
            Try Again
          </Button>
        </Alert>
      </Card.Body>
    );
  }

  if (!user) {
    return (
      <Card.Body className="text-center py-5">
        <p className="text-muted">No profile data available</p>
      </Card.Body>
    );
  }

  const getRoleBadge = (role: number) => {
    return role === 0 ? (
      <Badge bg="danger">Admin</Badge>
    ) : (
      <Badge bg="primary">User</Badge>
    );
  };

  return (
    <Card.Body>
      <div className="mb-3">
        <strong>Name:</strong>
        <p className="ms-2 mb-0">{user.name || 'N/A'}</p>
      </div>
      <div className="mb-3">
        <strong>Surname:</strong>
        <p className="ms-2 mb-0">{user.surname || 'N/A'}</p>
      </div>
      <div className="mb-3">
        <strong>Email:</strong>
        <p className="ms-2 mb-0">{user.email || 'N/A'}</p>
      </div>
      <div className="mb-3">
        <strong>Role:</strong>
        <div className="ms-2">{getRoleBadge(user.role)}</div>
      </div>
      <div className="mb-3">
        <strong>User ID:</strong>
        <p className="ms-2 mb-0 text-muted small">{user.id || 'N/A'}</p>
      </div>
    </Card.Body>
  );
};

export default UserProfileContent;

