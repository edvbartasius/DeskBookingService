import ExpandableCardContainer, { CardConfig } from "../components/ExpandableCardContainer.tsx";
import DatabaseViewer from "../components/DatabaseViewer/DatabaseViewer.tsx";
import { useUser } from "../contexts/UserContext.tsx"
import { Button, Card, Container } from "react-bootstrap";
import { Link } from "react-router-dom";

const AdminPage = () => {
    const { isAdmin } = useUser(); // Logged in user context for limiting accesibility to page
    const cards: CardConfig[] = [
        {
            id: 0,
            title: "Desk Management",
            description: "Manage desks registered in the system",
            allowExpand: false,
            content: undefined
        },
        {
            id: 1,
            title: "Database Viewer",
            description: "View and manage database records",
            allowExpand: true,
            content: <DatabaseViewer /> //removed database viewer temporarily
        }
    ];

    return (
        <>
            {isAdmin ? (
                <ExpandableCardContainer cards={cards} title="Admin Page" cardsPerRow={2} />
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