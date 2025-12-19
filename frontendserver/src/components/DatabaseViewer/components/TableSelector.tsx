import React from 'react';
import { Button, ButtonGroup, Form } from 'react-bootstrap';
import { TableName } from '../../../types/database.types.tsx';
import { TableConfig } from '../config/tableConfigs.tsx';

interface TableSelectorProps {
  tableConfigs: TableConfig[];
  selectedTable: TableName | null;
  currentConfig: TableConfig | null;
  onSelectTable: (table: TableName) => void;
  onAdd: () => void;
}

export const TableSelector: React.FC<TableSelectorProps> = ({
  tableConfigs,
  selectedTable,
  currentConfig,
  onSelectTable,
  onAdd
}) => {
  return (
    <div className="mb-3">
      <div className="d-flex justify-content-between align-items-end mb-2">
        <Form.Label className="mb-0">Select Table:</Form.Label>
        {currentConfig && (
          <Button variant="success" size="sm" onClick={onAdd}>
            + Add New {currentConfig.singularName}
          </Button>
        )}
      </div>
      <ButtonGroup className="d-flex gap-2">
        {tableConfigs.map(config => (
          <Button
            key={config.name}
            variant={selectedTable === config.name ? 'primary' : 'outline-primary'}
            onClick={() => onSelectTable(config.name)}
          >
            {config.displayName}
          </Button>
        ))}
      </ButtonGroup>
    </div>
  );
};
