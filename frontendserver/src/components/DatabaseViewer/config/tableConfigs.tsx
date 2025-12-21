import React, { JSX } from 'react';
import { Button } from 'react-bootstrap';
import { DatabaseService } from '../../../services/database.service.ts';
import { TableName, User, Building, Desk, Reservation, UserRole, DeskStatus } from '../../../types/database.types.tsx';
import { formatUserRole, formatDeskStatus, formatDate, formatNullableText } from '../utils/formatters.tsx';

export interface ColumnConfig {
  key: string;
  label: string;
  render?: (value: any, record: any) => JSX.Element | string | null;
}

export interface DetailViewConfig {
  title: (record: any) => string;
  fetchData: (record: any) => Promise<any[]>;
  columns: ColumnConfig[];
}

export interface FormField {
  key: string;
  label: string;
  type: 'text' | 'email' | 'number' | 'select' | 'date' | 'time' | 'textarea';
  required?: boolean;
  options?: { value: any; label: string }[];
  disabled?: boolean;
}

export interface TableConfig {
  name: TableName;
  displayName: string;
  singularName: string;
  columns: ColumnConfig[];
  fetchData: () => Promise<any[]>;
  getRecordId: (record: any) => string | number;
  detailView?: DetailViewConfig;
  formFields?: FormField[];
}

