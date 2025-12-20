import { Button, Alert } from "react-bootstrap";
import { DailyReservation, TimeSlot } from "../../types/reservation";
import { DailyReservationItem } from "./DailyReservationItem.tsx";

interface ReservationListProps {
  reservations: DailyReservation[];
  onUpdateReservation: (index: number, reservation: DailyReservation) => void;
  onRemoveReservation: (index: number) => void;
  onCopyToAll: (sourceIndex: number) => void;
  onClearAll: () => void;
}

export const ReservationList = ({
  reservations,
  onUpdateReservation,
  onRemoveReservation,
  onCopyToAll,
  onClearAll
}: ReservationListProps) => {
  if (reservations.length === 0) {
    return (
      <Alert variant="info" className="text-center">
        Select a date range and click "Generate Dates" to start planning your reservations.
      </Alert>
    );
  }

  const getTotalHours = (): number => {
    return reservations.reduce((total, reservation) => {
      const dayTotal = reservation.timeSlots.reduce((daySum, slot) => {
        if (!slot.startTime || !slot.endTime) return daySum;

        const start = new Date(`2000-01-01T${slot.startTime}`);
        const end = new Date(`2000-01-01T${slot.endTime}`);
        const hours = (end.getTime() - start.getTime()) / (1000 * 60 * 60);

        return daySum + hours;
      }, 0);
      return total + dayTotal;
    }, 0);
  };

  const hasAnyErrors = (): boolean => {
    return reservations.some(reservation => {
      // Check for invalid time slots
      const hasInvalidSlots = reservation.timeSlots.some(
        slot => !slot.startTime || !slot.endTime || slot.startTime >= slot.endTime
      );

      // Check for overlapping slots
      const hasOverlaps = checkOverlappingSlots(reservation.timeSlots);

      return hasInvalidSlots || hasOverlaps;
    });
  };

  const checkOverlappingSlots = (slots: TimeSlot[]): boolean => {
    for (let i = 0; i < slots.length; i++) {
      for (let j = i + 1; j < slots.length; j++) {
        const slot1 = slots[i];
        const slot2 = slots[j];

        if (slot1.startTime && slot1.endTime && slot2.startTime && slot2.endTime) {
          if (
            (slot1.startTime < slot2.endTime && slot1.endTime > slot2.startTime) ||
            (slot2.startTime < slot1.endTime && slot2.endTime > slot1.startTime)
          ) {
            return true;
          }
        }
      }
    }
    return false;
  };

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <div>
          <h5 className="mb-1">Reservation Schedule</h5>
          <small className="text-muted">
            {reservations.length} day{reservations.length !== 1 ? 's' : ''} • {getTotalHours().toFixed(1)} total hours
          </small>
        </div>
        <Button
          variant="outline-danger"
          size="sm"
          onClick={onClearAll}
        >
          Clear All
        </Button>
      </div>

      {hasAnyErrors() && (
        <Alert variant="warning" className="mb-3">
          <strong>Warning:</strong> Some time slots have errors. Please fix them before booking.
        </Alert>
      )}

      <div style={{ maxHeight: '60vh', overflowY: 'auto' }}>
        {reservations.map((reservation, index) => (
          <div key={reservation.date.toISOString()} className="position-relative">
            <DailyReservationItem
              reservation={reservation}
              onUpdate={(updated) => onUpdateReservation(index, updated)}
              onRemove={() => onRemoveReservation(index)}
            />
            {reservations.length > 1 && (
              <div className="position-absolute" style={{ top: '10px', right: '100px' }}>
                <Button
                  variant="outline-secondary"
                  size="sm"
                  onClick={() => onCopyToAll(index)}
                  title="Copy this day's time slots to all other days"
                >
                  Copy to All
                </Button>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};

export default {
  ReservationList
};