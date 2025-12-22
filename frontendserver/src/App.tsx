import React from 'react';
import { Routes, Route } from 'react-router-dom';
import { UserProvider } from './contexts/UserContext.tsx';
import Navbar from './components/Navbar.tsx';
import HomePage from './pages/HomePage.tsx';
import DeskPage from './pages/DeskPage.tsx';
import ProfilePage from './pages/ProfilePage.tsx';
import AdminPage from './pages/AdminPage.tsx';

function App() {
  return (
    <UserProvider>
      <div className="min-hs-screen flex flex-col min-vh-100">
        <Navbar />
        <span className="flex-grow">
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/home" element={<HomePage />} />
            <Route path="/desks" element={<DeskPage />} />
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/admin" element={<AdminPage />} />
          </Routes>
        </span>
      </div>
    </UserProvider>
  );
}

export default App;
