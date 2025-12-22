import { Card, Container, Row, Col, Modal, Button } from "react-bootstrap";
import { useUser } from "../contexts/UserContext.tsx";
import { Navigate } from "react-router-dom";
import { useState } from "react";
import {
    UserProfileContent,
    ActiveReservationsContent,
    ReservationHistoryContent
} from "../components/ProfilePage/index.tsx";
import { useActiveReservations, useReservationHistory, useUserProfile } from "../components/ProfilePage/hooks/useProfileInfo.ts";

const ProfilePage = () => {
    const { loggedInUser } = useUser();

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
    const cards = [
        {
            id: 0,
            title: "User Profile",
            description: "View your profile information",
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
            content: (
                <ActiveReservationsContent
                    reservations={activeReservations}
                    loading={loadingActive}
                    error={errorActive}
                    userId={loggedInUser?.id || ''}
                    onRefresh={refetchActive}
                    onRefreshHistory={refetchHistory}
                />
            )
        },
        {
            id: 2,
            title: "Reservation History",
            description: "View your past and cancelled reservations",
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
            <Container className="px-4">
                <h1 className="text-start mb-4 pt-4 fw-bold">Profile Page</h1>
                <Row className="g-3">
                    {cards.map((card) => (
                        <Col key={card.id} xs={12} lg={4}>
                            <Card className="h-100 d-flex flex-column">
                                <Card.Header>
                                    <div className="text-center">
                                        <h2 className="fs-1">{card.title}</h2>
                                        <p className="fs-6 mb-1 text-muted">{card.description}</p>
                                    </div>
                                </Card.Header>
                                <Card.Body className="overflow-auto" style={{ maxHeight: '600px' }}>
                                    {card.content}
                                </Card.Body>
                            </Card>
                        </Col>
                    ))}
                </Row>
            </Container>
        );
    }

    // Render login prompt for non-authenticated users
    return (
        // If user is not logged in, redirect home
        <Navigate to="/" replace/>
    );
}

export default ProfilePage;