import { Card, Spinner, Alert, Button } from 'react-bootstrap';
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

  const getRoleText = (role: number): string => {
    return role === 0 ? "Admin" : "User";
  };

  return (
    <Card.Body>
      <div className="mb-3">
        <strong>Name:</strong>
        <span className="d-block">{user.name || "N/A"}</span>
        <hr className="my-2" />
      </div>

      <div className="mb-3">
        <strong>Surname:</strong>
        <span className="d-block">{user.surname || "N/A"}</span>
        <hr className="my-2" />
      </div>

      <div className="mb-3">
        <strong>Email:</strong>
        <span className="d-block">{user.email || "N/A"}</span>
        <hr className="my-2" />
      </div>

      <div className="mb-3">
        <strong>Role:</strong>
        <span className="d-block">{getRoleText(user.role)}</span>
        <hr className="my-2" />
      </div>
    </Card.Body>
  );
};

export default UserProfileContent;

