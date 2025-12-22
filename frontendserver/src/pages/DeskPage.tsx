import { useState, useEffect } from "react";
import {
  FloorPlanDto,
  DeskDto,
  FloorPlanCanvas
} from "../components/FloorPlan/index.tsx";
import { Container, Row, Col, Card, ButtonGroup, Button, Modal } from "react-bootstrap";
import { DateSelector, useDeskAvailability, useOfficeClosedDates } from "../components/DateSelector/index.tsx";
import { startOfDay, addDays } from "date-fns";
import { Building } from "../types/booking.types.tsx";
import api from "../services/api.ts";
import { useUser } from "../contexts/UserContext.tsx";
import { BuildingSelector } from "../components/BuildingSelector/index.tsx";
import { DeskListView } from "../components/DeskListView/index.tsx";
import { ReservationConfirmModal } from "../components/ReservationConfirmModal/index.tsx";
import ConfirmationModal from "../components/ConfirmationModal.tsx";
import LoginModal from "../components/LoginModal.tsx";
import RegisterModal from "../components/RegisterModal.tsx";
import "../styles/DeskPage.css";

// Helper function to format Date to YYYY-MM-DD in local timezone
const formatDateLocal = (date: Date): string => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
};

type ViewMode = 'list' | 'floorplan';

