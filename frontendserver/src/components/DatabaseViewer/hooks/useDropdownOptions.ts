import { useState } from 'react';
import { DatabaseService } from '../../../services/database.service.ts';
import { User, Building, Desk } from '../../../types/database.types.tsx';

export const useDropdownOptions = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [buildings, setBuildings] = useState<Building[]>([]);
  const [desks, setDesks] = useState<Desk[]>([]);

  const fetchDropdownOptions = async () => {
    // TODO: dropdown menus arent always populated when first selected & check the need to limit data in dropdown menus
    try {
      const [fetchedUsers, fetchedBuildings, fetchedDesks] = await Promise.all([
        DatabaseService.getUsers(),
        DatabaseService.getBuildings(),
        DatabaseService.getDesks()
      ]);
      setUsers(fetchedUsers);
      setBuildings(fetchedBuildings);
      setDesks(fetchedDesks);
    } catch (err) {
      console.error('Failed to fetch dropdown options:', err);
    }
  };

  return {
    users,
    buildings,
    desks,
    fetchDropdownOptions
  };
};
