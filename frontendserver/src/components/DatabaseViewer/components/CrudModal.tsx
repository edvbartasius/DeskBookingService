import React from 'react';
import { Button, Form, Alert } from 'react-bootstrap';
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
  if (!show) return null;

  return (
    <div
      className="crud-modal-overlay"
      style={{
        position: 'absolute',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        backgroundColor: 'rgba(0, 0, 0, 0.5)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        zIndex: 1000
      }}
    >
      <div
        className="crud-modal-content bg-white rounded p-4"
        style={{
          maxWidth: '600px', // TODO: Make it fit inside the card
          width: '90%',
          maxHeight: '80vh',
          overflow: 'auto'
        }}
      >
        <div className="d-flex justify-content-between align-items-center mb-3">
          <h5 className="mb-0">{title}</h5>
          <Button variant="outline-secondary" size="sm" onClick={onClose}>
            ✕
          </Button>
        </div>

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
            <div className="mb-3">
              <strong>Details:</strong>
              <pre className="mt-2 p-2 bg-light rounded">
                {JSON.stringify(formData, null, 2)}
              </pre>
            </div>
            <div className="d-flex gap-2 justify-content-end">
              <Button variant="secondary" onClick={onClose}>
                Cancel
              </Button>
              <Button variant="danger" onClick={onSubmit}>
                Delete
              </Button>
            </div>
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

            <div className="d-flex gap-2 justify-content-end mt-4">
              <Button variant="secondary" onClick={onClose}>
                Cancel
              </Button>
              <Button variant="primary" onClick={onSubmit}>
                {mode === 'add' ? 'Create' : 'Update'}
              </Button>
            </div>
          </Form>
        )}
      </div>
    </div>
  );
};