const DeskPage = () => {
  const { loggedInUser } = useUser();
  const [buildings, setBuildings] = useState<Building[]>([]);
  const [selectedBuilding, setSelectedBuilding] = useState<Building | null>(null);
  const [floorPlan, setFloorPlan] = useState<FloorPlanDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadingDesks, setLoadingDesks] = useState(false);
  const [viewMode, setViewMode] = useState<ViewMode>('list');

  // Login/Register modal state
  const [showAuthPromptModal, setShowAuthPromptModal] = useState(false);
  const [showLoginModal, setShowLoginModal] = useState(false);
  const [showRegisterModal, setShowRegisterModal] = useState(false);

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

  // Cancellation confirmation modal state
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [pendingCancellation, setPendingCancellation] = useState<{
    desk: DeskDto;
    cancelType: 'single' | 'range';
  } | null>(null);

  // Fetch office closed dates
  const { closedDates, loading: loadingClosedDates } = useOfficeClosedDates(selectedBuilding?.id);
  const { bookedDates, loading: loadingBookedDates } = useDeskAvailability(deskToReserve?.id);

  // Fetch buildings on mount
  useEffect(() => {
    const fetchBuildings = async () => {
      try {
        const response = await api.get('buildings/get-buildings');
        if (response.status === 200) {
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
        if (response.status === 200) {
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
    // Check if user is logged in
    if (!loggedInUser) {
      setShowAuthPromptModal(true);
      return;
    }

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
      const url = `reservations/add`;
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

  const handleCancelReservation = (desk: DeskDto, cancelType: 'single' | 'range') => {
    setPendingCancellation({ desk, cancelType });
    setShowCancelModal(true);
  };

  const handleConfirmCancellation = async () => {
    if (!pendingCancellation) return;

    const { desk, cancelType } = pendingCancellation;

    try {
      let response;
      if (cancelType === 'single') {
        // Cancel single day
        const dateStr = selectedDate ? formatDateLocal(selectedDate) : '';
        console.log("Cancel single day:", desk.id, dateStr, loggedInUser?.id);
        const url = `/reservations/my-reservations/cancel-single-day/${desk.id}/${dateStr}/${loggedInUser?.id}`;
        response = await api.patch(url);
      } else {
        const dateStr = selectedDate ? formatDateLocal(selectedDate) : '';
        const url = `/reservations/my-reservations/cancel-booking-group-by-desk/${desk.id}/${dateStr}/${loggedInUser?.id}`;
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
    } finally {
      setPendingCancellation(null);
    }
  };

  const handleBuildingChange = (building: Building | null) => {
    setSelectedBuilding(building);
    setDeskToReserve(null);
  };

  // Calculate desk statistics
  const getDeskStats = () => {
    if (!floorPlan || !floorPlan.floorPlanDesks) {
      return { total: 0, available: 0, myReservations: 0 };
    }

    const total = floorPlan.floorPlanDesks.length;
    const available = floorPlan.floorPlanDesks.filter(desk => desk.status === 0).length; // DeskStatus.Available
    const myReservations = floorPlan.floorPlanDesks.filter(desk => desk.isReservedByCaller).length;

    return { total, available, myReservations };
  };

  // Find next available (non-closed) date
  const findNextAvailableDate = (fromDate: Date, direction: 'next' | 'prev' = 'next'): Date => {
    let currentDate = new Date(fromDate);
    const maxIterations = 365; // Prevent infinite loop
    let iterations = 0;

    while (iterations < maxIterations) {
      currentDate = direction === 'next'
        ? addDays(currentDate, 1)
        : addDays(currentDate, -1);

      const isClosed = closedDates.some(closedDate =>
        formatDateLocal(closedDate) === formatDateLocal(currentDate)
      );

      if (!isClosed) {
        return currentDate;
      }

      iterations++;
    }

    return fromDate; // Fallback to original date if no available date found
  };

  const deskStats = getDeskStats();
  return (
    <Container className="py-2">
      <h1 className="text-start mb-4 pt-2 fw-bold">Desk Booking</h1>
      <Card className="desk-page-card">
        <Card.Body className="p-4">
          <Row className="mb-4">
            <Col>
                <Row className="g-4 align-items-end">
                    {/* Left: Building Selector */}
                    <Col md={6}>
                      <BuildingSelector
                        buildings={buildings}
                        selectedBuilding={selectedBuilding}
                        onBuildingChange={handleBuildingChange}
                        loading={loading}
                      />
                    </Col>

                    {/* Right: Date Selector (only show when building is selected) */}
                    {selectedBuilding && (
                      <Col md={6}>
                        <DateSelector
                          selectedDate={selectedDate}
                          onSelectedDateChange={setSelectedDate}
                          closedDates={closedDates}
                          bookedDates={[]}
                          disabled={loading || loadingDesks || loadingClosedDates}
                        />
                      </Col>
                    )}
                  </Row>
            </Col>
          </Row>

          {/* Desk Stats */}
          {selectedBuilding && selectedDate && (
            <Row className="mb-3 align-items-center">
              <Col md={6} className="text-start">
                {!loadingDesks && (
                  <div className="desk-stats-summary">
                    <span className="badge bg-success me-2">
                      <i className="bi bi-check-circle me-1"></i>
                      {deskStats.available} Available
                    </span>
                    <span className="badge bg-primary me-2">
                      <i className="bi bi-person-check me-1"></i>
                      {deskStats.myReservations} Mine
                    </span>
                    <span className="badge bg-secondary">
                      <i className="bi bi-grid-3x3 me-1"></i>
                      {deskStats.total} Total
                    </span>
                  </div>
                )}
              </Col>
              <Col md={6} className="text-start">
                <Row className="mb-3">
                <Col className="d-flex justify-content-end">
                  <ButtonGroup className="desk-page-view-toggle">
                    <Button
                      variant={viewMode === 'list' ? 'dark' : 'outline-dark'}
                      onClick={() => setViewMode('list')}
                      title="List View"
                    >
                      <i className="bi bi-list-ul me-2"></i>
                      List
                    </Button>
                    <Button
                      variant={viewMode === 'floorplan' ? 'dark' : 'outline-dark'}
                      onClick={() => setViewMode('floorplan')}
                      title="Floor Plan View"
                    >
                      <i className="bi bi-map me-2"></i>
                      Floor Plan
                    </Button>
                  </ButtonGroup>
                </Col>
              </Row>
              </Col>
            </Row>
          )}

          {/* Main Content - View Toggle and Desk Selection */}
          {selectedBuilding && selectedDate && (
            <>
              {/* Desk Selection Section - No Card Borders */}
              <Row>
                <Col lg={12}>
                  {viewMode === 'floorplan' ? (
                    <div style={{ height: '70vh' }}>
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
                            <div className="text-center">
                              <div className="spinner-border text-primary mb-3" role="status">
                                <span className="visually-hidden">Loading floor plan...</span>
                              </div>
                              <p className="text-muted">Loading floor plan...</p>
                            </div>
                          ) : (
                            <div className="text-center">
                              <i className="bi bi-calendar-check"></i>
                              <p className="text-muted">Select a date to view the floor plan</p>
                            </div>
                          )}
                        </div>
                      )}
                    </div>
                  ) : (
                    <div style={{ height: '70vh', overflowY: 'auto' }}>
                      <DeskListView
                        floorPlan={floorPlan}
                        onReserveClick={handleReserveClick}
                        onCancelClick={handleCancelReservation}
                        loading={loadingDesks}
                      />
                    </div>
                  )}
                </Col>
              </Row>
            </>
          )}
        </Card.Body>
      </Card>

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

      {/* Cancellation Confirmation Modal */}
      <ConfirmationModal
        show={showCancelModal}
        onHide={() => setShowCancelModal(false)}
        onConfirm={handleConfirmCancellation}
        title="Cancel Reservation"
        message={
          pendingCancellation?.cancelType === 'single'
            ? `Are you sure you want to cancel your reservation for ${pendingCancellation.desk.description || `Desk ${pendingCancellation.desk.id}`} on ${selectedDate?.toLocaleDateString()}?`
            : `Are you sure you want to cancel ALL dates for your reservation at ${pendingCancellation?.desk.description || `Desk ${pendingCancellation?.desk.id}`}?`
        }
        confirmText="Yes, Cancel"
        cancelText="No, Keep It"
        variant="danger"
      />

      {/* Authentication Prompt Modal */}
      <Modal show={showAuthPromptModal} onHide={() => setShowAuthPromptModal(false)} centered>
        <Modal.Header closeButton>
          <Modal.Title>Login Required</Modal.Title>
        </Modal.Header>
        <Modal.Body className="text-center py-4">
          <i className="bi bi-person-lock" style={{ fontSize: '3rem', color: '#0d6efd' }}></i>
          <p className="mt-3 mb-4">
            You need to be logged in to make a reservation. Please login or register to continue.
          </p>
          <div className="d-flex gap-2 justify-content-center">
            <Button
              variant="primary"
              onClick={() => {
                setShowAuthPromptModal(false);
                setShowLoginModal(true);
              }}
            >
              Login
            </Button>
            <Button
              variant="outline-primary"
              onClick={() => {
                setShowAuthPromptModal(false);
                setShowRegisterModal(true);
              }}
            >
              Register
            </Button>
          </div>
        </Modal.Body>
      </Modal>

      {/* Login Modal */}
      <LoginModal
        show={showLoginModal}
        onHide={() => setShowLoginModal(false)}
        onSwitchToRegister={() => {
          setShowLoginModal(false);
          setShowRegisterModal(true);
        }}
      />

      {/* Register Modal */}
      <RegisterModal
        show={showRegisterModal}
        onHide={() => setShowRegisterModal(false)}
        onSwitchToLogin={() => {
          setShowRegisterModal(false);
          setShowLoginModal(true);
        }}
      />
    </Container>
  );
};

export default DeskPage;