export const createTableConfigs = (
  handleViewDetails: (record: any, config: DetailViewConfig, detailTableType: TableName) => void
): TableConfig[] => [
  {
    name: 'users' as TableName,
    displayName: 'Users',
    singularName: 'User',
    fetchData: DatabaseService.getUsers,
    getRecordId: (user: User) => user.id,
    formFields: [
      { key: 'id', label: 'ID', type: 'text', disabled: true },
      { key: 'name', label: 'Name', type: 'text', required: true },
      { key: 'surname', label: 'Surname', type: 'text', required: true },
      { key: 'email', label: 'Email', type: 'email', required: true },
      { key: 'password', label: 'Password', type: 'text', required: true },
      {
        key: 'role',
        label: 'Role',
        type: 'select',
        required: true,
        options: [
          { value: UserRole.User, label: 'User' },
          { value: UserRole.Admin, label: 'Admin' }
        ]
      }
    ],
    detailView: {
      title: (user: User) => `Reservations for ${user.name} ${user.surname}`,
      fetchData: async (user: User) => {
        const allReservations = await DatabaseService.getReservations();
        return allReservations.filter(r => r.userId === user.id);
      },
      columns: [
        { key: 'id', label: 'ID' },
        { key: 'deskId', label: 'Desk ID' },
        {
          key: 'reservationDate',
          label: 'Date',
          render: (value: string) => formatDate(value)
        },
        { key: 'startDate', label: 'Start Time' },
        { key: 'endDate', label: 'End Time' }
      ]
    },
    columns: [
      { key: 'id', label: 'ID' },
      { key: 'name', label: 'Name' },
      { key: 'surname', label: 'Surname' },
      { key: 'email', label: 'Email' },
      {
        key: 'role',
        label: 'Role',
        render: (role: UserRole) => formatUserRole(role)
      },
      {
        key: 'reservations',
        label: 'Reservations',
        render: (_value: any, user: User) => (
          <Button
            variant="link"
            size="sm"
            onClick={() => {
              const config: DetailViewConfig = {
                title: (user: User) => `Reservations for ${user.name} ${user.surname}`,
                fetchData: async (user: User) => {
                  const allReservations = await DatabaseService.getReservations();
                  return allReservations.filter(r => r.userId === user.id);
                },
                columns: [
                  { key: 'id', label: 'ID' },
                  { key: 'deskId', label: 'Desk ID' },
                  {
                    key: 'reservationDate',
                    label: 'Date',
                    render: (value: string) => formatDate(value)
                  },
                ]
              };
              handleViewDetails(user, config, 'reservations' as TableName);
            }}
            className="p-0"
          >
            View Reservations
          </Button>
        )
      }
    ]
  },
  {
    name: 'buildings' as TableName,
    displayName: 'Buildings',
    singularName: 'Building',
    fetchData: DatabaseService.getBuildings,
    getRecordId: (building: Building) => building.id,
    formFields: [
      { key: 'id', label: 'ID', type: 'number', disabled: true },
      { key: 'name', label: 'Building Name', type: 'text', required: true }
    ],
    detailView: {
      title: (building: Building) => `Desks in ${building.name}`,
      fetchData: async (building: Building) => {
        const allDesks = await DatabaseService.getDesks();
        return allDesks.filter(d => d.buildingId === building.id);
      },
      columns: [
        { key: 'id', label: 'ID' },
        {
          key: 'description',
          label: 'Description',
          render: (value: string | null) => formatNullableText(value)
        },
        {
          key: 'status',
          label: 'Status',
          render: (status: DeskStatus) => formatDeskStatus(status)
        }
      ]
    },
    columns: [
      { key: 'id', label: 'ID' },
      { key: 'name', label: 'Name' },
      {
        key: 'desks',
        label: 'Desks',
        render: (_value: any, building: Building) => (
          <Button
            variant="link"
            size="sm"
            onClick={() => {
              const config: DetailViewConfig = {
                title: (building: Building) => `Desks in ${building.name}`,
                fetchData: async (building: Building) => {
                  const allDesks = await DatabaseService.getDesks();
                  return allDesks.filter(d => d.buildingId === building.id);
                },
                columns: [
                  { key: 'id', label: 'ID' },
                  {
                    key: 'description',
                    label: 'Description',
                    render: (value: string | null) => formatNullableText(value)
                  },
                  {
                    key: 'status',
                    label: 'Status',
                    render: (status: DeskStatus) => formatDeskStatus(status)
                  }
                ]
              };
              handleViewDetails(building, config, 'desks' as TableName);
            }}
            className="p-0"
          >
            View Desks
          </Button>
        )
      }
    ]
  },
  {
    name: 'desks' as TableName,
    displayName: 'Desks',
    singularName: 'Desk',
    fetchData: DatabaseService.getDesks,
    getRecordId: (desk: Desk) => desk.id,
    formFields: [
      { key: 'id', label: 'ID', type: 'number', disabled: true },
      { key: 'description', label: 'Description', type: 'textarea', required: false },
      { key: 'buildingId', label: 'Building', type: 'number', required: true },
      {
        key: 'status',
        label: 'Status',
        type: 'select',
        required: true,
        options: [
          { value: DeskStatus.Available, label: 'Available' },
          { value: DeskStatus.Booked, label: 'Booked' },
          { value: DeskStatus.Unavailable, label: 'Unavailable' }
        ]
      }
    ],
    detailView: {
      title: (desk: Desk) => `Reservations for Desk ${desk.id}${desk.description ? ' - ' + desk.description : ''}`,
      fetchData: async (desk: Desk) => {
        const allReservations = await DatabaseService.getReservations();
        return allReservations.filter(r => r.deskId === desk.id);
      },
      columns: [
        { key: 'id', label: 'ID' },
        { key: 'userId', label: 'User ID' },
        {
          key: 'reservationDate',
          label: 'Date',
          render: (value: string) => formatDate(value)
        },
        { key: 'startDate', label: 'Start Time' },
        { key: 'endDate', label: 'End Time' }
      ]
    },
    columns: [
      { key: 'id', label: 'ID' },
      {
        key: 'description',
        label: 'Description',
        render: (value: string | null) => formatNullableText(value)
      },
      { key: 'buildingId', label: 'Building' },
      {
        key: 'reservations',
        label: 'Reservations',
        render: (_value: any, desk: Desk) => (
          <Button
            variant="link"
            size="sm"
            onClick={() => {
              const config: DetailViewConfig = {
                title: (desk: Desk) => `Reservations for Desk ${desk.id}${desk.description ? ' - ' + desk.description : ''}`,
                fetchData: async (desk: Desk) => {
                  const allReservations = await DatabaseService.getReservations();
                  return allReservations.filter(r => r.deskId === desk.id);
                },
                columns: [
                  { key: 'id', label: 'ID' },
                  { key: 'userId', label: 'User ID' },
                  {
                    key: 'reservationDate',
                    label: 'Date',
                    render: (value: string) => formatDate(value)
                  }
                ]
              };
              handleViewDetails(desk, config, 'reservations' as TableName);
            }}
            className="p-0"
          >
            View Reservations
          </Button>
        )
      }
    ]
  },
  {
    name: 'reservations' as TableName,
    displayName: 'Reservations',
    singularName: 'Reservation',
    fetchData: DatabaseService.getReservations,
    getRecordId: (reservation: Reservation) => reservation.id,
    formFields: [
      { key: 'id', label: 'ID', type: 'number', disabled: true },
      { key: 'userId', label: 'User', type: 'text', required: true },
      { key: 'deskId', label: 'Desk', type: 'number', required: true },
      { key: 'reservationDate', label: 'Reservation Date', type: 'date', required: true },
      { key: 'startDate', label: 'Start Time', type: 'time', required: true },
      { key: 'endDate', label: 'End Time', type: 'time', required: true }
    ],
    columns: [
      { key: 'id', label: 'ID' },
      { key: 'userId', label: 'User ID' },
      { key: 'deskId', label: 'Desk ID' },
      {
        key: 'reservationDate',
        label: 'Date',
        render: (value: string) => formatDate(value)
      }
    ]
  }
];
