// Types for reservation display and management

export interface ReservationDto {
  id: number;
  userId: string;
  deskId: number;
  reservationDate: string; // DateOnly from backend
  status: ReservationStatus;
  createdAt: string;
  canceledAt?: string | null;
  reservationGroupId?: string | null;
  user?: UserDto;
  desk?: DeskDto;
}

export interface DeskDto {
  id: number;
  description?: string;
  buildingId: number;
  positionX: number;
  positionY: number;
  status: number;
  type: number;
}

export interface UserDto {
  id: string;
  name?: string;
  surname?: string;
  email: string;
  role: number;
}

export enum ReservationStatus {
  Active = 0,
  Completed = 1,
  Cancelled = 2
}

// Grouped reservation response from backend
export interface GroupedReservation {
  reservationGroupId: string;
  createdAt: string;
  deskId: number;
  deskDescription?: string;
  buildingName?: string;
  reservationCount: number;
  dates: string[]; // Array of DateOnly strings
  reservations: ReservationDto[];
  hasToday: boolean; // Backend calculates if any date is today
  daysUntilFirst: number; // Backend calculates days until first reservation
}

