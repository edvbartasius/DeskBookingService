import { format, startOfDay, differenceInCalendarDays } from "date-fns";

/**
 * Formats a reservation date with relative labels (Today, Tomorrow, etc.)
 * @param date The date to format
 * @returns Formatted date string with relative label
 * @example "Today, Friday, December 20, 2025"
 * @example "Tomorrow, Saturday, December 21, 2025"
 * @example "In 4 days, Monday, December 24, 2025"
 */
export const getReservationDateLabel = (date: Date): string => {
  const today = startOfDay(new Date());
  const target = startOfDay(date);

  const diff = differenceInCalendarDays(target, today);
  const weekday = format(target, "EEEE");
  const formattedDate = format(target, 'MMMM d, yyyy');

  if (diff === 0) {
    return `Today, ${weekday}, ${formattedDate}`;
  } else if (diff === 1) {
    return `Tomorrow, ${weekday}, ${formattedDate}`;
  } else if (diff > 1 && diff <= 7) {
    return `In ${diff} days, ${weekday}, ${formattedDate}`;
  }

  return `${weekday}, ${formattedDate}`;
};
