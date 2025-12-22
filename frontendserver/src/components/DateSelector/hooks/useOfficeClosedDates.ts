import { useState, useEffect } from "react";
import { getDay, addDays, startOfDay } from "date-fns";
import api from "../../../services/api.ts";

interface UseOfficeClosedDatesResult {
  closedDates: Date[];
  loading: boolean;
  error: Error | null;
  refetch: () => void;
}

/**
 * Generates an array of Date objects representing closed days over the next year
 * based on day-of-week numbers (0=Sunday, 1=Monday, etc.)
 */
const generateClosedDatesFromDayOfWeek = (closedDayNumbers: number[]): Date[] => {
  const dates: Date[] = [];
  const today = startOfDay(new Date());
  const daysToGenerate = 120; 

  for (let i = 0; i < daysToGenerate; i++) {
    const currentDate = addDays(today, i);
    const dayOfWeek = getDay(currentDate); // 0=Sunday, 1=Monday, etc.

    if (closedDayNumbers.includes(dayOfWeek)) {
      dates.push(currentDate);
    }
  }

  return dates;
};

/**
 * Custom hook to fetch office closed dates
 *
 * @param buildingId - The building ID to fetch closed dates for
 * @returns Object containing closed dates, loading state, error, and refetch function
 *
 * @example
 * ```tsx
 * const { closedDates, loading } = useOfficeClosedDates(selectedBuilding?.id);
 *
 * <DateSelector
 *   selectedDate={selectedDate}
 *   onSelectedDateChange={setSelectedDate}
 *   closedDates={closedDates}
 *   disabled={loading}
 * />
 * ```
 */
export const useOfficeClosedDates = (buildingId: number | undefined): UseOfficeClosedDatesResult => {
  const [closedDates, setClosedDates] = useState<Date[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);
  const [refetchTrigger, setRefetchTrigger] = useState(0);

  useEffect(() => {
    // Don't fetch if no building is selected
    if (!buildingId) {
      setClosedDates([]);
      setLoading(false);
      return;
    }

    const fetchClosedDates = async () => {
      setLoading(true);
      setError(null);

      try {
        const response = await api.get(`buildings/closed-dates/${buildingId}`);

        if (response.status === 200) {
          // Backend returns DayOfWeek numbers (0=Sunday, 1=Monday, etc.)
          // Convert these to actual Date objects for the next year
          const closedDayOfWeekNumbers: number[] = response.data || [];
          const dates = generateClosedDatesFromDayOfWeek(closedDayOfWeekNumbers);
          setClosedDates(dates);
        }
      } catch (err) {
        const error = err instanceof Error ? err : new Error("Unknown error occurred");
        setError(error);
        console.error("Failed to fetch closed dates:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchClosedDates();
  }, [buildingId, refetchTrigger]);

  const refetch = () => {
    setRefetchTrigger(prev => prev + 1);
  };

  return { closedDates, loading, error, refetch };
};
