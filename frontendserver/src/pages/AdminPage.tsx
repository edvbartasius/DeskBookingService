import React from "react";
import { Container, Row, Col, Card } from "react-bootstrap";
import ExpandableCardContainer, { CardConfig } from "../components/ExpandableCardContainer.tsx";

const AdminPage = () => {
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
            content: undefined
        }
    ];

    return <ExpandableCardContainer cards={cards} title="Admin Page" cardsPerRow={2} />;
}

export default AdminPage;