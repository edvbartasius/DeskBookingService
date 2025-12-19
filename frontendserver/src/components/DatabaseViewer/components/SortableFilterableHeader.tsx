import React, { useState } from 'react';
import { Form } from 'react-bootstrap';
import { SortDirection } from '../hooks/useTableFilters.ts';

interface SortableFilterableHeaderProps {
  columnKey: string;
  label: string;
  sortColumn: string | null;
  sortDirection: SortDirection;
  filterValue: string;
  onSort: (columnKey: string) => void;
  onFilterChange: (columnKey: string, value: string) => void;
  canFilter?: boolean;
}

export const SortableFilterableHeader: React.FC<SortableFilterableHeaderProps> = ({
  columnKey,
  label,
  sortColumn,
  sortDirection,
  filterValue,
  onSort,
  onFilterChange,
  canFilter = true
}) => {
  const [isExpanded, setIsExpanded] = useState(false);

  const isActive = sortColumn === columnKey;

  const getSortIcon = () => {
    if (!isActive || sortDirection === null) {
      return '⇅'; // Default: both arrows
    }
    if (sortDirection === 'asc') {
      return '↑'; // Ascending
    }
    return '↓'; // Descending
  };

  const handleHeaderClick = () => {
    if (!isExpanded) {
      onSort(columnKey);
    }
  };

  const toggleExpand = (e: React.MouseEvent) => {
    e.stopPropagation();
    setIsExpanded(!isExpanded);
  };

  return (
    <th className="p-0 align-top position-relative">
      <div className="d-flex flex-column w-100">
        <div
          className="d-flex align-items-center justify-content-between p-2 user-select-none"
          onClick={handleHeaderClick}
          style={{ cursor: 'pointer', transition: 'background-color 0.15s' }}
          onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = 'rgba(255,255,255,0.1)')}
          onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = 'transparent')}
        >
          <span
            className="fw-semibold flex-grow-1"
            style={{
              transition: 'all 0.2s',
              fontSize: isExpanded ? '0.85em' : '1em',
              opacity: isExpanded ? 0.9 : 1
            }}
          >
            {label}
          </span>
          <span
            className="px-1"
            style={{
              fontSize: '1rem',
              opacity: isActive ? 1 : 0.4,
              fontWeight: isActive ? 'bold' : 'normal',
              cursor: 'pointer'
            }}
            title="Click to sort"
          >
            {getSortIcon()}
          </span>
          {canFilter && (
            <button
              className="btn btn-sm p-0 d-flex align-items-center justify-content-center"
              onClick={toggleExpand}
              type="button"
              title={isExpanded ? 'Hide filter' : 'Show filter'}
              style={{
                background: 'none',
                border: '1px solid rgba(255,255,255,0.3)',
                borderRadius: '3px',
                color: 'inherit',
                width: '22px',
                height: '22px',
                fontSize: '0.9rem'
              }}
            >
              {isExpanded ? '−' : '+'}
            </button>
          )}
        </div>
        {canFilter && isExpanded && (
          <div className="px-2 pb-2" onClick={(e) => e.stopPropagation()}>
            <Form.Control
              size="sm"
              type="text"
              placeholder="Filter..."
              value={filterValue || ''}
              onChange={(e) => onFilterChange(columnKey, e.target.value)}
              onClick={(e) => e.stopPropagation()}
            />
          </div>
        )}
      </div>
    </th>
  );
};
