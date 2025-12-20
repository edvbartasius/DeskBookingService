import { Form, Row, Col, Button, Card, Badge } from "react-bootstrap";
import { DateRange } from "../../types/reservation";
import { useState } from "react";
import { format, isBefore, startOfDay, addDays, startOfMonth, endOfMonth, getDay } from "date-fns";

interface DateRangeSelectorProps {
  dateRange: DateRange;
  onDateRangeChange: (dateRange: DateRange) => void;
  onGenerateDates: () => void;
  disabled?: boolean;
}

// Mock dates to blackout, fetch operating times from backend
const blackoutDates: Date[] = [
  new Date(2025, 0, 5),
  new Date(2025, 0, 12),
  new Date(2025, 0, 19),
];

const sameDay = (a: Date, b: Date): boolean =>
  a.toDateString() === b.toDateString();

export const DateListSelector: React.FC = () => {
  const today: Date = startOfDay(new Date());
  const tomorrow: Date = addDays(today, 1);

  const [selectedDates, setSelectedDates] = useState<Date[]>([tomorrow]);
  const [currentMonth, setCurrentMonth] = useState(new Date());

  const isDateDisabled = (date: Date): boolean => {
    return (
      isBefore(date, today) ||
      blackoutDates.some((d) => sameDay(d, date))
    );
  };

  const handleDateClick = (date: Date): void => {
    if (isDateDisabled(date)) return;

    const exists = selectedDates.some((d) => sameDay(d, date));

    if (exists) {
      setSelectedDates((prev) => prev.filter((d) => !sameDay(d, date)));
    } else {
      if (selectedDates.length >= 7) {
        return;
      }
      setSelectedDates((prev) => [...prev, date].sort((a, b) => a.getTime() - b.getTime()));
    }
  };

  const handlePrevMonth = () => {
    setCurrentMonth(new Date(currentMonth.getFullYear(), currentMonth.getMonth() - 1));
  };

  const handleNextMonth = () => {
    setCurrentMonth(new Date(currentMonth.getFullYear(), currentMonth.getMonth() + 1));
  };

  const renderCalendar = () => {
    const monthStart = startOfMonth(currentMonth);
    const monthEnd = endOfMonth(currentMonth);
    const startDate = addDays(monthStart, -getDay(monthStart));
    const endDate = addDays(monthEnd, 6 - getDay(monthEnd));

    const days = [];
    let currentDate = startDate;

    while (currentDate <= endDate) {
      days.push(new Date(currentDate));
      currentDate = addDays(currentDate, 1);
    }

    const weeks = [];
    for (let i = 0; i < days.length; i += 7) {
      weeks.push(days.slice(i, i + 7));
    }

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
            {["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"].map((day) => (
              <div key={day} className="text-center fw-bold" style={{ flex: 1, fontSize: "0.85rem" }}>
                {day}
              </div>
            ))}
          </div>

          {weeks.map((week, weekIdx) => (
            <div key={weekIdx} className="d-flex mb-1">
              {week.map((date, dayIdx) => {
                const isCurrentMonth = date.getMonth() === currentMonth.getMonth();
                const isSelected = selectedDates.some((d) => sameDay(d, date));
                const isDisabled = isDateDisabled(date);
                const isBlackout = blackoutDates.some((d) => sameDay(d, date));
                const isToday = sameDay(date, today);

                return (
                  <div
                    key={dayIdx}
                    onClick={() => !isDisabled && handleDateClick(date)}
                    className={`text-center p-2 ${
                      isDisabled ? "text-muted" : "cursor-pointer"
                    }`}
                    style={{
                      flex: 1,
                      cursor: isDisabled ? "not-allowed" : "pointer",
                      backgroundColor: isSelected
                        ? "#0d6efd"
                        : isBlackout
                        ? "#dc3545"
                        : isToday
                        ? "#e9ecef"
                        : "transparent",
                      color: isSelected || isBlackout ? "white" : isCurrentMonth ? "inherit" : "#adb5bd",
                      borderRadius: "4px",
                      fontSize: "0.9rem",
                      opacity: isDisabled ? 0.5 : 1,
                    }}
                  >
                    {date.getDate()}
                  </div>
                );
              })}
            </div>
          ))}
        </div>
      </div>
    );
  };

  return (
    <Card className="shadow-sm">
      <Card.Body>
        <div className="d-flex justify-content-between align-items-center mb-3">
          <h5 className="mb-0">Select Booking Dates</h5>
          <Badge bg={selectedDates.length >= 7 ? "danger" : "secondary"}>
            {selectedDates.length}/7 days
          </Badge>
        </div>
        {renderCalendar()}

        {selectedDates.length > 0 && (
          <div className="mt-3">
            <h6 className="mb-2">Selected Dates ({selectedDates.length}):</h6>
            <div className="d-flex flex-wrap gap-2">
              {selectedDates.map((date, idx) => (
                <Badge
                  key={idx}
                  bg="primary"
                  className="d-flex align-items-center"
                  style={{ cursor: "pointer" }}
                  onClick={() => handleDateClick(date)}
                >
                  {format(date, "MMM d, yyyy")}
                  <span className="ms-2">&times;</span>
                </Badge>
              ))}
            </div>
            <Button
              variant="outline-danger"
              size="sm"
              className="mt-2"
              onClick={() => setSelectedDates([])}
            >
              Clear All
            </Button>
          </div>
        )}
      </Card.Body>
    </Card>
  );
};
export default {
  DateListSelector
};
