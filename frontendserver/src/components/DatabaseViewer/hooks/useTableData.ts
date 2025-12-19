import { useState } from 'react';
import { TableName } from '../../../types/database.types.tsx';
import { TableConfig } from '../config/tableConfigs.tsx';

export const useTableData = () => {
  const [selectedTable, setSelectedTable] = useState<TableName | null>(null);
  const [tableData, setTableData] = useState<any[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const fetchTableData = async (tableName: TableName, tableConfigs: TableConfig[]) => {
    setLoading(true);
    setError(null);

    try {
      const config = tableConfigs.find(c => c.name === tableName);
      if (!config) {
        throw new Error(`No configuration found for table: ${tableName}`);
      }

      console.log(`Fetching ${tableName}`);
      const data = await config.fetchData();
      setTableData(data);
    } catch (err) {
      setError(`Failed to fetch ${tableName} data. ${err instanceof Error ? err.message : 'Unknown error'}`);
      setTableData([]);
    } finally {
      setLoading(false);
    }
  };

  const selectTable = (table: TableName, tableConfigs: TableConfig[]) => {
    setSelectedTable(table);
    fetchTableData(table, tableConfigs);
  };

  return {
    selectedTable,
    tableData,
    loading,
    error,
    setError,
    fetchTableData,
    selectTable
  };
};
