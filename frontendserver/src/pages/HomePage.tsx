import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card } from 'react-bootstrap';
import LoginModal from '../components/LoginModal.tsx';
import RegisterModal from '../components/RegisterModal.tsx';
import { useUser } from '../contexts/UserContext.tsx';

const HomePage = () => {
  const navigate = useNavigate();
  const { loggedInUser } = useUser();
  const [showLoginModal, setShowLoginModal] = useState(false);
  const [showRegisterModal, setShowRegisterModal] = useState(false);

  const handleSwitchToRegister = () => {
    setShowLoginModal(false);
    setShowRegisterModal(true);
  };

  const handleSwitchToLogin = () => {
    setShowRegisterModal(false);
    setShowLoginModal(true);
  };

  return (
    <div className="container py-5">
      <div className="text-center mb-5">
        <h1 className="display-4 mb-3">Desk Booking Service</h1>
        <p className="lead text-muted mb-4">Book your workspace for the day</p>
        <div className="d-flex gap-3 justify-content-center">
          <button className="btn btn-primary btn-lg" onClick={() => navigate('/desks')}>
            Book a Desk!
          </button>
          {!loggedInUser && (
            <button className="btn btn-outline-primary btn-lg" onClick={() => setShowLoginModal(true)}>
              Log In
            </button>
          )};
        </div>
      </div>

      <LoginModal
        show={showLoginModal}
        onHide={() => setShowLoginModal(false)}
        onSwitchToRegister={handleSwitchToRegister}
      />

      <RegisterModal
        show={showRegisterModal}
        onHide={() => setShowRegisterModal(false)}
        onSwitchToLogin={handleSwitchToLogin}
      />

      <div className="row g-4 mt-5">
        <div className="col-md-6">
          <Card className="h-100 shadow">
            <Card.Body className="text-center p-5">
              <div className="display-1 text-primary mb-3">
                <i className="bi bi-calendar-check"></i>
              </div>
              <Card.Title className="fw-bold fs-4 mb-3">Easy Booking</Card.Title>
              <Card.Text>Reserve your desk in seconds with our intuitive interface</Card.Text>
            </Card.Body>
          </Card>
        </div>
        <div className="col-md-6">
          <Card className="h-100 shadow">
            <Card.Body className="text-center p-5">
              <div className="display-1 text-primary mb-3">
                <i className="bi bi-buildings"></i>
              </div>
              <Card.Title className="fw-bold fs-4 mb-3">Multiple Locations</Card.Title>
              <Card.Text>Choose from various buildings and floors that suit your needs</Card.Text>
            </Card.Body>
          </Card>
        </div>
      </div>
    </div>
  );
}

export default HomePage;
