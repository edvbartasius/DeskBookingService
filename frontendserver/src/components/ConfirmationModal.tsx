import React from "react";
import { Modal, Button } from "react-bootstrap";

interface ConfirmationModalProps {
    show: boolean;
    onHide: () => void;
    onConfirm: () => void;
    title: string;
    message: string;
    confirmText?: string;
    cancelText?: string;
    variant?: "primary" | "danger" | "warning" | "success";
}

const ConfirmationModal: React.FC<ConfirmationModalProps> = ({
    show,
    onHide,
    onConfirm,
    title,
    message,
    confirmText = "Confirm",
    cancelText = "Cancel",
    variant = "danger"
}) => {
    const handleConfirm = () => {
        onConfirm();
        onHide();
    };

    return (
        <Modal show={show} onHide={onHide} centered>
            <Modal.Header closeButton>
                <Modal.Title>{title}</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <p className="mb-0">{message}</p>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="outline-secondary" onClick={onHide}>
                    {cancelText}
                </Button>
                <Button variant={variant} onClick={handleConfirm}>
                    {confirmText}
                </Button>
            </Modal.Footer>
        </Modal>
    );
};

export default ConfirmationModal;
