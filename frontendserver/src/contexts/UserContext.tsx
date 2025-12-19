import React, { useEffect, useState, createContext, useContext } from "react";
import { User, UserRole} from "../types/database.types.tsx"


interface UserContextType {
    user: User | null;
    setUser: (userData: User | null) => void;
    isAdmin: boolean;
    clearUser: () => void;
}
const UserContext = createContext<UserContextType | undefined>(undefined);

// Provides session-like functionality without implementing actual authentification/authorization
export const UserProvider: React.FC<{children: React.ReactNode}> = ({ children }) => {
    const [user, setUserState] = useState<User | null>(null);

    // Load from localStorage on mount
    useEffect(() => {
        const stored = localStorage.getItem('user');
        if (stored) {
            setUserState(JSON.parse(stored));
        }
    }, []);
    
    const setUser = (userData: User | null) => {
        setUserState(userData);
        if (userData) {
            localStorage.setItem('user', JSON.stringify(userData));
        }
    };

    const isAdmin = user?.role === UserRole.Admin;

    const clearUser = () => {
        setUserState(null);
        localStorage.removeItem('user');
    };

    return (
        <UserContext.Provider value={{ user, setUser, isAdmin, clearUser }}>
            {children}
        </UserContext.Provider>
    );
}

export const useUser = () => {
    const context = useContext(UserContext);
    if (!context){
        throw new Error ('useUser must be used within UserProvider');
    }
    return context;
}