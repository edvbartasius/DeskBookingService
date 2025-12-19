import React from 'react';
import { Button, Alert, Spinner } from 'react-bootstrap';
import { ColumnConfig, TableConfig } from '../config/tableConfigs.tsx';
import { DataTable } from './DataTable.tsx';
import { Pagination } from './Pagination.tsx';
import { useTableFilters } from '../hooks/useTableFilters.ts';

interface DetailViewProps {
  title: string;
  data: any[];
  columns: ColumnConfig[];
  loading: boolean;
  currentConfig: TableConfig;
  onClose: () => void;
  onAdd: () => void;
  onEdit: (record: any) => void;
  onDelete: (record: any) => void;
}

export const DetailView: React.FC<DetailViewProps> = ({
  title,
  data,
  columns,
  loading,
  currentConfig,
  onClose,
  onAdd,
  onEdit,
  onDelete
}) => {
  // Create a minimal config for the detail view table
  const detailConfig: TableConfig = {
    name: currentConfig.name,
    displayName: currentConfig.displayName,
    singularName: currentConfig.singularName,
    columns: columns,
    fetchData: async () => data,
    getRecordId: (record: any) => record.id || record,
    formFields: currentConfig.formFields
  };

  // Use table filters for detail view
  const filters = useTableFilters(data, columns);

  return (
    <div className="detail-view">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h5 className="mb-0">{title}</h5>
        <div className="d-flex gap-2">
          <Button variant="success" size="sm" onClick={onAdd}>
            + Add New
          </Button>
          <Button variant="outline-secondary" size="sm" onClick={onClose}>
            ← Back to {currentConfig.displayName}
          </Button>
        </div>
      </div>

      {loading ? (
        <div className="text-center py-5">
          <Spinner animation="border" role="status">
            <span className="visually-hidden">Loading...</span>
          </Spinner>
        </div>
      ) : data.length === 0 ? (
        <Alert variant="info">No data found.</Alert>
      ) : (
        <>
          <DataTable
            config={detailConfig}
            data={filters.paginatedData}
            onEdit={onEdit}
            onDelete={onDelete}
            filters={filters.filters}
            sortColumn={filters.sortColumn}
            sortDirection={filters.sortDirection}
            onSort={filters.handleSort}
            onFilterChange={filters.handleFilterChange}
          />
          <Pagination
            currentPage={filters.currentPage}
            totalPages={filters.totalPages}
            totalRecords={filters.totalRecords}
            originalRecordCount={filters.originalRecordCount}
            itemsPerPage={filters.itemsPerPage}
            onPageChange={filters.setCurrentPage}
            onItemsPerPageChange={filters.setItemsPerPage}
          />
        </>
      )}
    </div>
  );
};
