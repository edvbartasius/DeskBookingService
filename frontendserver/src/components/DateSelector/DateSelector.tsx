import { Button, Modal } from "react-bootstrap";
import { useState } from "react";
import { format, isBefore, startOfDay, addDays, startOfMonth, endOfMonth, getDay } from "date-fns";
import { getReservationDateLabel } from "./utils/dateUtils.ts";

interface DateSelectorProps {
  selectedDate: Date | null;
  onSelectedDateChange: (date: Date | null) => void;
  disabled?: boolean;
  bookedDates?: Date[]; // Dates already booked for the selected desk
  closedDates?: Date[]; // Dates when the office/building is closed
  multiSelect?: boolean; // Enable multi-date selection
  selectedDates?: Date[]; // For multi-select mode
  onSelectedDatesChange?: (dates: Date[]) => void; // For multi-select mode
}

const WEEKDAY_LABELS = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

const sameDay = (a: Date, b: Date): boolean =>
  a.toDateString() === b.toDateString();

export const DateSelector: React.FC<DateSelectorProps> = ({
  selectedDate,
  onSelectedDateChange,
  disabled = false,
  bookedDates = [],
  closedDates = [],
  multiSelect = false,
  selectedDates = [],
  onSelectedDatesChange
}) => {
  const today: Date = startOfDay(new Date());
  const [currentMonth, setCurrentMonth] = useState(new Date());
  const [showModal, setShowModal] = useState(false);

  const isBlackoutDate = (date: Date): boolean => {
    return [...bookedDates, ...closedDates].some((d: Date) => sameDay(d, date));
  };

  const isDateDisabled = (date: Date): boolean => {
    return isBefore(date, today) || isBlackoutDate(date);
  };

  const handleDateClick = (date: Date): void => {
    if (isDateDisabled(date) || disabled) return;

    if (multiSelect && onSelectedDatesChange) {
      // Multi-select mode
      const isAlreadySelected = selectedDates.some(d => sameDay(d, date));

      if (isAlreadySelected) {
        onSelectedDatesChange(selectedDates.filter(d => !sameDay(d, date)));
      } else {
        onSelectedDatesChange([...selectedDates, date]);
      }
    } else {
      // Single-select mode
      const isSameDate = selectedDate && sameDay(selectedDate, date);

      if (isSameDate) {
        onSelectedDateChange(null);
      } else {
        onSelectedDateChange(date);
      }
      setShowModal(false);
    }
  };

  const handlePrevMonth = () => {
    setCurrentMonth(new Date(currentMonth.getFullYear(), currentMonth.getMonth() - 1));
  };

  const handleNextMonth = () => {
    setCurrentMonth(new Date(currentMonth.getFullYear(), currentMonth.getMonth() + 1));
  };

  const generateCalendarDays = (): Date[][] => {
    const monthStart = startOfMonth(currentMonth);
    const monthEnd = endOfMonth(currentMonth);
    const startDate = addDays(monthStart, -getDay(monthStart));
    const endDate = addDays(monthEnd, 6 - getDay(monthEnd));

    const days: Date[] = [];
    let currentDate = startDate;

    while (currentDate <= endDate) {
      days.push(new Date(currentDate));
      currentDate = addDays(currentDate, 1);
    }

    const weeks: Date[][] = [];
    for (let i = 0; i < days.length; i += 7) {
      weeks.push(days.slice(i, i + 7));
    }

    return weeks;
  };

  const getDayBackgroundColor = (
    isSelected: boolean,
    isBlackout: boolean,
    isToday: boolean
  ): string => {
    if (isSelected) return "#0d6efd";
    if (isBlackout) return "#dc3545";
    if (isToday) return "#e9ecef";
    return "transparent";
  };

  const getDayTextColor = (
    isSelected: boolean,
    isBlackout: boolean,
    isCurrentMonth: boolean
  ): string => {
    if (isSelected || isBlackout) return "white";
    if (!isCurrentMonth) return "#adb5bd";
    return "inherit";
  };

  const renderCalendarDay = (date: Date, dayIdx: number) => {
    const isCurrentMonth = date.getMonth() === currentMonth.getMonth();
    const isSelected = multiSelect
      ? selectedDates.some(d => sameDay(d, date))
      : selectedDate ? sameDay(selectedDate, date) : false;
    const isDisabled = isDateDisabled(date);
    const isBlackout = isBlackoutDate(date);
    const isToday = sameDay(date, today);

    return (
      <div
        key={dayIdx}
        onClick={() => !isDisabled && handleDateClick(date)}
        className={`text-center p-2 ${isDisabled ? "text-muted" : "cursor-pointer"}`}
        style={{
          flex: 1,
          cursor: isDisabled ? "not-allowed" : "pointer",
          backgroundColor: getDayBackgroundColor(isSelected, isBlackout, isToday),
          color: getDayTextColor(isSelected, isBlackout, isCurrentMonth),
          borderRadius: "4px",
          fontSize: "0.9rem",
          opacity: isDisabled ? 0.5 : 1,
        }}
      >
        {date.getDate()}
      </div>
    );
  };

  const renderCalendar = () => {
    const weeks = generateCalendarDays();

    return (
      <div>
        <div className="d-flex justify-content-between align-items-center mb-3">
          <Button variant="outline-secondary" size="sm" onClick={handlePrevMonth}>
            &lt;
          </Button>
          <h6 className="mb-0">{format(currentMonth, "MMMM yyyy")}</h6>
          <Button variant="outline-secondary" size="sm" onClick={handleNextMonth}>
            &gt;
          </Button>
        </div>

        <div className="calendar-grid">
          <div className="d-flex mb-2">
            {WEEKDAY_LABELS.map((day) => (
              <div key={day} className="text-center fw-bold" style={{ flex: 1, fontSize: "0.85rem" }}>
                {day}
              </div>
            ))}
          </div>

          {weeks.map((week, weekIdx) => (
            <div key={weekIdx} className="d-flex mb-1">
              {week.map((date, dayIdx) => renderCalendarDay(date, dayIdx))}
            </div>
          ))}
        </div>
      </div>
    );
  };

  const getSelectedDatesLabel = () => {
    if (selectedDates.length === 0) return "No dates selected";
    if (selectedDates.length === 1) return getReservationDateLabel(selectedDates[0]);
    return `${selectedDates.length} dates selected`;
  };

  return (
    <>
      {!multiSelect ? (
        <div className="d-flex align-items-center gap-2">
          <span className="text-muted">Date:</span>
          <strong>
            {selectedDate ? getReservationDateLabel(selectedDate) : "Not selected"}
          </strong>
          <Button
            variant="outline-primary"
            size="sm"
            onClick={() => setShowModal(true)}
            disabled={disabled}
          >
            {selectedDate ? "Change Date" : "Select Date"}
          </Button>
        </div>
      ) : (
        <div className="d-flex align-items-center gap-2">
          <span className="text-muted">Dates:</span>
          <strong>{getSelectedDatesLabel()}</strong>
          <Button
            variant="outline-primary"
            size="sm"
            onClick={() => setShowModal(true)}
            disabled={disabled}
          >
            {selectedDates.length > 0 ? "Change Dates" : "Select Dates"}
          </Button>
        </div>
      )}

      <Modal show={showModal} onHide={() => setShowModal(false)} centered>
        <Modal.Header closeButton>
          <Modal.Title>
            {multiSelect ? "Select Booking Dates" : "Select Booking Date"}
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {renderCalendar()}
          {multiSelect && selectedDates.length > 0 && (
            <div className="mt-3">
              <small className="text-muted">
                Selected: {selectedDates.map(d => format(d, "MMM d")).join(", ")}
              </small>
            </div>
          )}
        </Modal.Body>
        {multiSelect && (
          <Modal.Footer>
            <Button variant="secondary" onClick={() => setShowModal(false)}>
              Done
            </Button>
          </Modal.Footer>
        )}
      </Modal>
    </>
  );
};

export default {
  DateSelector
};
