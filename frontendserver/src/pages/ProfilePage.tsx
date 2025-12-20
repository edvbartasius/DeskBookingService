import { Button, Card, Container } from "react-bootstrap";
import ExpandableCardContainer, { CardConfig } from "../components/ExpandableCardContainer.tsx";
import { useUser } from "../contexts/UserContext.tsx";
import { Link } from "react-router-dom";
import LoginModal from '../components/LoginModal.tsx';
import { useState } from "react";
import RegisterModal from "../components/RegisterModal.tsx";

const ProfilePage = () => {
    const { loggedInUser } = useUser(); // Logged in user context to display cards only for logged in user
    const [showLoginModal, setShowLoginModal] = useState(false);
    const [showRegisterModal, setShowRegisterModal] = useState(false);

    const UserProfileContent = () => {
        return (
            <Card.Body>
                <div className="mb-3">
                    <strong>Name:</strong>
                    <p className="ms-2">{loggedInUser?.name || 'N/A'}</p>
                </div>
                <div className="mb-3">
                    <strong>Surname:</strong>
                    <p className="ms-2">{loggedInUser?.surname || 'N/A'}</p>
                </div>
                <div className="mb-3">
                    <strong>Email:</strong>
                    <p className="ms-2">{loggedInUser?.email || 'N/A'}</p>
                </div>
                <div className="mb-3">
                    <strong>ID:</strong>
                    <p className="ms-2">{loggedInUser?.id || 'N/A'}</p>
                </div>
            </Card.Body>
        );
    }

    const cards: CardConfig[] = [
        {
            id: 0,
            title: "User Profile",
            description: "View and edit your profile information",
            allowExpand: false,
            content: <UserProfileContent />
        },
        {
            id: 1,
            title: "Active Reservations",
            description: "View and manage active reservations",
            allowExpand: true,
            content: "Active reservation functionality goes here"
        },
        {
            id: 2,
            title: "Reservation History",
            description: "View and manage reservation history",
            allowExpand: true,
            content: "Reservation history functionality here"
        }
    ];



    return (
        <> {loggedInUser ? (
            <ExpandableCardContainer cards={cards} title="Profile Page" cardsPerRow={3} />
        ) : (
            <Container className="md-6 d-flex justify-content-center align-items-center"
                style={{ minHeight: '50vh' }}>
                <Card className="d-flex justify-content-center align-items-center">
                    <Card.Header>Login to view your profile!</Card.Header>
                    <Card.Body>
                        <Link to="/home">
                            <Button variant="primary">Home</Button>
                        </Link>
                        <Button onClick={() => setShowLoginModal(true)}>
                            Login
                        </Button>

                    </Card.Body>

                </Card>
                <LoginModal
                    show={showLoginModal}
                    onHide={() => setShowLoginModal(false)}
                    onSwitchToRegister={() => {
                        setShowRegisterModal(true);
                        setShowLoginModal(false);
                    }}
                />
                <RegisterModal
                    show={showRegisterModal}
                    onHide={() => setShowRegisterModal(false)}
                    onSwitchToLogin={() => {
                        setShowRegisterModal(false);
                        setShowLoginModal(true);
                    }}
                />
            </Container>


        )}
        </>
    );
}

export default ProfilePage;