import React from 'react';
import { Form } from 'react-bootstrap';
import { Building } from '../../types/booking.types';

interface BuildingSelectorProps {
  buildings: Building[];
  selectedBuilding: Building | null;
  onBuildingChange: (building: Building | null) => void;
  loading?: boolean;
}

export const BuildingSelector: React.FC<BuildingSelectorProps> = ({
  buildings,
  selectedBuilding,
  onBuildingChange,
  loading = false
}) => {
  const handleChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const buildingId = parseInt(e.target.value);
    const building = buildings.find(b => b.id === buildingId);
    onBuildingChange(building || null);
  };

  return (
    <Form.Group>
          <Form.Label>Select Building</Form.Label>
          <Form.Select
            value={selectedBuilding?.id || ''}
            onChange={handleChange}
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
  );
};
