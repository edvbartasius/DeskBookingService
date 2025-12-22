import React from 'react';
import { Modal, Button, Form, Alert } from 'react-bootstrap';
import { FormField } from '../config/tableConfigs.tsx';

interface CrudModalProps {
  show: boolean;
  mode: 'add' | 'edit' | 'delete';
  title: string;
  formData: any;
  formFields: FormField[];
  singularName: string;
  error: string | null;
  onClose: () => void;
  onFormChange: (key: string, value: any) => void;
  onSubmit: () => void;
}

export const CrudModal: React.FC<CrudModalProps> = ({
  show,
  mode,
  title,
  formData,
  formFields,
  singularName,
  error,
  onClose,
  onFormChange,
  onSubmit
}) => {
  return (
    <Modal show={show} onHide={onClose} size="lg" centered>
      <Modal.Header closeButton>
        <Modal.Title>{title}</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        {error && (
          <Alert variant="danger" dismissible onClose={() => {}}>
            {error}
          </Alert>
        )}

        {mode === 'delete' ? (
          <div>
            <Alert variant="warning">
              Are you sure you want to delete this {singularName}?
            </Alert>
          </div>
        ) : (
          <Form>
            {formFields.map(field => (
              <Form.Group key={field.key} className="mb-3">
                <Form.Label>
                  {field.label}
                  {field.required && <span className="text-danger"> *</span>}
                </Form.Label>

                {field.type === 'select' ? (
                  <Form.Select
                    value={formData[field.key] ?? ''}
                    onChange={(e) => onFormChange(field.key, e.target.value)}
                    disabled={field.disabled}
                    required={field.required}
                  >
                    <option value="">Select {field.label}</option>
                    {field.options?.map(opt => (
                      <option key={opt.value} value={opt.value}>
                        {opt.label}
                      </option>
                    ))}
                  </Form.Select>
                ) : field.type === 'textarea' ? (
                  <Form.Control
                    as="textarea"
                    rows={3}
                    value={formData[field.key] ?? ''}
                    onChange={(e) => onFormChange(field.key, e.target.value)}
                    disabled={field.disabled}
                    required={field.required}
                  />
                ) : (
                  <Form.Control
                    type={field.type}
                    value={formData[field.key] ?? ''}
                    onChange={(e) => onFormChange(field.key, e.target.value)}
                    disabled={field.disabled}
                    required={field.required}
                  />
                )}
              </Form.Group>
            ))}
          </Form>
        )}
      </Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" onClick={onClose}>
          Cancel
        </Button>
        <Button
          variant={mode === 'delete' ? 'danger' : 'primary'}
          onClick={onSubmit}
        >
          {mode === 'delete' ? 'Delete' : mode === 'add' ? 'Create' : 'Update'}
        </Button>
      </Modal.Footer>
    </Modal>
  );
};
