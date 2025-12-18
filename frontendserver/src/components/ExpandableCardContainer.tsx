import React, { useState, useEffect, ReactNode } from "react";
import { Container, Row } from "react-bootstrap";
import { motion } from 'framer-motion';
import DataCard from "./DataCard.tsx";

export interface CardConfig {
    id: number;
    title: string;
    description: string;
    allowExpand?: boolean;
    content?: ReactNode;
}

interface ExpandableCardContainerProps {
    cards: CardConfig[];
    title?: string;
    cardsPerRow?: 2 | 3 | 4;
}

const ExpandableCardContainer: React.FC<ExpandableCardContainerProps> = ({
    cards,
    title,
    cardsPerRow = 3
}) => {
    
    const [expandedCard, setExpandedCard] = useState<number | null>(null);
    const [reorderedCard, setReorderedCard] = useState<number | null>(null);
    const [isInitialRender, setIsInitialRender] = useState(true);

    useEffect(() => {
        setIsInitialRender(false);
    }, []);

    // Calculate visual order for each card
    const getOrder = (cardId: number) => {
        if (reorderedCard === null) return cardId;
        if (cardId === reorderedCard) return 0;
        return cardId < reorderedCard ? cardId + 1 : cardId;
    };

    const handleToggleExpand = (cardIndex: number) => (expanded: boolean) => {
        if (expanded) {
            setReorderedCard(cardIndex);
            setTimeout(() => setExpandedCard(cardIndex), 500);
        } else {
            setExpandedCard(null);
            setTimeout(() => setReorderedCard(null), 500);
        }
    };

    // Get the base width for non-expanded cards based on cardsPerRow
    const getBaseWidth = () => {
        const widthMap = {
            2: 'calc(50% - 1rem)',
            3: 'calc(33.333% - 1rem)',
            4: 'calc(25% - 1rem)'
        };
        return widthMap[cardsPerRow];
    };

    return (
        <div className="page-container">
            <Container fluid className="page-container">
                {title && <h1>{title}</h1>}
                <Row className="mt-8 px-5 g-4 card-container" style={{ display: 'flex', flexWrap: 'wrap' }}>
                    {cards.map((card) => (
                        <motion.div
                            key={card.id}
                            layout
                            data-expanded={expandedCard === card.id}
                            animate={{
                                width: expandedCard === card.id ? '100%' : getBaseWidth()
                            }}
                            transition={isInitialRender ? { duration: 0 } : {
                                layout: {
                                    type: "spring",
                                    stiffness: 250,
                                    damping: 30
                                },
                                width: {
                                    type: "spring",
                                    stiffness: 250,
                                    damping: 30
                                }
                            }}
                            initial={false}
                            style={{
                                order: getOrder(card.id),
                                marginBottom: '1rem'
                            }}
                        >
                            <DataCard
                                title={card.title}
                                description={card.description}
                                allowExpand={card.allowExpand ?? true}
                                isExpanded={expandedCard === card.id}
                                onToggleExpand={handleToggleExpand(card.id)}
                            >
                                {card.content}
                            </DataCard>
                        </motion.div>
                    ))}
                </Row>
            </Container>
        </div>
    );
};

export default ExpandableCardContainer;
