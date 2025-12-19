import { useState } from 'react';
import api from '../../../services/api.ts';
import { TableName, User, Building, Desk } from '../../../types/database.types.tsx';
import { FormField } from '../config/tableConfigs.tsx';
import { validateFormData } from '../utils/validators.ts';

export const useCrudOperations = () => {
  const [showCrudModal, setShowCrudModal] = useState<boolean>(false);
  const [crudMode, setCrudMode] = useState<'add' | 'edit' | 'delete'>('add');
  const [crudFormData, setCrudFormData] = useState<any>({});
  const [crudModalTitle, setCrudModalTitle] = useState<string>('');
  const [crudFormFields, setCrudFormFields] = useState<FormField[]>([]);
  const [crudError, setCrudError] = useState<string | null>(null);

  const getDynamicFormFields = (
    baseFields: FormField[],
    users: User[],
    buildings: Building[],
    desks: Desk[],
    contextData?: any
  ): FormField[] => {
    return baseFields
      .filter(field => field.key !== 'id')  // Skip ID field (don't show it)
      .map(field => {
        // If context data has this field, mark it as disabled and pre-filled
        if (contextData && contextData.hasOwnProperty(field.key)) {
          return {
            ...field,
            disabled: true  // Lock the field
          };
        }
      if (field.key === 'userId') {
        return {
          ...field,
          type: 'select' as const,
          options: users.map(user => ({
            value: user.id,
            label: `${user.name} ${user.surname} (${user.email})`
          }))
        };
      }

      if (field.key === 'deskId') {
        return {
          ...field,
          type: 'select' as const,
          options: desks.map(desk => ({
            value: desk.id,
            label: `Desk ${desk.id}${desk.description ? ' - ' + desk.description : ''} (${desk.buildingName})`
          }))
        };
      }

      if (field.key === 'buildingId') {
        return {
          ...field,
          type: 'select' as const,
          options: buildings.map(building => ({
            value: building.id,
            label: building.name
          }))
        };
      }

      return field;
    });
  };

  const openAddModal = (
    singularName: string,
    formFields: FormField[],
    users: User[],
    buildings: Building[],
    desks: Desk[],
    contextData?: any
  ) => {
    setCrudMode('add');
    setCrudModalTitle(`Add New ${singularName}`);
    const dynamicFields = getDynamicFormFields(formFields, users, buildings, desks, contextData);
    setCrudFormFields(dynamicFields);
    setCrudFormData(contextData || {});
    setCrudError(null);
    setShowCrudModal(true);
  };

  const openEditModal = (
    record: any,
    singularName: string,
    formFields: FormField[],
    users: User[],
    buildings: Building[],
    desks: Desk[]
  ) => {
    setCrudMode('edit');
    setCrudModalTitle(`Edit ${singularName}`);
    const dynamicFields = getDynamicFormFields(formFields, users, buildings, desks);
    setCrudFormFields(dynamicFields);
    setCrudFormData({ ...record });
    setCrudError(null);
    setShowCrudModal(true);
  };

  const openDeleteModal = (record: any, singularName: string) => {
    setCrudMode('delete');
    setCrudModalTitle(`Delete ${singularName}`);
    setCrudFormData({ ...record });
    setCrudError(null);
    setShowCrudModal(true);
  };

  const closeCrudModal = () => {
    setShowCrudModal(false);
    setCrudFormData({});
    setCrudFormFields([]);
    setCrudError(null);
  };

  const handleFormChange = (key: string, value: any) => {
    setCrudFormData((prev: any) => ({ ...prev, [key]: value }));
  };

  const handleCrudSubmit = async (targetTable: TableName, onSuccess: () => void) => {
    setCrudError(null);

    // Frontend validation before API call (skip for delete operations)
    if (crudMode !== 'delete') {
      const validationError = validateFormData(targetTable, crudFormData);
      if (validationError) {
        setCrudError(validationError);
        return; // Stop submission if validation fails
      }
    }

    try {
      let response;
      switch (crudMode) {
        case 'add':
          response = await api.post(`admin/${targetTable}`, crudFormData);
          console.log('Add response:', response);
          break;
        case 'edit':
          const editId = crudFormData.id;
          response = await api.put(`admin/${targetTable}/${editId}`, crudFormData);
          console.log('Edit response:', response);
          break;
        case 'delete':
          const deleteId = crudFormData.id;
          response = await api.delete(`admin/${targetTable}/${deleteId}`);
          console.log('Delete response:', response);
          break;
      }
      closeCrudModal();
      onSuccess();
    } catch (error: any) {
      console.error('Operation failed:', error);
      // Extract error message from backend response
      const errorMessage = error.response?.data?.error || error.message || 'An unexpected error occurred';
      setCrudError(errorMessage);
    }
  };

  return {
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
  };
};
