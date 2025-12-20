import { Form, Button, Row, Col, InputGroup } from "react-bootstrap";
import { TimeSlot } from "../../types/reservation";

interface TimeSlotEditorProps {
  timeSlot: TimeSlot;
  onUpdate: (timeSlot: TimeSlot) => void;
  onRemove: () => void;
  canRemove: boolean;
}

export const TimeSlotEditor = ({

}: TimeSlotEditorProps) => {
  
};

