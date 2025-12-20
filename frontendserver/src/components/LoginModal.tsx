import React, { useState } from "react";
import { Modal, Form, Button } from "react-bootstrap";
import api from "../services/api.ts";
import { useUser } from "../contexts/UserContext.tsx"

interface LoginModalProps {
    show: boolean;
    onHide: () => void;
    onSwitchToRegister: () => void;
}

const LoginModal: React.FC<LoginModalProps> = ({ show, onHide, onSwitchToRegister }) => {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [errorMessage, setErrorMessage] = useState("");
    const { setUser } = useUser();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setErrorMessage("");
        try {
            const loginData = {
                email,
                password
            };
            console.log("Submitting login data:", loginData);
            const response = await api.post("users/login-user", loginData);
            if (response.status === 200) { // success
                // TODO: Remove password from DTO when returning User object from backend
                console.log("Login successful", response.data);
                console.log("User role from backend:", response.data.role);

                // Store User context for session-like functionality
                setUser({
                    name: response.data.name,
                    surname: response.data.surname,
                    role: response.data.role
                })
                onHide(); // Hide modal on successfull login
            } else {
                // Display error message
                setErrorMessage(response.data || "Login failed");
                console.log("Login failed:", response.data);
            }
        } catch (error: any)
        {
            console.error("Login failed:", error);
            const message = error.response?.data || error.message || "An error occurred during login";
            setErrorMessage(message);
        }
    };

    return (
        <Modal show={show} onHide={onHide} centered>
            <Modal.Header closeButton>
                <Modal.Title>Login</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                {errorMessage && (
                    <div className="alert alert-danger" role="alert">
                        {errorMessage}
                    </div>
                )}
                <Form onSubmit={handleSubmit}>
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
                        Login
                    </Button>
                </Form>

                <div className="text-center mt-3">
                    <span className="text-muted">Don't have an account? </span>
                    <Button variant="link" onClick={onSwitchToRegister} className="p-0">
                        Register
                    </Button>
                </div>
            </Modal.Body>
        </Modal>
    );
};

export default LoginModal;