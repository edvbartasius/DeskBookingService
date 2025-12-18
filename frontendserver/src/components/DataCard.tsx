import React from "react";
import { Button, Card } from "react-bootstrap";
import { ArrowsAngleExpand, ArrowsAngleContract } from 'react-bootstrap-icons';

interface DataCardProps {
    title: string;
    description: string;
    children?: React.ReactNode;
    allowExpand?: boolean;
    isExpanded?: boolean;
    onToggleExpand?: (expanded: boolean) => void;
}

const DataCard: React.FC<DataCardProps> = ({ title, description, children, allowExpand = true, isExpanded = false, onToggleExpand }) => {
    const [internalOpen, setInternalOpen] = React.useState(false);
    const open = onToggleExpand !== undefined ? isExpanded : internalOpen;

    const handleToggle = () => {
        if (onToggleExpand) {
            onToggleExpand(!open);
        } else {
            setInternalOpen(!open);
        }
    };

    return (
        <Card>
            <Card.Header className="py-2">
                <div className="position-relative d-flex align-items-start">
                    <div className="flex-grow-1 text-center">
                        <h2>{title}</h2>
                        <p className="fs-6">{description}</p>
                    </div>
                    {allowExpand && <Button
                        variant="outline-secondary"
                        size="sm"
                        onClick={handleToggle}
                        className="d-flex align-items-center justify-content-center"
                    >
                        {open ? <ArrowsAngleContract /> : <ArrowsAngleExpand />}
                    </Button>
                    }
                </div>
            </Card.Header>
            <Card.Body style={{ minHeight: '50vh' }}>
                {/* Additional card functionality goes in children */}
                {children}
            </Card.Body>
        </Card>
    );
}

export default DataCard;