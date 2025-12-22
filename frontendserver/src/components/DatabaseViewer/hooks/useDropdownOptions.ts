import { useState } from 'react';
import { DatabaseService } from '../../../services/database.service.ts';
import { User, Building, Desk } from '../../../types/database.types.tsx';

export const useDropdownOptions = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [buildings, setBuildings] = useState<Building[]>([]);
  const [desks, setDesks] = useState<Desk[]>([]);

  const fetchDropdownOptions = async () => {
    try {
      const [fetchedUsers, fetchedBuildings, fetchedDesks] = await Promise.all([
        DatabaseService.getUsers(),
        DatabaseService.getBuildings(),
        DatabaseService.getDesks()
      ]);
      setUsers(fetchedUsers);
      setBuildings(fetchedBuildings);
      setDesks(fetchedDesks);
      return { users: fetchedUsers, buildings: fetchedBuildings, desks: fetchedDesks };
    } catch (err) {
      console.error('Failed to fetch dropdown options:', err);
      return { users: [], buildings: [], desks: [] };
    }
  };

  return {
    users,
    buildings,
    desks,
    fetchDropdownOptions
  };
};
