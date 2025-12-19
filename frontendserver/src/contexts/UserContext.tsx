import React, { useEffect, useState, createContext, useContext } from "react";
import { User, UserRole} from "../types/database.types.tsx"


interface UserContextType {
    loggedInUser: User | null;
    setUser: (userData: User | null) => void;
    isAdmin: boolean;
    clearUser: () => void;
}
const UserContext = createContext<UserContextType | undefined>(undefined);

// Provides session-like functionality without implementing actual authentification/authorization
export const UserProvider: React.FC<{children: React.ReactNode}> = ({ children }) => {
    const [loggedInUser, setLoggedInUserState] = useState<User | null>(null);

    // Load from localStorage on mount
    useEffect(() => {
        const stored = localStorage.getItem('loggedInUser');
        if (stored) {
            setLoggedInUserState(JSON.parse(stored));
        }
    }, []);
    
    const setUser = (userData: User | null) => {
        setLoggedInUserState(userData);
        if (userData) {
            localStorage.setItem('loggedInUser', JSON.stringify(userData));
        }
    };

    const isAdmin = loggedInUser?.role === UserRole.Admin;

    const clearUser = () => {
        setLoggedInUserState(null);
        localStorage.removeItem('loggedInUser');
    };

    return (
        <UserContext.Provider value={{ loggedInUser, setUser, isAdmin, clearUser }}>
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