import React, { JSX } from 'react';
import { Badge, OverlayTrigger, Tooltip } from 'react-bootstrap';
import { UserRole, DeskStatus, ReservationStatus } from '../../../types/database.types.tsx';
import { DeskType } from '../../../types/booking.types.tsx';

export const formatUserRole = (role: UserRole): JSX.Element => {
  return role === UserRole.Admin
    ? <Badge bg="danger">Admin</Badge>
    : <Badge bg="primary">User</Badge>;
};

export const formatUserRoleString = (role: UserRole): string => {
  return role === UserRole.Admin ? 'Admin' : 'User';
}

export const formatDeskStatus = (status: DeskStatus): JSX.Element => {
  switch (status) {
    case DeskStatus.Available:
      return <Badge bg="success">Available</Badge>;
    case DeskStatus.Booked:
      return <Badge bg="warning">Booked</Badge>;
    case DeskStatus.Unavailable:
      return <Badge bg="secondary">Unavailable</Badge>;
    default:
      return <Badge bg="secondary">Unknown</Badge>;
  }
};

export const formatDeskStatusString = (status: DeskStatus): string => {
  switch (status) {
    case DeskStatus.Available:
      return 'Available';
    case DeskStatus.Booked:
      return 'Booked';
    case DeskStatus.Unavailable:
      return 'Unavailable';
    default:
      return 'Unknown';
  }
};

export const formatDeskType = (type: DeskType): JSX.Element => {
  return type === DeskType.ConferenceRoom
    ? <Badge bg="info">Conference Room</Badge>
    : <Badge bg="secondary">Regular Desk</Badge>;
};

export const formatDeskTypeString = (type: DeskType): string => {
  return type === DeskType.ConferenceRoom ? 'Conference Room' : 'Regular Desk';
}

export const formatBoolean = (value: boolean): JSX.Element => {
  return value
    ? <Badge bg="warning">Yes</Badge>
    : <Badge bg="success">No</Badge>;
};

export const formatMaintenaceString = (value: boolean): string => {
  return value ? 'Yes' : 'No';
}

export const formatDate = (value: string): string => {
  return new Date(value).toLocaleDateString();
};

export const formatNullableText = (value: string | null): JSX.Element | string => {
  return value || <em className="text-muted">No description</em>;
};

export const formatReservationStatus = (status: ReservationStatus): JSX.Element => {
  const commonClasses = "d-block text-center w-100 h-100";
  switch (status) {
    case ReservationStatus.Active:
      return <Badge bg="success" className={commonClasses}>Active</Badge>;
    case ReservationStatus.Completed:
      return <Badge bg="secondary" className={commonClasses}>Completed</Badge>;
    case ReservationStatus.Cancelled:
      return <Badge bg="danger" className={commonClasses}>Cancelled</Badge>;
    default:
      return <Badge bg="secondary" className={commonClasses}>Unknown</Badge>;
  }
};

export const formatReservationStatusString = (status: ReservationStatus): string => {
  switch (status) {
    case ReservationStatus.Active:
      return 'Active';
    case ReservationStatus.Completed:
      return 'Completed';
    case ReservationStatus.Cancelled:
      return 'Cancelled';
    default:
      return 'Unknown';
  }
};

export const formatGuid = (guid: string | null | undefined): JSX.Element => {
  if (!guid) {
    return <em className="text-muted">None</em>;
  }

  // Shorten GUID for display (first 8 chars)
  const shortGuid = guid.substring(0, 8);

  return (
    <OverlayTrigger
      placement="top"
      overlay={<Tooltip>{guid}</Tooltip>}
    >
      <code style={{ cursor: 'help' }}>{shortGuid}...</code>
    </OverlayTrigger>
  );
};