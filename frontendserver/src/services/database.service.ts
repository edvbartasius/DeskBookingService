import api from './api.ts';
import { User, Building, Desk, Reservation } from '../types/database.types';

/**
 * Service for fetching database table data
 */
export class DatabaseService {
  /**
   * Fetch all users from the database
   */
  static async getUsers(): Promise<User[]> {
    const response = await api.get('users/get-users');
    return response.data;
  }

  /**
   * Fetch all buildings from the database
   */
  static async getBuildings(): Promise<Building[]> {
    const response = await api.get<Building[]>('buildings/get-buildings');
    return response.data;
  }

  /**
   * Fetch all desks from the database
   */
  static async getDesks(): Promise<Desk[]> {
    const response = await api.get<Desk[]>('desks/get-desks');
    return response.data;
  }

  /**
   * Fetch all reservations from the database
   */
  static async getReservations(): Promise<Reservation[]> {
    const response = await api.get<Reservation[]>('reservations/');
    return response.data;
  }
}
