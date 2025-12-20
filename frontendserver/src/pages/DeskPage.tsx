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
import { Container, Row, Col, Card, Button, Badge, Alert, Form } from "react-bootstrap";
import { DateSelector } from "../components/DateSelector.tsx";
import { startOfDay, addDays } from "date-fns";
import { Building } from "../types/booking.types.tsx";

const DeskPage = () => {
  const [buildings, setBuildings] = useState<Building[]>([]);
  const [selectedBuilding, setSelectedBuilding] = useState<Building | null>(null);
  const [floorPlan, setFloorPlan] = useState<FloorPlanDto | null>(null);
  const [selectedDesk, setSelectedDesk] = useState<DeskDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadingDesks, setLoadingDesks] = useState(false);

  // Single-date reservation state
  const today = startOfDay(new Date());
  const tomorrow = addDays(today, 1);
  const [selectedDate, setSelectedDate] = useState<Date | null>(tomorrow);
  const [selectedDateLabel, setSelectedDateLabel] = useState<String | null>();

  // Fetch buildings on mount
  useEffect(() => {
    const fetchBuildings = async () => {
      try {
        const response = await fetch('/api/buildings/get-buildings');
        if (response.ok) {
          const data = await response.json();
          setBuildings(data);
          // Auto-select first building if available
          if (data.length > 0) {
            setSelectedBuilding(data[0]);
          }
        }
      } catch (error) {
        console.error('Failed to fetch buildings:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchBuildings();
  }, []);

  // Fetch floor plan and desk availability when building or date changes
  useEffect(() => {
    if (!selectedBuilding || !selectedDate) {
      setFloorPlan(null);
      return;
    }

    const fetchFloorPlan = async () => {
      setLoadingDesks(true);
      try {
        // TODO: Replace with actual API call to get floor plan with availability for selected date
        // Example: /api/desks/floor-plan?buildingId=${selectedBuilding.id}
        // And: /api/desks/availability/by-date?buildingId=${selectedBuilding.id}&date=${dateString}
      } catch (error) {
        console.error('Failed to fetch floor plan:', error);
      } finally {
        setLoadingDesks(false);
      }
    };

    fetchFloorPlan();
  }, [selectedBuilding, selectedDate]);

  const handleDeskClick = (desk: DeskDto) => {
    setSelectedDesk(desk);
  };

  // Book desk for the selected date
  const handleBookDesk = () => {
    if (!selectedDesk) {
      alert("Please select a desk first");
      return;
    }

    if (!selectedDate) {
      alert("Please select a date");
      return;
    }

    // Format booking request
    const bookingRequest = {
      deskId: selectedDesk.id,
      reservationDates: [selectedDate.toISOString().split('T')[0]]
    };

    console.log("Booking request:", bookingRequest);
    alert(`Booking desk ${selectedDesk.description} for ${selectedDate.toDateString()}.\nCheck console for details.`);

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

      {/* Building Selector */}
      <Row className="mb-3">
        <Col>
          <Card>
            <Card.Body>
              <Form.Group>
                <Form.Label>Select Building</Form.Label>
                <Form.Select
                  value={selectedBuilding?.id || ''}
                  onChange={(e) => {
                    const buildingId = parseInt(e.target.value);
                    const building = buildings.find(b => b.id === buildingId);
                    setSelectedBuilding(building || null);
                    setSelectedDesk(null); // Clear selected desk when changing building
                  }}
                  disabled={loading || buildings.length === 0}
                >
                  {buildings.length === 0 ? (
                    <option value="">No buildings available</option>
                  ) : (
                    <>
                      <option value="">Select a building...</option>
                      {buildings.map(building => (
                        <option key={building.id} value={building.id}>
                          {building.name}
                        </option>
                      ))}
                    </>
                  )}
                </Form.Select>
              </Form.Group>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      {/* Main Content */}
      {selectedBuilding && (
        <Row>
          {/* Floor Plan Canvas */}
          <Col lg={7} className="mb-3">
            <Card style={{ height: '70vh' }}>
              <Card.Header>
                <div className="d-flex justify-content-between align-items-center mb-3">
                  <h5 className="mb-0">Select a Desk</h5>
                </div>

                <DateSelector
                  selectedDate={selectedDate}
                  onSelectedDateChange={setSelectedDate}
                  disabled={loading || loadingDesks}
                />

              {selectedDesk && (
                <div className="mt-3">
                  <Badge bg="primary" className="me-2">
                    Selected: {selectedDesk.description}
                  </Badge>
                  <Badge bg={getStatusBadgeVariant(selectedDesk.status)}>
                    {getDeskTypeLabel(selectedDesk.type)}
                  </Badge>
                </div>
              )}
            </Card.Header>
            <Card.Body className="p-3" style={{ maxHeight: '60vh', overflowY: 'auto' }}>
              {loadingDesks ? (
                <div className="d-flex justify-content-center align-items-center h-100">
                  <div className="spinner-border text-primary" role="status">
                    <span className="visually-hidden">Loading desks...</span>
                  </div>
                </div>
              ) : floorPlan && floorPlan.floorPlanDesks ? (
                // Temporarily disabled FloorPlanCanvas - showing list view instead
                // <FloorPlanCanvas
                //   floorPlan={floorPlan}
                //   selectedDate={new Date()}
                //   onDeskClick={handleDeskClick}
                //   selectedDeskId={selectedDesk?.id}
                // />
                <div>
                  <Row className="g-3">
                    {floorPlan.floorPlanDesks.map((desk) => (
                      <Col key={desk.id} md={6} lg={4}>
                        <Card
                          className={`h-100 ${selectedDesk?.id === desk.id ? 'border-primary border-2' : ''}`}
                          style={{ cursor: 'pointer' }}
                          onClick={() => handleDeskClick(desk)}
                        >
                          <Card.Body>
                            <div className="d-flex justify-content-between align-items-start mb-2">
                              <h5 className="mb-0">{desk.description}</h5>
                              <Badge bg={getStatusBadgeVariant(desk.status)}>
                                {getDeskStatusLabel(desk.status)}
                              </Badge>
                            </div>
                            <div className="text-muted small">
                              <div>Type: {getDeskTypeLabel(desk.type)}</div>
                              <div>Position: ({desk.positionX}, {desk.positionY})</div>
                            </div>
                          </Card.Body>
                        </Card>
                      </Col>
                    ))}
                  </Row>
                </div>
              ) : (
                <div className="d-flex justify-content-center align-items-center h-100">
                  <Alert variant="info">Select a date to view available desks</Alert>
                </div>
              )}
            </Card.Body>
          </Card>
        </Col>
      </Row>
      )}
    </Container>
  );
};

export default DeskPage;
