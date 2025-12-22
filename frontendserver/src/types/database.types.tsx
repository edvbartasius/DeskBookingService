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
    id: string;
    name: string;
    surname: string;
    email: string;
    password?: string; // password may be omitted when DTOs are returned
    role: UserRole;
}

export interface Building {
    id: number;
    name: string;
    floorPlanWidth: number;
    floorPlanHeight: number;
}

export interface Desk {
    id: number;
    deskNumber: string;
    description?: string | null;
    status: DeskStatus;
    buildingId: number;
    buildingName?: string | null;
    positionX: number;
    positionY: number;
    type: number; // DeskType enum
    isInMaintenance: boolean;
    maintenanceReason?: string | null;
}

export enum ReservationStatus {
    Active = 0,
    Completed = 1,
    Cancelled = 2
}

export interface Reservation {
    id: number;
    userId: string;
    deskId: number;
    reservationDate: string; // ISO date string
    status: ReservationStatus;
    createdAt: string; // ISO date string
    canceledAt?: string | null; // ISO date string
    reservationGroupId?: string | null; // GUID for grouping reservations made together
}