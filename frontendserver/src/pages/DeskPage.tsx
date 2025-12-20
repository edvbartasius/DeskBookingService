import { useState, useEffect } from "react";
import {
  FloorPlanCanvas,
  FloorPlanDto,
  DeskDto,
  DeskType,
  DeskStatus,
  getDeskTypeLabel,
  getDeskStatusLabel,
  getStatusBadgeVariant
} from "../components/FloorPlan/index.tsx";
import { Container, Row, Col, Card, Button, Badge, Alert } from "react-bootstrap";
import { DateListSelector, ReservationList } from "../components/Reservation/index.ts";
import { DailyReservation, DateRange, TimeSlot } from "../types/reservation";
import { v4 as uuidv4 } from 'uuid';

const DeskPage = () => {
  const [floorPlan, setFloorPlan] = useState<FloorPlanDto | null>(null);
  const [selectedDesk, setSelectedDesk] = useState<DeskDto | null>(null);
  const [loading, setLoading] = useState(true);

  // Multi-date reservation state
  const [dateRange, setDateRange] = useState<DateRange>({ start: null, end: null });
  const [reservations, setReservations] = useState<DailyReservation[]>([]);

  // Fetch floor plan from backend (replace with actual API call)
  useEffect(() => {
    // Simulate API call
    setTimeout(() => {
      const mockFloorPlan: FloorPlanDto = {
        buildingName: "Main Office - Floor 1",
        floorPlanWidth: 20, // 20 grid columns
        floorPlanHeight: 12, // 12 grid rows
        floorPlanDesks: [
          // Row 1 - Standard desks (grid row 1)
          {
            id: 1,
            description: "A1",
            buildingId: 1,
            positionX: 1, // Grid column 1
            positionY: 1, // Grid row 1
            type: DeskType.Standard,
            status: DeskStatus.Available,
            bookedTimeSpans: []
          },
          {
            id: 2,
            description: "A2",
            buildingId: 1,
            positionX: 3, // Grid column 3
            positionY: 1, // Grid row 1
            type: DeskType.Standard,
            status: DeskStatus.Booked,
            bookedTimeSpans: [
              {
                id: 1,
                startTime: "09:00:00",
                endTime: "12:00:00",
                status: 1,
              },
              {
                id: 2,
                startTime: "13:00:00",
                endTime: "17:00:00",
                status: 1,
              }
            ]
          },
          {
            id: 3,
            description: "A3",
            buildingId: 1,
            positionX: 5,
            positionY: 1,
            type: DeskType.Standard,
            status: DeskStatus.Available,
            bookedTimeSpans: []
          },
          {
            id: 4,
            description: "A4",
            buildingId: 1,
            positionX: 7,
            positionY: 1,
            type: DeskType.Standard,
            status: DeskStatus.Available,
            bookedTimeSpans: []
          },
          {
            id: 5,
            description: "A5",
            buildingId: 1,
            positionX: 9,
            positionY: 1,
            type: DeskType.Standard,
            status: DeskStatus.Booked,
            bookedTimeSpans: [
              {
                id: 3,
                startTime: "09:00:00",
                endTime: "17:00:00",
                status: 1,
              }
            ]
          },
          // Row 2 - More desks (grid row 3)
          {
            id: 6,
            description: "B1",
            buildingId: 1,
            positionX: 1,
            positionY: 3,
            type: DeskType.Standard,
            status: DeskStatus.Available,
            bookedTimeSpans: []
          },
          {
            id: 7,
            description: "B2",
            buildingId: 1,
            positionX: 3,
            positionY: 3,
            type: DeskType.Standard,
            status: DeskStatus.Available,
            bookedTimeSpans: []
          },
          {
            id: 8,
            description: "B3",
            buildingId: 1,
            positionX: 5,
            positionY: 3,
            type: DeskType.Standard,
            status: DeskStatus.Booked,
            bookedTimeSpans: [
              {
                id: 4,
                startTime: "10:00:00",
                endTime: "14:00:00",
                status: 1,
              }
            ]
          },
          {
            id: 9,
            description: "B4",
            buildingId: 1,
            positionX: 7,
            positionY: 3,
            type: DeskType.Standard,
            status: DeskStatus.Available,
            bookedTimeSpans: []
          },
          {
            id: 10,
            description: "B5",
            buildingId: 1,
            positionX: 9,
            positionY: 3,
            type: DeskType.Standard,
            status: DeskStatus.Unavailable,
            bookedTimeSpans: []
          },
          // Conference room
          {
            id: 11,
            description: "Conf A",
            buildingId: 1,
            positionX: 13,
            positionY: 1,
            type: DeskType.Conference,
            status: DeskStatus.Available,
            bookedTimeSpans: []
          },
          // Row 3 (grid row 6)
          {
            id: 12,
            description: "C1",
            buildingId: 1,
            positionX: 1,
            positionY: 6,
            type: DeskType.Standard,
            status: DeskStatus.Available,
            bookedTimeSpans: []
          },
          {
            id: 13,
            description: "C2",
            buildingId: 1,
            positionX: 3,
            positionY: 6,
            type: DeskType.Standard,
            status: DeskStatus.Available,
            bookedTimeSpans: []
          },
          {
            id: 14,
            description: "C3",
            buildingId: 1,
            positionX: 5,
            positionY: 6,
            type: DeskType.Standard,
            status: DeskStatus.Available,
            bookedTimeSpans: []
          },
          {
            id: 15,
            description: "C4",
            buildingId: 1,
            positionX: 7,
            positionY: 6,
            type: DeskType.Standard,
            status: DeskStatus.Booked,
            bookedTimeSpans: [
              {
                id: 5,
                startTime: "08:00:00",
                endTime: "12:00:00",
                status: 1,
              }
            ]
          },
          {
            id: 16,
            description: "C5",
            buildingId: 1,
            positionX: 9,
            positionY: 6,
            type: DeskType.Standard,
            status: DeskStatus.Available,
            bookedTimeSpans: []
          },
          // Row 4 (grid row 8)
          {
            id: 17,
            description: "D1",
            buildingId: 1,
            positionX: 1,
            positionY: 8,
            type: DeskType.Standard,
            status: DeskStatus.Available,
            bookedTimeSpans: []
          },
          {
            id: 18,
            description: "D2",
            buildingId: 1,
            positionX: 3,
            positionY: 8,
            type: DeskType.Standard,
            status: DeskStatus.Available,
            bookedTimeSpans: []
          },
          {
            id: 19,
            description: "D3",
            buildingId: 1,
            positionX: 5,
            positionY: 8,
            type: DeskType.Standard,
            status: DeskStatus.Available,
            bookedTimeSpans: []
          },
          {
            id: 20,
            description: "D4",
            buildingId: 1,
            positionX: 7,
            positionY: 8,
            type: DeskType.Standard,
            status: DeskStatus.Available,
            bookedTimeSpans: []
          },
          // Conference room 2
          {
            id: 21,
            description: "Conf B",
            buildingId: 1,
            positionX: 13,
            positionY: 6,
            type: DeskType.Conference,
            status: DeskStatus.Booked,
            bookedTimeSpans: [
              {
                id: 6,
                startTime: "14:00:00",
                endTime: "16:00:00",
                status: 1,
              }
            ]
          }
        ]
      };
      setFloorPlan(mockFloorPlan);
      setLoading(false);
    }, 500);

    // TODO: Replace with actual API call

  }, []);

  const handleDeskClick = (desk: DeskDto) => {
    setSelectedDesk(desk);
  };

  // Generate dates from range
  const handleGenerateDates = () => {
    if (!dateRange.start || !dateRange.end) return;

    const dates: DailyReservation[] = [];
    const currentDate = new Date(dateRange.start);
    const endDate = new Date(dateRange.end);

    while (currentDate <= endDate) {
      dates.push({
        date: new Date(currentDate),
        timeSlots: [
          {
            id: uuidv4(),
            startTime: "09:00:00",
            endTime: "17:00:00"
          }
        ]
      });
      currentDate.setDate(currentDate.getDate() + 1);
    }

    setReservations(dates);
  };

  // Update a specific reservation
  const handleUpdateReservation = (index: number, updated: DailyReservation) => {
    const newReservations = [...reservations];
    newReservations[index] = updated;
    setReservations(newReservations);
  };

  // Remove a specific reservation
  const handleRemoveReservation = (index: number) => {
    setReservations(reservations.filter((_, i) => i !== index));
  };

  // Copy time slots from one day to all others
  const handleCopyToAll = (sourceIndex: number) => {
    const sourceSlots = reservations[sourceIndex].timeSlots;
    const newReservations = reservations.map((reservation, index) => {
      if (index === sourceIndex) return reservation;

      return {
        ...reservation,
        timeSlots: sourceSlots.map(slot => ({
          ...slot,
          id: uuidv4() // Generate new IDs for copied slots
        }))
      };
    });
    setReservations(newReservations);
  };

  // Clear all reservations
  const handleClearAll = () => {
    setReservations([]);
    setDateRange({ start: null, end: null });
  };

  // Book desk with multiple reservations
  const handleBookDesk = () => {
    if (!selectedDesk) {
      alert("Please select a desk first");
      return;
    }

    if (reservations.length === 0) {
      alert("Please add at least one reservation date");
      return;
    }

    // Validate all reservations
    const hasErrors = reservations.some(reservation => {
      return reservation.timeSlots.some(
        slot => !slot.startTime || !slot.endTime || slot.startTime >= slot.endTime
      );
    });

    if (hasErrors) {
      alert("Please fix all time slot errors before booking");
      return;
    }

    // Format booking request
    const bookingRequest = {
      deskId: selectedDesk.id,
      reservations: reservations.map(reservation => ({
        date: reservation.date.toISOString().split('T')[0],
        timeSlots: reservation.timeSlots.map(slot => ({
          startTime: slot.startTime,
          endTime: slot.endTime
        }))
      }))
    };

    console.log("Booking request:", bookingRequest);
    alert(`Booking desk ${selectedDesk.description} for ${reservations.length} day(s).\nCheck console for details.`);

    // TODO: Implement actual API call
  };

  return (
    <Container fluid className="py-4">
      {/* Header */}
      <Row className="mb-4">
        <Col>
          <h1>Desk Booking</h1>
        </Col>
      </Row>

      {/* Date Range Selector */}
      <Row className="mb-4">
        <Col>
          <DateListSelector />
        </Col>
      </Row>
      {/* <Row className="mb-4">
        <Col>
          <DateListSelector
            dateRange={dateRange}
            onDateRangeChange={setDateRange}
            onGenerateDates={handleGenerateDates}
            disabled={loading}
          />
        </Col>
      </Row> */}

      {/* Main Content */}
      <Row>
        {/* Floor Plan Canvas */}
        <Col lg={7} className="mb-3">
          <Card style={{ height: '70vh' }}>
            <Card.Header>
              <h5 className="mb-0">Select a Desk</h5>
              {selectedDesk && (
                <div className="mt-2">
                  <Badge bg="primary" className="me-2">
                    Selected: {selectedDesk.description}
                  </Badge>
                  <Badge bg={getStatusBadgeVariant(selectedDesk.status)}>
                    {getDeskTypeLabel(selectedDesk.type)}
                  </Badge>
                </div>
              )}
            </Card.Header>
            <Card.Body className="p-0">
              {loading ? (
                <div className="d-flex justify-content-center align-items-center h-100">
                  <div className="spinner-border text-primary" role="status">
                    <span className="visually-hidden">Loading floor plan...</span>
                  </div>
                </div>
              ) : floorPlan ? (
                <FloorPlanCanvas
                  floorPlan={floorPlan}
                  selectedDate={new Date()}
                  onDeskClick={handleDeskClick}
                  selectedDeskId={selectedDesk?.id}
                />
              ) : (
                <div className="d-flex justify-content-center align-items-center h-100">
                  <Alert variant="danger">Failed to load floor plan</Alert>
                </div>
              )}
            </Card.Body>
          </Card>
        </Col>

        {/* Reservation List */}
        <Col lg={5} className="mb-3">
          <Card style={{ height: '70vh' }}>
            <Card.Header className="d-flex justify-content-between align-items-center">
              <h5 className="mb-0">Reservation Details</h5>
              {selectedDesk && reservations.length > 0 && (
                <Button
                  variant="success"
                  size="sm"
                  onClick={handleBookDesk}
                >
                  Book Now
                </Button>
              )}
            </Card.Header>
            <Card.Body style={{ overflowY: 'auto' }}>
              {!selectedDesk ? (
                <Alert variant="info">
                  Please select a desk from the floor plan to start booking.
                </Alert>
              ) : (
                <ReservationList
                  reservations={reservations}
                  onUpdateReservation={handleUpdateReservation}
                  onRemoveReservation={handleRemoveReservation}
                  onCopyToAll={handleCopyToAll}
                  onClearAll={handleClearAll}
                />
              )}
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default DeskPage;
