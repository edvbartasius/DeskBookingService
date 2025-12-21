import React, { useState } from "react";
import { Modal, Form, Button, Col, Row } from "react-bootstrap";
import api from "../services/api.ts";

interface RegisterModalProps {
    show: boolean;
    onHide: () => void;
    onSwitchToLogin: () => void;
}

const RegisterModal: React.FC<RegisterModalProps> = ({ show, onHide, onSwitchToLogin }) => {
    const [name, setName] = useState("");
    const [surname, setSurname] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [errorMessage, setErrorMessage] = useState("");
    const [isSuccess, setIsSuccess] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setErrorMessage("");
        try {
            const registrationData = {
                name,
                surname,
                email,
                password,
            };
            console.log("Submitting registration data:", registrationData);
            const response = await api.post("users/register-user", registrationData);
            if (response.status === 200)
            {
                console.log("Registration successful:", response.data);
                setIsSuccess(true); // Show success message
            } else {
                // Display error message
                setErrorMessage(response.data || "Login failed");
                console.log("Registration failed", response.data);
            }
        } catch (error: any) {
            console.error("Registration failed:", error);
            const message = error.response?.data || error.message || "An error occured during registration";
            setErrorMessage(message);
        }
    };

    const handleClose = () => {
        setIsSuccess(false);
        setErrorMessage("");
        setName("");
        setSurname("");
        setEmail("");
        setPassword("");
        onHide();
    };

    const handleGoToLogin = () => {
        setIsSuccess(false);
        setErrorMessage("");
        setName("");
        setSurname("");
        setEmail("");
        setPassword("");
        onSwitchToLogin();
    };

    return (
        <Modal show={show} onHide={handleClose} centered>
            <Modal.Header closeButton>
                <Modal.Title>Register</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                {isSuccess ? (
                    // Success view
                    <div className="text-center py-4">
                        <h4 className="text-success mb-3 text-start">Registration Successful!</h4>
                        <p className="text-muted mb-4 text-start">
                            Your account has been created successfully. You can now log in to start booking desks.
                        </p>
                    </div>
                ) : (
                    // Registration form view
                    <>
                        {errorMessage && (
                            <div className="alert alert-danger" role="alert">
                                {errorMessage}
                            </div>
                        )}
                        <Form onSubmit={handleSubmit}>
                            <Form.Group className="mb-3">
                                <Form.Label>Name</Form.Label>
                                <Form.Control
                                    type="text"
                                    placeholder="Enter your name"
                                    value={name}
                                    onChange={(e) => setName(e.target.value)}
                                    required
                                />
                            </Form.Group>

                            <Form.Group className="mb-3">
                                <Form.Label>Surname</Form.Label>
                                <Form.Control
                                    type="text"
                                    placeholder="Enter your surname"
                                    value={surname}
                                    onChange={(e) => setSurname(e.target.value)}
                                    required
                                />
                            </Form.Group>

                            <Form.Group className="mb-3">
                                <Form.Label>Email</Form.Label>
                                <Form.Control
                                    type="email"
                                    placeholder="Enter your email"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    required
                                />
                            </Form.Group>

                            <Form.Group className="mb-3">
                                <Form.Label>Password</Form.Label>
                                <Form.Control
                                    type="password"
                                    placeholder="Enter your password"
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    required
                                />
                            </Form.Group>

                            <Button variant="primary" type="submit" className="w-100">
                                Register
                            </Button>
                        </Form>

                        <div className="text-center mt-3">
                            <span className="text-muted">Already have an account? </span>
                            <Button variant="link" onClick={onSwitchToLogin} className="p-0">
                                Login
                            </Button>
                        </div>
                    </>
                )}
            </Modal.Body>
            {isSuccess && (
                <Modal.Footer className="d-flex justify-content-center gap-2">
                    <Row className="w-100">
                        <Col>
                            <Button variant="outline-secondary" onClick={handleClose} className="w-100">
                                Close
                            </Button>
                        </Col>
                        <Col>
                            <Button variant="primary" onClick={handleGoToLogin} className="w-100">
                                Go to Login
                            </Button>
                        </Col>
                    </Row>
                </Modal.Footer>
            )}
        </Modal>
    );
};

export default RegisterModal;
