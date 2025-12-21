export interface DailyReservation {
  date: Date;
}

export interface DateRange {
  start: Date | null;
  end: Date | null;
}

export interface BookingRequest {
  deskId: number;
  reservationDates: string[]; // Array of ISO date strings "YYYY-MM-DD"
}
