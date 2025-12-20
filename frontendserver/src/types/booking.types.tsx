// src/types/booking.types.tsx

export type Weekday =
    | 'monday'
    | 'tuesday'
    | 'wednesday'
    | 'thursday'
    | 'friday'
    | 'saturday'
    | 'sunday';

/**
 * Simple start / end pair.
 * Use ISO 8601 strings (e.g. "2025-01-01T09:00:00Z") or Date objects where convenient.
 */
export interface TimeSpan {
    start: string | Date;
    end: string | Date;
}

/**
 * Desk representation on a floor plan with positioning metadata.
 * No actual data included — fill as needed.
 */
export interface FloorPlanDesk {
    id: string;
    label?: string;
    buildingId?: string;
    // position in pixels, percentages, or grid units depending on your renderer
    posX: number;
    posY: number;
    width?: number;
    height?: number;
    rotation?: number;
}

/**
 * Availability for a single desk on a given date.
 * Each slot indicates whether that timespan is available.
 */
// export interface DeskAvailability {
//     deskId: string;
//     // ISO date (yyyy-mm-dd) or Date — choose one shape for your API
//     date: string;
//     slots: Array<{
//         timespan: TimeSpan;
//         available: boolean;
//         reservationId?: string;
//     }>;
//     computedAt?: string | Date;
// }

/**
 * Request payload when creating a reservation.
 */
export interface ReservationRequest {
    userId: string;
    deskId: string;
    timespan: TimeSpan;
    purpose?: string;
    attendees?: number;
    // Optional recurrence support
    recurring?: {
        frequency: 'daily' | 'weekly' | 'monthly';
        interval?: number; // every N units
        until?: string | Date;
        byWeekday?: Weekday[];
    } | null;
    metadata?: Record<string, any>;
}

/**
 * Representation of a user's reservation as stored/returned by the system.
 */
export interface UserReservation {
    id: string;
    userId: string;
    deskId: string;
    timespan: TimeSpan;
    status: 'pending' | 'confirmed' | 'cancelled' | 'completed';
    createdAt: string | Date;
    updatedAt?: string | Date;
    notes?: string;
    source?: 'web' | 'mobile' | 'api';
    metadata?: Record<string, any>;
}

/**
 * Building operating hours by weekday.
 * Each weekday can have zero or more open timespans.
 */
export interface BuildingHours {
    hours: Partial<Record<Weekday, TimeSpan[]>>;
}