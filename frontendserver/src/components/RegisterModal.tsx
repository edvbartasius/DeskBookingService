import React, { useState } from "react";
import { Modal, Form, Button } from "react-bootstrap";
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

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        
        try {
            const registrationData = {
                name,
                surname,
                email,
                password,
            };
            console.log("Submitting registration data:", registrationData);
            const response = await api.post("users/register", registrationData);
            console.log("Registration successful:", response.data);
        } catch (error: any) {
            console.error("Registration failed:", error);
        }
        onHide();
    };

    return (
        <Modal show={show} onHide={onHide} centered>
            <Modal.Header closeButton>
                <Modal.Title>Register</Modal.Title>
            </Modal.Header>
            <Modal.Body>
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
            </Modal.Body>
        </Modal>
    );
};

export default RegisterModal;
