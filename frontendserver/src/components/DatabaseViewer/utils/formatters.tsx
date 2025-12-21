import React, { JSX } from 'react';
import { Badge } from 'react-bootstrap';
import { UserRole, DeskStatus } from '../../../types/database.types.tsx';
import { DeskType } from '../../../types/booking.types.tsx';

export const formatUserRole = (role: UserRole): JSX.Element => {
  return role === UserRole.Admin
    ? <Badge bg="danger">Admin</Badge>
    : <Badge bg="primary">User</Badge>;
};

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

export const formatDeskType = (type: DeskType): JSX.Element => {
  return type === DeskType.ConferenceRoom
    ? <Badge bg="info">Conference Room</Badge>
    : <Badge bg="secondary">Regular Desk</Badge>;
};

export const formatBoolean = (value: boolean): JSX.Element => {
  return value
    ? <Badge bg="warning">Yes</Badge>
    : <Badge bg="success">No</Badge>;
};

export const formatDate = (value: string): string => {
  return new Date(value).toLocaleDateString();
};

export const formatNullableText = (value: string | null): JSX.Element | string => {
  return value || <em className="text-muted">No description</em>;
};