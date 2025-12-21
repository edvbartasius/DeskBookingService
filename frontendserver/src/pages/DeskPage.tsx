import { useState, useEffect } from "react";
import {
  FloorPlanDto,
  DeskDto,
  FloorPlanCanvas
} from "../components/FloorPlan/index.tsx";
import { Container, Row, Col, Card } from "react-bootstrap";
import { DateSelector, useDeskAvailability, useOfficeClosedDates } from "../components/DateSelector/index.tsx";
import { startOfDay, addDays } from "date-fns";
import { Building } from "../types/booking.types.tsx";
import api from "../services/api.ts";
import { useUser } from "../contexts/UserContext.tsx";
import { BuildingSelector } from "../components/BuildingSelector/index.tsx";
import { DeskListView } from "../components/DeskListView/index.tsx";
import { ReservationConfirmModal } from "../components/ReservationConfirmModal/index.tsx";

// Helper function to format Date to YYYY-MM-DD in local timezone
const formatDateLocal = (date: Date): string => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
};

const DeskPage = () => {
  const { loggedInUser } = useUser();
  const [buildings, setBuildings] = useState<Building[]>([]);
  const [selectedBuilding, setSelectedBuilding] = useState<Building | null>(null);
  const [floorPlan, setFloorPlan] = useState<FloorPlanDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadingDesks, setLoadingDesks] = useState(false);

  // Single-date reservation state
  const today = startOfDay(new Date());
  const tomorrow = addDays(today, 1);
  const [selectedDate, setSelectedDate] = useState<Date | null>(tomorrow);

  // Reservation modal state
  const [showReservationModal, setShowReservationModal] = useState(false);
  const [deskToReserve, setDeskToReserve] = useState<DeskDto | null>(null);
  const [isReservationSuccess, setIsReservationSuccess] = useState(false);
  const [isReservationLoading, setIsReservationLoading] = useState(false);
  const [reservationError, setReservationError] = useState<string | null>(null);

  // Fetch office closed dates
  const { closedDates, loading: loadingClosedDates } = useOfficeClosedDates(selectedBuilding?.id);
  const { bookedDates, loading: loadingBookedDates } = useDeskAvailability(deskToReserve?.id);

  // Fetch buildings on mount
  useEffect(() => {
    const fetchBuildings = async () => {
      try {
        const response = await api.get('buildings/get-buildings');
        if (response.status === 200){
          setBuildings(response.data);
        }
      } catch (error: any) {
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
      const buildingId = selectedBuilding.id;
      const dateStr = formatDateLocal(selectedDate);
      const userId = loggedInUser?.id;
      const url = `buildings/get-floor-plan/${buildingId}/${dateStr}/${userId}`;

      try {
        const response = await api.get(url);
        if (response.status === 200){
          setFloorPlan(response.data);
        }
      } catch (error: any) {
        console.error('Failed to fetch floor plan:', error);
      } finally {
        setLoadingDesks(false);
      }
    };

    fetchFloorPlan();
  }, [selectedBuilding, selectedDate, loggedInUser?.id]);

  const handleReserveClick = (desk: DeskDto) => {
    setDeskToReserve(desk);
    setIsReservationSuccess(false);
    setReservationError(null);
    setShowReservationModal(true);
  };

  const handleConfirmReservation = async (selectedDates: Date[]) => {
    if (!deskToReserve || selectedDates.length === 0) {
      return;
    }

    setIsReservationLoading(true);
    setReservationError(null);

    try {
      // Format booking request - use local date formatting to avoid timezone shifts
      const bookingRequest = {
        userId: loggedInUser?.id,
        deskId: deskToReserve.id,
        reservationDates: selectedDates.map(formatDateLocal)
      };

      console.log("Booking request:", bookingRequest);
      const url = `reservations/create`;
      const response = await api.post(url, bookingRequest);

      if (response.status === 200) {
        // Mark as success
        setIsReservationSuccess(true);

        // Refresh floor plan to update desk availability for current selected date
        if (selectedBuilding && selectedDate) {
          const buildingId = selectedBuilding.id;
          const dateStr = formatDateLocal(selectedDate);
          const userId = loggedInUser?.id;
          const url = `buildings/get-floor-plan/${buildingId}/${dateStr}/${userId}`;

          const response = await api.get(url);
          if (response.status === 200) {
            setFloorPlan(response.data);
          }
        }
      }
    } catch (error: any) {
      console.error('Failed to book desk:', error);
      // Extract error message from backend response
      const errorMessage = error.response?.data?.error || error.response?.data || error.message || 'Failed to reserve the desk. Please try again.';
      setReservationError(errorMessage);
    } finally {
      setIsReservationLoading(false);
    }
  };

  const handleCloseReservationModal = () => {
    setShowReservationModal(false);
    setDeskToReserve(null);
    setIsReservationSuccess(false);
    setReservationError(null);
  };

  const handleCancelReservation = async (desk: DeskDto, cancelType: 'single' | 'range') => {
    const confirmMessage = cancelType === 'single'
      ? `Are you sure you want to cancel your reservation for ${desk.description || `Desk ${desk.id}`} on ${selectedDate?.toLocaleDateString()}?`
      : `Are you sure you want to cancel ALL dates for your reservation at ${desk.description || `Desk ${desk.id}`}?`;

    if (!window.confirm(confirmMessage)) {
      return;
    }

    try {
      let response;
      if (cancelType === 'single') {
        // Cancel single day
        const dateStr = selectedDate ? formatDateLocal(selectedDate) : '';
        const url = `/reservations/my-reservations/cancel-single-day/${desk.id}/${dateStr}/${loggedInUser?.id}`;
        response = await api.patch(url);
      } else {
        const dateStr = selectedDate ? formatDateLocal(selectedDate) : '';
        const url = `/reservations/my-reservations/cancel-booking-group/${desk.id}/${dateStr}/${loggedInUser?.id}`;
        response = await api.patch(url);
      }

      // After successful cancellation, refresh the floor plan
      if (response.status === 200 && selectedBuilding && selectedDate) {
        const buildingId = selectedBuilding.id;
        const dateStr = formatDateLocal(selectedDate);
        const userId = loggedInUser?.id;
        const url = `buildings/get-floor-plan/${buildingId}/${dateStr}/${userId}`;
        const refreshResponse = await api.get(url);
        if (refreshResponse.status === 200) {
          setFloorPlan(refreshResponse.data);
        }
      }
    } catch (error: any) {
      console.error('Failed to cancel reservation:', error);
      // Extract error message from backend response
      const errorMessage = error.response?.data?.error || error.response?.data || error.message || 'Failed to cancel reservation. Please try again.';
      alert(`Error: ${errorMessage}`);
    }
  };

  const handleBuildingChange = (building: Building | null) => {
    setSelectedBuilding(building);
    setDeskToReserve(null);
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
          <BuildingSelector
            buildings={buildings}
            selectedBuilding={selectedBuilding}
            onBuildingChange={handleBuildingChange}
            loading={loading}
          />
        </Col>
      </Row>

      {/* Date Selector - Full Width */}
      {selectedBuilding && (
        <Row className="mb-3">
          <Col>
            <Card>
              <Card.Body>
                <DateSelector
                  selectedDate={selectedDate}
                  onSelectedDateChange={setSelectedDate}
                  closedDates={closedDates}
                  bookedDates={[]}
                  disabled={loading || loadingDesks || loadingClosedDates}
                />
              </Card.Body>
            </Card>
          </Col>
        </Row>
      )}

      {/* Main Content - Floor Plan and Desk List Side by Side */}
      {selectedBuilding && (
        <Row>
          {/* Floor Plan Canvas - Left Side */}
          <Col lg={7} className="mb-3">
            <Card style={{ height: '70vh' }}>
              <Card.Header>
                <h5 className="mb-0">Floor Plan</h5>
              </Card.Header>
              <Card.Body className="p-0" style={{ height: 'calc(70vh - 60px)' }}>
                {floorPlan ? (
                  <FloorPlanCanvas
                    floorPlan={floorPlan}
                    onDeskClick={handleReserveClick}
                    onCancelClick={handleCancelReservation}
                    selectedDeskId={deskToReserve?.id}
                  />
                ) : (
                  <div className="d-flex justify-content-center align-items-center h-100">
                    {loadingDesks ? (
                      <div className="spinner-border text-primary" role="status">
                        <span className="visually-hidden">Loading floor plan...</span>
                      </div>
                    ) : (
                      <p className="text-muted">Select a date to view the floor plan</p>
                    )}
                  </div>
                )}
              </Card.Body>
            </Card>
          </Col>

          {/* Desk List View - Right Side */}
          <Col lg={5} className="mb-3">
            <Card style={{ height: '70vh' }}>
              <Card.Header>
                <h5 className="mb-0">Available Desks</h5>
              </Card.Header>
              <Card.Body className="p-3" style={{ maxHeight: 'calc(70vh - 60px)', overflowY: 'auto' }}>
                <DeskListView
                  floorPlan={floorPlan}
                  onReserveClick={handleReserveClick}
                  onCancelClick={handleCancelReservation}
                  loading={loadingDesks}
                />
              </Card.Body>
            </Card>
          </Col>
        </Row>
      )}

      {/* Reservation Confirmation Modal */}
      <ReservationConfirmModal
        show={showReservationModal}
        onHide={handleCloseReservationModal}
        desk={deskToReserve}
        isSuccess={isReservationSuccess}
        isLoading={isReservationLoading}
        error={reservationError}
        onConfirm={handleConfirmReservation}
        bookedDates={bookedDates}
        closedDates={closedDates}
        initialDate={selectedDate}
      />
    </Container>
  );
};

export default DeskPage;
