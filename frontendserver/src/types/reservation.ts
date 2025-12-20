export interface TimeSlot {
  id: string; // Unique identifier for React keys
  startTime: string; // "HH:mm:ss" format (e.g., "09:00:00")
  endTime: string;   // "HH:mm:ss" format (e.g., "17:00:00")
}

export interface DailyReservation {
  date: Date;
  timeSlots: TimeSlot[];
}

export interface DateRange {
  start: Date | null;
  end: Date | null;
}

export interface BookingRequest {
  deskId: number;
  reservations: {
    date: string; // ISO date format "YYYY-MM-DD"
    timeSlots: {
      startTime: string;
      endTime: string;
    }[];
  }[];
}
