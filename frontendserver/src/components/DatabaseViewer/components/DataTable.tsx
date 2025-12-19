import React, { JSX } from 'react';
import { Table } from 'react-bootstrap';
import { TableConfig } from '../config/tableConfigs.tsx';
import { ActionButtons } from './ActionButtons.tsx';
import { SortableFilterableHeader } from './SortableFilterableHeader.tsx';
import { SortDirection } from '../hooks/useTableFilters.ts';

interface DataTableProps {
  config: TableConfig;
  data: any[];
  onEdit: (record: any) => void;
  onDelete: (record: any) => void;
  // Filter/Sort props
  filters?: { [key: string]: string };
  sortColumn?: string | null;
  sortDirection?: SortDirection;
  onSort?: (columnKey: string) => void;
  onFilterChange?: (columnKey: string, value: string) => void;
}

export const DataTable: React.FC<DataTableProps> = ({
  config,
  data,
  onEdit,
  onDelete,
  filters = {},
  sortColumn = null,
  sortDirection = null,
  onSort,
  onFilterChange
}) => {
  const renderTableHeaders = (): JSX.Element => {
    const useFiltering = onSort && onFilterChange;

    return (
      <tr>
        {config.columns.map(col => {
          // Don't allow filtering/sorting on columns with custom render (like action buttons)
          const canFilter = useFiltering && !col.render;

          return useFiltering ? (
            <SortableFilterableHeader
              key={col.key}
              columnKey={col.key}
              label={col.label}
              sortColumn={sortColumn}
              sortDirection={sortDirection}
              filterValue={filters[col.key] || ''}
              onSort={onSort}
              onFilterChange={onFilterChange}
              canFilter={canFilter}
            />
          ) : (
            <th key={col.key}>{col.label}</th>
          );
        })}
        <th className="p-2">Actions</th>
      </tr>
    );
  };

  const renderTableRows = (): JSX.Element[] => {
    if (data.length === 0) {
      return [
        <tr key="no-data">
          <td colSpan={config.columns.length + 1} className="text-center text-muted">
            No data available
          </td>
        </tr>
      ];
    }

    return data.map((record) => (
      <tr key={config.getRecordId(record)}>
        {config.columns.map(col => (
          <td key={col.key}>
            {col.render
              ? col.render(record[col.key], record)
              : record[col.key]
            }
          </td>
        ))}
        <td>
          <ActionButtons
            onEdit={() => onEdit(record)}
            onDelete={() => onDelete(record)}
          />
        </td>
      </tr>
    ));
  };

  return (
    <div className="table-responsive">
      <Table striped bordered hover>
        <thead className="table-dark">
          {renderTableHeaders()}
        </thead>
        <tbody>
          {renderTableRows()}
        </tbody>
      </Table>
    </div>
  );
};
