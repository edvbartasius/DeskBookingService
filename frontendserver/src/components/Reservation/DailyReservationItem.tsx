import { Card, Button, Badge } from "react-bootstrap";
import { DailyReservation, TimeSlot } from "../../types/reservation";
import { TimeSlotEditor } from "./TimeSlotEditor.tsx";
import { v4 as uuidv4 } from 'uuid';

interface DailyReservationItemProps {
  reservation: DailyReservation;
  onUpdate: (reservation: DailyReservation) => void;
  onRemove: () => void;
}

export const DailyReservationItem = ({
  reservation,
  onUpdate,
  onRemove
}: DailyReservationItemProps) => {
  const handleAddTimeSlot = () => {
    const newTimeSlot: TimeSlot = {
      id: uuidv4(),
      startTime: "09:00:00",
      endTime: "17:00:00"
    };
    onUpdate({
      ...reservation,
      timeSlots: [...reservation.timeSlots, newTimeSlot]
    });
  };

  const handleUpdateTimeSlot = (index: number, updatedSlot: TimeSlot) => {
    const newTimeSlots = [...reservation.timeSlots];
    newTimeSlots[index] = updatedSlot;
    onUpdate({
      ...reservation,
      timeSlots: newTimeSlots
    });
  };

  const handleRemoveTimeSlot = (index: number) => {
    const newTimeSlots = reservation.timeSlots.filter((_, i) => i !== index);
    onUpdate({
      ...reservation,
      timeSlots: newTimeSlots
    });
  };

  const formatDate = (date: Date): string => {
    return date.toLocaleDateString('en-US', {
      weekday: 'short',
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  };

  const getDayOfWeek = (date: Date): string => {
    return date.toLocaleDateString('en-US', { weekday: 'long' });
  };

  // Check if there are overlapping time slots
  const hasOverlappingSlots = (): boolean => {
    for (let i = 0; i < reservation.timeSlots.length; i++) {
      for (let j = i + 1; j < reservation.timeSlots.length; j++) {
        const slot1 = reservation.timeSlots[i];
        const slot2 = reservation.timeSlots[j];

        if (slot1.startTime && slot1.endTime && slot2.startTime && slot2.endTime) {
          // Check if slots overlap
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

  const isWeekend = (date: Date): boolean => {
    const day = date.getDay();
    return day === 0 || day === 6; // Sunday or Saturday
  };

  return (
    <Card className="mb-3">
      <Card.Header className="d-flex justify-content-between align-items-center">
        <div>
          <strong>{formatDate(reservation.date)}</strong>
          <Badge
            bg={isWeekend(reservation.date) ? "warning" : "secondary"}
            className="ms-2"
          >
            {getDayOfWeek(reservation.date)}
          </Badge>
          {hasOverlappingSlots() && (
            <Badge bg="danger" className="ms-2">
              Overlapping slots!
            </Badge>
          )}
        </div>
        <Button
          variant="outline-danger"
          size="sm"
          onClick={onRemove}
          title="Remove this date"
        >
          Remove Date
        </Button>
      </Card.Header>
      <Card.Body>
        <div className="mb-2">
          <strong className="text-muted small">Time Slots:</strong>
        </div>
        {/* {reservation.timeSlots.map((slot, index) => (
          <></>
        )} */}
        <Button
          variant="outline-primary"
          size="sm"
          onClick={handleAddTimeSlot}
          className="mt-2"
        >
          + Add Time Slot
        </Button>
      </Card.Body>
    </Card>
  );
};
export default {
  DailyReservationItem
};
