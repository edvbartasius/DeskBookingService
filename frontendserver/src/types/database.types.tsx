export enum UserRole {
    Admin = 0,
    User = 1
}

export enum DeskStatus {
    Available = 0,
    Booked = 1,
    Unavailable = 2
}

export enum TableName {
    Users = 'users',
    Buildings = 'buildings',
    Desks = 'desks',
    Reservations = 'reservations'
}

export interface User {
    id?: string;
    name: string;
    surname: string;
    email?: string;
    password?: string; // password may be omitted when DTOs are returned
    role: UserRole;
}

export interface Building {
    id: number;
    name: string;
}

export interface Desk {
    id: number;
    description?: string | null;
    status: DeskStatus;
    buildingId: number;
    buildingName?: string | null;
}

export interface Reservation {
    id: number;
    userId: string;
    deskId: number;
    reservationDate: string; // ISO date string
    startDate: string; // time string
    endDate: string; // time string
}