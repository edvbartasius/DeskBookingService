import React, { useMemo } from 'react';
import { Alert, Spinner } from 'react-bootstrap';
import { createTableConfigs } from './config/tableConfigs.tsx';
import { useTableData } from './hooks/useTableData.ts';
import { useDetailView } from './hooks/useDetailView.ts';
import { useCrudOperations } from './hooks/useCrudOperations.ts';
import { useDropdownOptions } from './hooks/useDropdownOptions.ts';
import { useTableFilters } from './hooks/useTableFilters.ts';
import { TableSelector } from './components/TableSelector.tsx';
import { DataTable } from './components/DataTable.tsx';
import { DetailView } from './components/DetailView.tsx';
import { CrudModal } from './components/CrudModal.tsx';
import { Pagination } from './components/Pagination.tsx';

// TODO:
// After checking details (like user reservations) and selecting different table, details still shown
const DatabaseViewer: React.FC = () => {
  // Custom hooks
  const {
    selectedTable,
    tableData,
    loading,
    error,
    setError,
    fetchTableData,
    selectTable
  } = useTableData();

  const {
    showDetailView,
    detailViewTitle,
    detailViewData,
    detailViewColumns,
    detailViewLoading,
    detailViewTableType,
    detailViewParentRecord,
    handleViewDetails,
    handleCloseDetailView
  } = useDetailView();

  const {
    showCrudModal,
    crudMode,
    crudFormData,
    crudModalTitle,
    crudFormFields,
    crudError,
    openAddModal,
    openEditModal,
    openDeleteModal,
    closeCrudModal,
    handleFormChange,
    handleCrudSubmit
  } = useCrudOperations();

  const {
    users,
    buildings,
    desks,
    fetchDropdownOptions
  } = useDropdownOptions();

  // Table configurations
  const tableConfigs = useMemo(() => createTableConfigs(handleViewDetails), [handleViewDetails]);

  // Get current table config
  const currentConfig = useMemo(
    () => selectedTable ? tableConfigs.find(config => config.name === selectedTable) || null : null,
    [tableConfigs, selectedTable]
  );

  // Filtering and pagination for main table
  const mainTableFilters = useTableFilters(tableData, currentConfig?.columns || []);

  // Get the active config (detail view if active, otherwise main table)
  const getActiveConfig = () => {
    if (showDetailView && detailViewTableType) {
      return tableConfigs.find(c => c.name === detailViewTableType) || currentConfig;
    }
    return currentConfig;
  };

  // CRUD handlers
  const handleAdd = async () => {
    await fetchDropdownOptions();
    const activeConfig = getActiveConfig();
    if (!activeConfig) return;

    // Create context data if in detail view
    let contextData = undefined;
    if (showDetailView && detailViewParentRecord && detailViewTableType) {
      // Map parent record to child field (e.g., userId for reservations)
      if (selectedTable === 'users' && detailViewTableType === 'reservations') {
        contextData = { userId: detailViewParentRecord.id };
      } else if (selectedTable === 'buildings' && detailViewTableType === 'desks') {
        contextData = { buildingId: detailViewParentRecord.id };
      } else if (selectedTable === 'desks' && detailViewTableType === 'reservations') {
        contextData = { deskId: detailViewParentRecord.id };
      }
    }

    openAddModal(
      activeConfig.singularName,
      activeConfig.formFields || [],
      users,
      buildings,
      desks,
      contextData
    );
  };

  const handleEdit = async (record: any) => {
    await fetchDropdownOptions();
    const activeConfig = getActiveConfig();
    if (!activeConfig) return;
    openEditModal(
      record,
      activeConfig.singularName,
      activeConfig.formFields || [],
      users,
      buildings,
      desks
    );
  };

  const handleDelete = (record: any) => {
    const activeConfig = getActiveConfig();
    if (!activeConfig) return;
    openDeleteModal(record, activeConfig.singularName);
  };

  const handleSubmit = async () => {
    if (!selectedTable) return;

    // Use detail view table type if in detail view, otherwise use main table
    const targetTable = showDetailView && detailViewTableType
      ? detailViewTableType
      : selectedTable;

    await handleCrudSubmit(targetTable, () => {
      // Refresh the main table
      fetchTableData(selectedTable, tableConfigs);

      // If in detail view, re-trigger the detail view to refresh its data
      if (showDetailView && detailViewParentRecord && detailViewTableType) {
        const activeConfig = tableConfigs.find(c => c.name === selectedTable);
        const detailConfig = activeConfig?.detailView;
        if (detailConfig) {
          handleViewDetails(detailViewParentRecord, detailConfig, detailViewTableType);
        }
      }
    });
  };

  return (
    <div className="database-viewer" style={{ position: 'relative' }}>
      <TableSelector
        tableConfigs={tableConfigs}
        selectedTable={selectedTable}
        currentConfig={currentConfig}
        onSelectTable={(table) => selectTable(table, tableConfigs)}
        onAdd={handleAdd}
      />

      {error && (
        <Alert variant="danger" dismissible onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {showDetailView ? (
        currentConfig && (
          <DetailView
            title={detailViewTitle}
            data={detailViewData}
            columns={detailViewColumns}
            loading={detailViewLoading}
            currentConfig={currentConfig}
            onClose={handleCloseDetailView}
            onAdd={handleAdd}
            onEdit={handleEdit}
            onDelete={handleDelete}
          />
        )
      ) : loading ? (
        <div className="text-center py-5">
          <Spinner animation="border" role="status">
            <span className="visually-hidden">Loading...</span>
          </Spinner>
        </div>
      ) : currentConfig ? (
        <>
          <DataTable
            config={currentConfig}
            data={mainTableFilters.paginatedData}
            onEdit={handleEdit}
            onDelete={handleDelete}
            filters={mainTableFilters.filters}
            sortColumn={mainTableFilters.sortColumn}
            sortDirection={mainTableFilters.sortDirection}
            onSort={mainTableFilters.handleSort}
            onFilterChange={mainTableFilters.handleFilterChange}
          />
          <Pagination
            currentPage={mainTableFilters.currentPage}
            totalPages={mainTableFilters.totalPages}
            totalRecords={mainTableFilters.totalRecords}
            originalRecordCount={mainTableFilters.originalRecordCount}
            itemsPerPage={mainTableFilters.itemsPerPage}
            onPageChange={mainTableFilters.setCurrentPage}
            onItemsPerPageChange={mainTableFilters.setItemsPerPage}
          />
        </>
      ) : (
        <div className="text-center py-5 text-muted">
          Please select a table to view
        </div>
      )}

      <CrudModal
        show={showCrudModal}
        mode={crudMode}
        title={crudModalTitle}
        formData={crudFormData}
        formFields={crudFormFields}
        singularName={getActiveConfig()?.singularName || ''}
        error={crudError}
        onClose={closeCrudModal}
        onFormChange={handleFormChange}
        onSubmit={handleSubmit}
      />
    </div>
  );
};

export default DatabaseViewer;
