import { PlusOutlined } from '@ant-design/icons';
import type { ActionType, ProColumns } from '@ant-design/pro-components';
import {
  ModalForm,
  PageContainer,
  ProFormSelect,
  ProFormText,
  ProTable,
} from '@ant-design/pro-components';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { FormattedMessage } from '@umijs/max';
import { Button, message, Popconfirm, Space, Tag } from 'antd';
import React, { useRef, useState } from 'react';
import { queryPermissions } from '../permissions/service';
import type { RoleItem } from './data.d';
import {
  createRole,
  deleteRole,
  queryRoles,
  setRolePermissions,
  updateRole,
} from './service';

const RoleListPage: React.FC = () => {
  const actionRef = useRef<ActionType | null>(null);
  const queryClient = useQueryClient();
  const [messageApi, contextHolder] = message.useMessage();
  const [createOpen, setCreateOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [permOpen, setPermOpen] = useState(false);
  const [editing, setEditing] = useState<RoleItem | null>(null);

  const { data: permissions = [] } = useQuery({
    queryKey: ['admin-permissions-options'],
    queryFn: async () => {
      const result = await queryPermissions();
      return result.data;
    },
  });

  const permissionOptions = permissions.map((p) => ({ label: p.name, value: p.name }));

  const invalidate = () => {
    actionRef.current?.reload();
    queryClient.invalidateQueries({ queryKey: ['admin-roles'] });
    queryClient.invalidateQueries({ queryKey: ['admin-roles-options'] });
  };

  const createMutation = useMutation({
    mutationFn: createRole,
    onSuccess: () => {
      messageApi.success('Role created');
      setCreateOpen(false);
      invalidate();
    },
    onError: () => messageApi.error('Failed to create role'),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, body }: { id: string; body: Parameters<typeof updateRole>[1] }) =>
      updateRole(id, body),
    onSuccess: () => {
      messageApi.success('Role updated');
      setEditOpen(false);
      setEditing(null);
      invalidate();
    },
    onError: () => messageApi.error('Failed to update role'),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteRole,
    onSuccess: () => {
      messageApi.success('Role deleted');
      invalidate();
    },
    onError: () => messageApi.error('Failed to delete role (system roles cannot be removed)'),
  });

  const permMutation = useMutation({
    mutationFn: ({ id, permissions: perms }: { id: string; permissions: string[] }) =>
      setRolePermissions(id, perms),
    onSuccess: () => {
      messageApi.success('Permissions updated');
      setPermOpen(false);
      setEditing(null);
      invalidate();
    },
    onError: () => messageApi.error('Failed to update permissions'),
  });

  const columns: ProColumns<RoleItem>[] = [
    { title: 'Name', dataIndex: 'name' },
    { title: 'Description', dataIndex: 'description', ellipsis: true },
    {
      title: 'Permissions',
      dataIndex: 'permissions',
      render: (_, record) =>
        record.permissions?.length
          ? record.permissions.map((p) => (
              <Tag key={p} color="blue">
                {p}
              </Tag>
            ))
          : '—',
    },
    {
      title: 'System',
      dataIndex: 'isSystem',
      render: (_, record) => (
        <Tag color={record.isSystem ? 'purple' : 'default'}>
          {record.isSystem ? 'Yes' : 'No'}
        </Tag>
      ),
    },
    {
      title: 'Actions',
      valueType: 'option',
      render: (_, record) => (
        <Space>
          <a
            onClick={() => {
              setEditing(record);
              setEditOpen(true);
            }}
          >
            Edit
          </a>
          <a
            onClick={() => {
              setEditing(record);
              setPermOpen(true);
            }}
          >
            Permissions
          </a>
          {!record.isSystem && (
            <Popconfirm title="Delete this role?" onConfirm={() => deleteMutation.mutate(record.id)}>
              <a>Delete</a>
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  return (
    <PageContainer>
      {contextHolder}
      <ProTable<RoleItem>
        headerTitle={<FormattedMessage id="pages.roles.title" defaultMessage="Roles" />}
        actionRef={actionRef}
        rowKey="id"
        search={false}
        request={queryRoles}
        columns={columns}
        toolBarRender={() => [
          <Button key="create" type="primary" icon={<PlusOutlined />} onClick={() => setCreateOpen(true)}>
            <FormattedMessage id="pages.roles.create" defaultMessage="New role" />
          </Button>,
        ]}
      />

      <ModalForm
        title="Create role"
        open={createOpen}
        onOpenChange={setCreateOpen}
        modalProps={{ destroyOnClose: true }}
        onFinish={async (values) => {
          await createMutation.mutateAsync(values as Parameters<typeof createRole>[0]);
          return true;
        }}
      >
        <ProFormText
          name="name"
          label="Name"
          rules={[{ required: true, pattern: /^[a-z0-9-]+$/, message: 'Lowercase letters, numbers, hyphens only' }]}
          fieldProps={{ placeholder: 'e.g. support-agent' }}
        />
        <ProFormText name="description" label="Description" rules={[{ required: true }]} />
      </ModalForm>

      <ModalForm
        title="Edit role"
        open={editOpen}
        initialValues={editing ? { description: editing.description } : undefined}
        onOpenChange={(open) => {
          setEditOpen(open);
          if (!open) setEditing(null);
        }}
        modalProps={{ destroyOnClose: true }}
        onFinish={async (values) => {
          if (!editing) return false;
          await updateMutation.mutateAsync({
            id: editing.id,
            body: values as Parameters<typeof updateRole>[1],
          });
          return true;
        }}
      >
        <ProFormText name="description" label="Description" rules={[{ required: true }]} />
      </ModalForm>

      <ModalForm
        title={`Permissions — ${editing?.name ?? ''}`}
        open={permOpen}
        initialValues={editing ? { permissions: editing.permissions } : undefined}
        onOpenChange={(open) => {
          setPermOpen(open);
          if (!open) setEditing(null);
        }}
        modalProps={{ destroyOnClose: true }}
        onFinish={async (values) => {
          if (!editing) return false;
          await permMutation.mutateAsync({
            id: editing.id,
            permissions: (values.permissions as string[]) ?? [],
          });
          return true;
        }}
      >
        <ProFormSelect
          name="permissions"
          label="Permission claims"
          mode="multiple"
          options={permissionOptions}
          fieldProps={{ placeholder: 'Select permissions' }}
        />
      </ModalForm>
    </PageContainer>
  );
};

export default RoleListPage;
