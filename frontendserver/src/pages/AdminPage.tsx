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
                    <Container className="page-container">
                        <h1>Admin Page</h1>
                        <Card className="mt-4">
                            <Card.Header>Database Viewer</Card.Header>
                            <Card.Body>
                                <DatabaseViewer />
                            </Card.Body>
                        </Card>
                    </Container>
                </div>
            ) : (
                // If user is not logged in, redirect home
                <Navigate to="/" replace/>
            )}
        </>
    );
}

export default AdminPage;