import DatabaseViewer from "../components/DatabaseViewer/DatabaseViewer.tsx";
import { useUser } from "../contexts/UserContext.tsx"
import { Button, Card, Container } from "react-bootstrap";
import { Link } from "react-router-dom";

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
                <Container className="md-6 d-flex justify-content-center align-items-center"
                style={{minHeight: '50vh'}}>
                    <Card className="d-flex justify-content-center align-items-center">
                        <Card.Header>Unauthorized access!</Card.Header>
                        <Card.Body>
                            <Link to="/home">
                                <Button variant="primary">Home</Button>
                            </Link>
                        </Card.Body>

                    </Card>
                </Container>
            )}
        </>
    );
}

export default AdminPage;