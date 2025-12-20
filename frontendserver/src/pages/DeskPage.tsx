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
import { Container, Row, Col, Card, Button, Badge } from "react-bootstrap";

const DeskPage = () => {
  const [floorPlan, setFloorPlan] = useState<FloorPlanDto | null>(null);
  const [selectedDesk, setSelectedDesk] = useState<DeskDto | null>(null);
  const [selectedDate, setSelectedDate] = useState<Date>(new Date());
  const [loading, setLoading] = useState(true);

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

  const handleBookDesk = () => {
    if (selectedDesk) {
      alert(`Booking desk ${selectedDesk.description} (ID: ${selectedDesk.id})`);
      // TODO: Implement actual booking logic
    }
  };

  return (
    <Container fluid className="py-4">
      {/* Header */}
      <Row className="mb-4">
        <Col>
          <div className="d-flex justify-content-between align-items-center">
            <h1>Desk Booking</h1>
            <input
              type="date"
              value={selectedDate.toISOString().split('T')[0]}
              onChange={(e) => setSelectedDate(new Date(e.target.value))}
              className="form-control"
              style={{ width: 'auto' }}
            />
          </div>
        </Col>
      </Row>

      {/* Main Content */}
      <Row style={{ height: 'calc(100vh - 200px)' }}>
        {/* Floor Plan Canvas */}
        <Col lg={8} className="mb-3">
          <Card className="h-100">
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
                  selectedDate={selectedDate}
                  onDeskClick={handleDeskClick}
                  selectedDeskId={selectedDesk?.id}
                />
              ) : (
                <div className="d-flex justify-content-center align-items-center h-100">
                  <div className="alert alert-danger">Failed to load floor plan</div>
                </div>
              )}
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default DeskPage;
