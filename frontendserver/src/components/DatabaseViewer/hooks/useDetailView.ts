import { useState } from 'react';
import { TableName } from '../../../types/database.types.tsx';
import { ColumnConfig, DetailViewConfig } from '../config/tableConfigs.tsx';

export const useDetailView = () => {
  const [showDetailView, setShowDetailView] = useState<boolean>(false);
  const [detailViewTitle, setDetailViewTitle] = useState<string>('');
  const [detailViewData, setDetailViewData] = useState<any[]>([]);
  const [detailViewColumns, setDetailViewColumns] = useState<ColumnConfig[]>([]);
  const [detailViewLoading, setDetailViewLoading] = useState<boolean>(false);
  const [detailViewTableType, setDetailViewTableType] = useState<TableName | null>(null);
  const [detailViewParentRecord, setDetailViewParentRecord] = useState<any>(null);

  const handleViewDetails = async (record: any, config: DetailViewConfig, detailTableType: TableName) => {
    setDetailViewTitle(config.title(record));
    setDetailViewColumns(config.columns);
    setDetailViewTableType(detailTableType);
    setDetailViewParentRecord(record);
    setShowDetailView(true);
    setDetailViewLoading(true);

    try {
      const data = await config.fetchData(record);
      setDetailViewData(data);
    } catch (err) {
      console.error('Failed to fetch detail data:', err);
      setDetailViewData([]);
    } finally {
      setDetailViewLoading(false);
    }
  };

  const handleCloseDetailView = () => {
    setShowDetailView(false);
    setDetailViewData([]);
    setDetailViewColumns([]);
    setDetailViewTitle('');
    setDetailViewTableType(null);
    setDetailViewParentRecord(null);
  };

  return {
    showDetailView,
    detailViewTitle,
    detailViewData,
    detailViewColumns,
    detailViewLoading,
    detailViewTableType,
    detailViewParentRecord,
    handleViewDetails,
    handleCloseDetailView
  };
};
