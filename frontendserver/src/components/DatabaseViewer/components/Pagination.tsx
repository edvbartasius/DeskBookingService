import React from 'react';
import { Button, ButtonGroup, Form } from 'react-bootstrap';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  totalRecords: number;
  originalRecordCount: number;
  itemsPerPage: number;
  onPageChange: (page: number) => void;
  onItemsPerPageChange: (itemsPerPage: number) => void;
}

export const Pagination: React.FC<PaginationProps> = ({
  currentPage,
  totalPages,
  totalRecords,
  originalRecordCount,
  itemsPerPage,
  onPageChange,
  onItemsPerPageChange
}) => {
  if (totalRecords === 0) return null;

  const startRecord = (currentPage - 1) * itemsPerPage + 1;
  const endRecord = Math.min(currentPage * itemsPerPage, totalRecords);

  const getPageNumbers = () => {
    const pages: (number | string)[] = [];
    const maxVisible = 5;

    if (totalPages <= maxVisible) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      pages.push(1);

      let start = Math.max(2, currentPage - 1);
      let end = Math.min(totalPages - 1, currentPage + 1);

      if (currentPage <= 3) {
        end = 4;
      } else if (currentPage >= totalPages - 2) {
        start = totalPages - 3;
      }

      if (start > 2) {
        pages.push('...');
      }

      for (let i = start; i <= end; i++) {
        pages.push(i);
      }

      if (end < totalPages - 1) {
        pages.push('...');
      }

      pages.push(totalPages);
    }

    return pages;
  };

  return (
    <div className="d-flex justify-content-between align-items-center mt-3 flex-wrap gap-2">
      <div className="d-flex align-items-center gap-2">
        <span className="text-muted small">
          Showing {startRecord}-{endRecord} of {totalRecords}
          {totalRecords !== originalRecordCount && (
            <span> (filtered from {originalRecordCount})</span>
          )}
        </span>
        <Form.Select
          size="sm"
          value={itemsPerPage}
          onChange={(e) => onItemsPerPageChange(Number(e.target.value))}
          style={{ width: 'auto' }}
        >
          <option value={5}>5 per page</option>
          <option value={10}>10 per page</option>
          <option value={25}>25 per page</option>
          <option value={50}>50 per page</option>
          <option value={100}>100 per page</option>
        </Form.Select>
      </div>

      {totalPages > 1 && (
        <ButtonGroup size="sm">
          <Button
            variant="outline-secondary"
            disabled={currentPage === 1}
            onClick={() => onPageChange(currentPage - 1)}
          >
            Previous
          </Button>

          {getPageNumbers().map((page, index) =>
            typeof page === 'number' ? (
              <Button
                key={index}
                variant={currentPage === page ? 'primary' : 'outline-secondary'}
                onClick={() => onPageChange(page)}
              >
                {page}
              </Button>
            ) : (
              <Button key={index} variant="outline-secondary" disabled>
                {page}
              </Button>
            )
          )}

          <Button
            variant="outline-secondary"
            disabled={currentPage === totalPages}
            onClick={() => onPageChange(currentPage + 1)}
          >
            Next
          </Button>
        </ButtonGroup>
      )}
    </div>
  );
};
