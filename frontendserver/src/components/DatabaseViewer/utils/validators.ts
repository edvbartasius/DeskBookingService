import { TableName } from '../../../types/database.types.tsx';

export interface ValidationError {
  field: string;
  message: string;
}

/**
 * Validates form data before submission
 * Returns null if valid, or an error message if invalid
 */
export const validateFormData = (
  tableName: TableName,
  formData: any
): string | null => {
  switch (tableName.toLowerCase()) {
    case 'users':
      return validateUser(formData);
    case 'buildings':
      return validateBuilding(formData);
    case 'desks':
      return validateDesk(formData);
    case 'reservations':
      return validateReservation(formData);
    default:
      return null;
  }
};

const validateUser = (data: any): string | null => {
  // Required fields
  if (!data.name?.trim()) {
    return 'Name is required';
  }
  if (!data.surname?.trim()) {
    return 'Surname is required';
  }
  if (!data.email?.trim()) {
    return 'Email is required';
  }
  if (!data.password?.trim()) {
    return 'Password is required';
  }

  // Email format validation
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!emailRegex.test(data.email)) {
    return 'Invalid email format';
  }

  // Password length
  if (data.password.length < 6) {
    return 'Password must be at least 6 characters long';
  }

  return null;
};

const validateBuilding = (data: any): string | null => {
  if (!data.name?.trim()) {
    return 'Building name is required';
  }

  return null;
};

const validateDesk = (data: any): string | null => {
  if (!data.buildingId) {
    return 'Building is required';
  }

  return null;
};

const validateReservation = (data: any): string | null => {
  // Required fields
  if (!data.userId?.trim()) {
    return 'User is required';
  }
  if (!data.deskId) {
    return 'Desk is required';
  }
  if (!data.reservationDate) {
    return 'Reservation date is required';
  }
  if (!data.startDate) {
    return 'Start time is required';
  }
  if (!data.endDate) {
    return 'End time is required';
  }

  // Date/time logic validation
  try {
    const startTime = parseTime(data.startDate);
    const endTime = parseTime(data.endDate);

    if (endTime <= startTime) {
      return 'End time must be after start time';
    }
  } catch (e) {
    return 'Invalid time format';
  }

  // Check if reservation date is in the past
  try {
    const reservationDate = new Date(data.reservationDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (reservationDate < today) {
      return 'Reservation date cannot be in the past';
    }
  } catch (e) {
    return 'Invalid date format';
  }

  return null;
};

/**
 * Helper to parse time string to minutes since midnight for comparison
 */
const parseTime = (timeString: string): number => {
  const [hours, minutes] = timeString.split(':').map(Number);
  return hours * 60 + minutes;
};
