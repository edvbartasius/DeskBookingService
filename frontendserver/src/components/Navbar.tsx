import React, { useState } from 'react';
import Nav from 'react-bootstrap/Nav';
import BootstrapNavbar from 'react-bootstrap/Navbar';
import Container from 'react-bootstrap/Container';
import Button from 'react-bootstrap/Button';
import { Link, NavLink } from 'react-router-dom';
import LoginModal from './LoginModal.tsx';
import RegisterModal from './RegisterModal.tsx';
import { useUser } from '../contexts/UserContext.tsx';

const Navbar = () => {
    const [showLogin, setShowLogin] = useState(false);
    const [showRegister, setShowRegister] = useState(false);
    const { loggedInUser, isAdmin, clearUser } = useUser();

    const handleSwitchToRegister = () => {
        setShowLogin(false);
        setShowRegister(true);
    };

    const handleSwitchToLogin = () => {
        setShowRegister(false);
        setShowLogin(true);
    };

    return (
        <>
            <BootstrapNavbar expand="lg" bg="dark" variant="dark">
                <Container>
                    <BootstrapNavbar.Brand as={Link} to="/home" className="d-flex align-items-center gap-2">
                        <img
                            src="/logo512.png"
                            alt="Desk Booking Service Logo"
                            height="30"
                            style={{ filter: 'invert(1)' }}
                        />
                        Desk Booking Service
                    </BootstrapNavbar.Brand>
                    <BootstrapNavbar.Toggle aria-controls="basic-navbar-nav" />
                    <BootstrapNavbar.Collapse id="basic-navbar-nav">
                        <Nav className="me-auto gap-1">
                            <Nav.Link as={NavLink} to="/desks">Booking</Nav.Link>
                            {loggedInUser && 
                                <Nav.Link as={NavLink} to="/profile">Profile</Nav.Link>
                            }
                            {isAdmin &&
                                <Nav.Link as={NavLink} to="/admin">Admin Dashboard</Nav.Link>
                            }
                        </Nav>

                        <div className="ms-auto d-flex flex-row align-items-center gap-2">
                            {loggedInUser ? (
                                <>
                                    <span className="text-light">Hello, {loggedInUser.name} {loggedInUser.surname}</span>
                                    <Button variant="outline-light" onClick={clearUser}>
                                        Logout
                                    </Button>
                                </>
                            ) : (
                                <>
                                    <Button variant="outline-light" onClick={() => setShowLogin(true)}>
                                        Login
                                    </Button>
                                    <Button variant="light" onClick={() => setShowRegister(true)}>
                                        Register
                                    </Button>
                                </>
                            )}
                        </div>
                    </BootstrapNavbar.Collapse>
                </Container>
            </BootstrapNavbar>

            <LoginModal
                show={showLogin}
                onHide={() => setShowLogin(false)}
                onSwitchToRegister={handleSwitchToRegister}
            />
            <RegisterModal
                show={showRegister}
                onHide={() => setShowRegister(false)}
                onSwitchToLogin={handleSwitchToLogin}
            />
        </>
    );
}
export default Navbar;