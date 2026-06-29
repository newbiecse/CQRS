import { PlusOutlined } from '@ant-design/icons';
import type { ActionType, ProColumns } from '@ant-design/pro-components';
import {
  ModalForm,
  PageContainer,
  ProFormText,
  ProTable,
} from '@ant-design/pro-components';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { FormattedMessage } from '@umijs/max';
import { Button, message, Popconfirm, Space } from 'antd';
import React, { useRef, useState } from 'react';
import type { PermissionItem } from './data.d';
import {
  createPermission,
  deletePermission,
  queryPermissions,
  updatePermission,
} from './service';

const PermissionListPage: React.FC = () => {
  const actionRef = useRef<ActionType | null>(null);
  const queryClient = useQueryClient();
  const [messageApi, contextHolder] = message.useMessage();
  const [createOpen, setCreateOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [editing, setEditing] = useState<PermissionItem | null>(null);

  const invalidate = () => {
    actionRef.current?.reload();
    queryClient.invalidateQueries({ queryKey: ['admin-permissions'] });
    queryClient.invalidateQueries({ queryKey: ['admin-permissions-options'] });
  };

  const createMutation = useMutation({
    mutationFn: createPermission,
    onSuccess: () => {
      messageApi.success('Permission created');
      setCreateOpen(false);
      invalidate();
    },
    onError: () => messageApi.error('Failed to create permission'),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, body }: { id: string; body: Parameters<typeof updatePermission>[1] }) =>
      updatePermission(id, body),
    onSuccess: () => {
      messageApi.success('Permission updated');
      setEditOpen(false);
      setEditing(null);
      invalidate();
    },
    onError: () => messageApi.error('Failed to update permission'),
  });

  const deleteMutation = useMutation({
    mutationFn: deletePermission,
    onSuccess: () => {
      messageApi.success('Permission deleted');
      invalidate();
    },
    onError: () => messageApi.error('Failed to delete permission'),
  });

  const columns: ProColumns<PermissionItem>[] = [
    { title: 'Claim name', dataIndex: 'name', copyable: true },
    { title: 'Description', dataIndex: 'description', ellipsis: true },
    {
      title: 'Created',
      dataIndex: 'createdAt',
      valueType: 'dateTime',
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
          <Popconfirm title="Delete this permission?" onConfirm={() => deleteMutation.mutate(record.id)}>
            <a>Delete</a>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <PageContainer>
      {contextHolder}
      <ProTable<PermissionItem>
        headerTitle={
          <FormattedMessage id="pages.permissions.title" defaultMessage="Permissions" />
        }
        actionRef={actionRef}
        rowKey="id"
        search={false}
        request={queryPermissions}
        columns={columns}
        toolBarRender={() => [
          <Button key="create" type="primary" icon={<PlusOutlined />} onClick={() => setCreateOpen(true)}>
            <FormattedMessage id="pages.permissions.create" defaultMessage="New permission" />
          </Button>,
        ]}
      />

      <ModalForm
        title="Create permission"
        open={createOpen}
        onOpenChange={setCreateOpen}
        modalProps={{ destroyOnClose: true }}
        onFinish={async (values) => {
          await createMutation.mutateAsync(values as Parameters<typeof createPermission>[0]);
          return true;
        }}
      >
        <ProFormText
          name="name"
          label="Claim name"
          rules={[
            {
              required: true,
              pattern: /^[a-z0-9.]+$/,
              message: 'Lowercase letters, numbers, dots only (e.g. catalog.manage)',
            },
          ]}
          fieldProps={{ placeholder: 'e.g. catalog.manage' }}
        />
        <ProFormText name="description" label="Description" rules={[{ required: true }]} />
      </ModalForm>

      <ModalForm
        title="Edit permission"
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
            body: values as Parameters<typeof updatePermission>[1],
          });
          return true;
        }}
      >
        <ProFormText name="description" label="Description" rules={[{ required: true }]} />
      </ModalForm>
    </PageContainer>
  );
};

export default PermissionListPage;
