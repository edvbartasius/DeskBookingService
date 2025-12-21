import { Button, Card, Container } from "react-bootstrap";
import ExpandableCardContainer, { CardConfig } from "../components/ExpandableCardContainer.tsx";
import { useUser } from "../contexts/UserContext.tsx";
import { Link } from "react-router-dom";
import LoginModal from '../components/LoginModal.tsx';
import { useState } from "react";
import RegisterModal from "../components/RegisterModal.tsx";
import {
    UserProfileContent,
    ActiveReservationsContent,
    ReservationHistoryContent
} from "../components/ProfilePage/index.tsx";
import { useActiveReservations, useReservationHistory, useUserProfile } from "../components/ProfilePage/hooks/useProfileInfo.ts";

const ProfilePage = () => {
    const { loggedInUser } = useUser();
    const [showLoginModal, setShowLoginModal] = useState(false);
    const [showRegisterModal, setShowRegisterModal] = useState(false);

    // Fetch user profile data dynamically
    const {
        user: userProfile,
        loading: loadingProfile,
        error: errorProfile,
        refetch: refetchProfile
    } = useUserProfile(loggedInUser?.id);

    // Fetch reservations data
    const {
        reservations: activeReservations,
        loading: loadingActive,
        error: errorActive,
        refetch: refetchActive
    } = useActiveReservations(loggedInUser?.id);

    const {
        history: reservationHistory,
        loading: loadingHistory,
        error: errorHistory,
        refetch: refetchHistory
    } = useReservationHistory(loggedInUser?.id);

    // Card configurations
    const cards: CardConfig[] = [
        {
            id: 0,
            title: "User Profile",
            description: "View your profile information",
            allowExpand: false,
            content: (
                <UserProfileContent
                    user={userProfile}
                    loading={loadingProfile}
                    error={errorProfile}
                    onRefresh={refetchProfile}
                />
            )
        },
        {
            id: 1,
            title: "Active Reservations",
            description: "View and manage your active reservations",
            allowExpand: true,
            content: (
                <ActiveReservationsContent
                    reservations={activeReservations}
                    loading={loadingActive}
                    error={errorActive}
                    userId={loggedInUser?.id || ''}
                    onRefresh={refetchActive}
                />
            )
        },
        {
            id: 2,
            title: "Reservation History",
            description: "View your past and cancelled reservations",
            allowExpand: true,
            content: (
                <ReservationHistoryContent
                    history={reservationHistory}
                    loading={loadingHistory}
                    error={errorHistory}
                    onRefresh={refetchHistory}
                />
            )
        }
    ];



    // Render logged-in user view
    if (loggedInUser) {
        return (
            <ExpandableCardContainer
                cards={cards}
                title="Profile Page"
                cardsPerRow={3}
            />
        );
    }

    // Render login prompt for non-authenticated users
    return (
        <Container
            className="md-6 d-flex justify-content-center align-items-center"
            style={{ minHeight: '50vh' }}
        >
            <Card className="d-flex justify-content-center align-items-center">
                <Card.Header>Login to view your profile!</Card.Header>
                <Card.Body>
                    <Link to="/home">
                        <Button variant="primary" className="me-2">Home</Button>
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
    );
}

export default ProfilePage;