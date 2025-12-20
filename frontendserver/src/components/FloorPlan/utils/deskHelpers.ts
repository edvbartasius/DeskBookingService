import { DeskType, DeskStatus, DeskColors } from '../types/floorPlan.types.ts';
import { DESK_COLORS } from '../config/constants.ts';

/**
 * Get color scheme based on desk status
 */
export const getDeskColor = (status?: DeskStatus): DeskColors => {
  switch (status) {
    case DeskStatus.Available:
      return DESK_COLORS.available;
    case DeskStatus.Booked:
      return DESK_COLORS.booked;
    case DeskStatus.Unavailable:
      return DESK_COLORS.unavailable;
    default:
      return DESK_COLORS.default;
  }
};

/**
 * Get human-readable label for desk type
 */
export const getDeskTypeLabel = (type: DeskType): string => {
  switch (type) {
    case DeskType.Standard:
      return 'Standard Desk';
    case DeskType.Conference:
      return 'Conference Room';
    default:
      return 'Unknown';
  }
};

/**
 * Get human-readable label for desk status
 */
export const getDeskStatusLabel = (status?: DeskStatus): string => {
  switch (status) {
    case DeskStatus.Available:
      return 'Available';
    case DeskStatus.Booked:
      return 'Booked';
    case DeskStatus.Unavailable:
      return 'Unavailable';
    default:
      return 'Unknown';
  }
};

/**
 * Get Bootstrap badge variant for desk status
 */
export const getStatusBadgeVariant = (status?: DeskStatus): string => {
  switch (status) {
    case DeskStatus.Available:
      return 'success';
    case DeskStatus.Booked:
      return 'danger';
    case DeskStatus.Unavailable:
      return 'secondary';
    default:
      return 'primary';
  }
};
