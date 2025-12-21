import { useState, useEffect } from "react";
import api from "../../../services/api.ts";

interface UseDeskAvailabilityResult {
  bookedDates: Date[];
  loading: boolean;
  error: Error | null;
  refetch: () => void;
}

/**
 * Custom hook to fetch booked dates for a specific desk
 *
 * @param deskId - The ID of the desk to fetch reservations for
 * @returns Object containing booked dates, loading state, error, and refetch function
 *
 * @example
 * ```tsx
 * const { bookedDates, loading, error } = useDeskAvailability(selectedDesk?.id);
 *
 * <DateSelector
 *   selectedDate={selectedDate}
 *   onSelectedDateChange={setSelectedDate}
 *   bookedDates={bookedDates}
 *   disabled={loading}
 * />
 * ```
 */
export const useDeskAvailability = (deskId: number | undefined): UseDeskAvailabilityResult => {
  const [bookedDates, setBookedDates] = useState<Date[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);
  const [refetchTrigger, setRefetchTrigger] = useState(0);

  useEffect(() => {
    if (!deskId) {
      setBookedDates([]);
      setLoading(false);
      setError(null);
      return;
    }

    const fetchBookedDates = async () => {
      setLoading(true);
      setError(null);

      try {
        const response = await api.get(`reservations/desk/${deskId}`);

        if (response.status === 200) {
          // Backend returns array of DateOnly strings (e.g., ["2025-12-25", "2025-12-26"])
          const dateStrings: string[] = response.data || [];

          console.log(`Fetched ${dateStrings.length} booked dates for desk ${deskId}`);
          console.log(`bookedDates: ${dateStrings}`);

          // Convert date strings to Date objects in local timezone
          // Parse "2025-12-25" as Dec 25 at midnight local time (not UTC)
          const dates = dateStrings.map((dateStr: string) => {
            const [year, month, day] = dateStr.split('-').map(Number);
            return new Date(year, month - 1, day);
          });
          setBookedDates(dates);
        }
      } catch (err) {
        const error = err instanceof Error ? err : new Error("Unknown error occurred");
        setError(error);
        console.error("Failed to fetch booked dates:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchBookedDates();
  }, [deskId, refetchTrigger]);

  const refetch = () => {
    setRefetchTrigger(prev => prev + 1);
  };

  return { bookedDates, loading, error, refetch };
};
