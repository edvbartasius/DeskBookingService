// src/types/booking.types.tsx

export type Weekday =
    | 'monday'
    | 'tuesday'
    | 'wednesday'
    | 'thursday'
    | 'friday'
    | 'saturday'
    | 'sunday';

export enum DeskStatus {
    Available = 'Available',
    Booked = 'Booked',
    Unavailable = 'Unavailable'
}

export enum ReservationStatus {
    Active = 0,
    Completed = 1,
    Cancelled = 2
}

export enum DeskType {
    RegularDesk = 0,
    ConferenceRoom = 1
}

/**
 * Desk representation matching backend DeskDto
 */
export interface Desk {
    id: number;
    description?: string;
    buildingId: number;
    positionX: number;
    positionY: number;
    type: DeskType;
}

/**
 * Desk with position for floor plan rendering
 */
export interface FloorPlanDesk extends Desk {
    label?: string;
    width?: number;
    height?: number;
    rotation?: number;
}

/**
 * User representation matching backend UserDto
 */
export interface User {
    id: string;
    name?: string;
    surname?: string;
    email: string;
    role: number; // UserRole enum
}

/**
 * Reservation (full-day booking)
 */
export interface Reservation {
    id: number;
    userId: string;
    deskId: number;
    reservationDate: string; // ISO date string (yyyy-MM-dd)
    status: ReservationStatus;
    createdAt: string; // ISO datetime string
    canceledAt?: string; // ISO datetime string
    user?: User;
    desk?: Desk;
}

/**
 * Request payload for creating reservations
 */
export interface CreateReservationRequest {
    deskId: number;
    reservationDates: string[]; // Array of ISO date strings
}

/**
 * Response from creating reservations
 */
export interface CreateReservationResult {
    success: boolean;
    errorMessage?: string;
    createdReservations: Reservation[];
    failedDates: string[];
}

/**
 * Status for a specific desk on a specific date
 */
export interface DeskStatusInfo {
    deskId: number;
    date: string; // ISO date string
    status: DeskStatus;
    reservedByFirstName?: string;
    reservedByLastName?: string;
    reservedByUserId?: string;
    unavailableReason?: string;
}

/**
 * Desk availability for a date range
 */
export interface DeskAvailability {
    deskId: number;
    description?: string;
    positionX: number;
    positionY: number;
    type: DeskType;
    statusByDate: DeskStatusInfo[];
}

/**
 * Building operating hours by weekday
 */
export interface OperatingHours {
    id: number;
    buildingId: number;
    dayOfWeek: number; // 0 = Sunday, 6 = Saturday
    openingTime: string; // HH:mm:ss format
    closingTime: string; // HH:mm:ss format
    isClosed: boolean;
}

/**
 * Building
 */
export interface Building {
    id: number;
    name: string;
    floorPlanWidth: number;
    floorPlanHeight: number;
    desks?: Desk[];
    operatingHours?: OperatingHours[];
}

/**
 * Floor plan with desks
 */
export interface FloorPlan {
    buildingName: string;
    floorPlanDesks: FloorPlanDesk[];
}

/**
 * Cancel date range request
 */
export interface CancelDateRangeRequest {
    deskId: number;
    startDate: string; // ISO date string
    endDate: string; // ISO date string
}
