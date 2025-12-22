import DatabaseViewer from "../components/DatabaseViewer/DatabaseViewer.tsx";
import { useUser } from "../contexts/UserContext.tsx"
import { Button, Card, Container } from "react-bootstrap";
import { Navigate } from "react-router-dom";

const AdminPage = () => {
    const { isAdmin } = useUser(); // Logged in user context for limiting accesibility to page

    return (
        <>
            {isAdmin ? (
                <div className="page-container">
                    <Container className="page-container py-2">
                        <h1 className="text-start mb-4 pt-2 fw-bold">Admin Dashboard</h1>
                        <Card className="mt-4">
                            <Card.Header className="fw-bold fs-2">Database Viewer</Card.Header>
                            <Card.Body>
                                <DatabaseViewer />
                            </Card.Body>
                        </Card>
                    </Container>
                </div>
            ) : (
                // If user is not logged in, redirect home
                <Navigate to="/" replace />
            )}
        </>
    );
}

export default AdminPage;