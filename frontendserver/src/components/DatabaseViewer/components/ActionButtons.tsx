import React from 'react';
import { ButtonGroup, Button } from 'react-bootstrap';

interface ActionButtonsProps {
  onEdit: () => void;
  onDelete: () => void;
}

export const ActionButtons: React.FC<ActionButtonsProps> = ({ onEdit, onDelete }) => {
  return (
    <ButtonGroup size="sm">
      <Button variant="outline-primary" onClick={onEdit}>
        Edit
      </Button>
      <Button variant="outline-danger" onClick={onDelete}>
        Delete
      </Button>
    </ButtonGroup>
  );
};
