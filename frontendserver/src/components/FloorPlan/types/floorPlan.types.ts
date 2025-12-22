// Backend DTO types matching C# models

export enum DeskType {
  Standard = 0,
  Conference = 1
}

export enum DeskStatus {
  Available = 0,
  Booked = 1,
  Unavailable = 2
}

export interface DeskDto {
  id: number;
  deskNumber: string;
  description?: string;
  buildingId: number;
  positionX: number; // Grid column
  positionY: number; // Grid row
  status: DeskStatus;
  type: DeskType;
  isReservedByCaller: boolean; // True if the reservation belongs to the current user
  reservedByFullName: string;
  maintenanceReason?: string;
}

export interface FloorPlanDto {
  buildingName?: string;
  floorPlanWidth: number; // Number of grid columns
  floorPlanHeight: number; // Number of grid rows
  floorPlanDesks?: DeskDto[];
}

// Component Props
export interface FloorPlanCanvasProps {
  floorPlan: FloorPlanDto;
  selectedDate?: Date;
  selectedTime?: string;
  onDeskClick?: (desk: DeskDto) => void;
  onCancelClick?: (desk: DeskDto, cancelType: 'single' | 'range') => void;
  selectedDeskId?: number;
  cellSize?: number;
}

export interface DeskTileProps {
  desk: DeskDto;
  onClick?: () => void;
  onHover?: (desk: DeskDto | null) => void;
  isSelected?: boolean;
  cellSize: number;
}

// Internal state types
export interface ViewBox {
  x: number;
  y: number;
  width: number;
  height: number;
}

export interface Point {
  x: number;
  y: number;
}

export interface ContainerSize {
  width: number;
  height: number;
}

export interface DeskColors {
  fill: string;
  stroke: string;
  hover: string;
}
