import { PlusOutlined } from '@ant-design/icons';
import type { ActionType, ProColumns } from '@ant-design/pro-components';
import {
  ModalForm,
  PageContainer,
  ProFormSelect,
  ProFormSwitch,
  ProFormText,
  ProTable,
} from '@ant-design/pro-components';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { FormattedMessage } from '@umijs/max';
import { Button, message, Popconfirm, Space, Tag } from 'antd';
import React, { useRef, useState } from 'react';
import { queryRoles } from '../roles/service';
import type { UserItem } from './data.d';
import {
  createUser,
  deactivateUser,
  queryUsers,
  updateUser,
} from './service';

const UserListPage: React.FC = () => {
  const actionRef = useRef<ActionType | null>(null);
  const queryClient = useQueryClient();
  const [messageApi, contextHolder] = message.useMessage();
  const [createOpen, setCreateOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [editing, setEditing] = useState<UserItem | null>(null);

  const { data: roles = [] } = useQuery({
    queryKey: ['admin-roles-options'],
    queryFn: async () => {
      const result = await queryRoles();
      return result.data;
    },
  });

  const roleOptions = roles.map((r) => ({ label: r.name, value: r.name }));

  const invalidate = () => {
    actionRef.current?.reload();
    queryClient.invalidateQueries({ queryKey: ['admin-users'] });
  };

  const createMutation = useMutation({
    mutationFn: createUser,
    onSuccess: () => {
      messageApi.success('User created');
      setCreateOpen(false);
      invalidate();
    },
    onError: () => messageApi.error('Failed to create user'),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, body }: { id: string; body: Parameters<typeof updateUser>[1] }) =>
      updateUser(id, body),
    onSuccess: () => {
      messageApi.success('User updated');
      setEditOpen(false);
      setEditing(null);
      invalidate();
    },
    onError: () => messageApi.error('Failed to update user'),
  });

  const deactivateMutation = useMutation({
    mutationFn: deactivateUser,
    onSuccess: () => {
      messageApi.success('User deactivated');
      invalidate();
    },
    onError: () => messageApi.error('Failed to deactivate user'),
  });

  const columns: ProColumns<UserItem>[] = [
    { title: 'Email', dataIndex: 'email', copyable: true },
    { title: 'Display name', dataIndex: 'displayName' },
    {
      title: 'Roles',
      dataIndex: 'roles',
      render: (_, record) =>
        record.roles?.map((role) => <Tag key={role}>{role}</Tag>) ?? '—',
    },
    {
      title: 'Permissions',
      dataIndex: 'permissions',
      ellipsis: true,
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
      title: 'Status',
      dataIndex: 'isActive',
      render: (_, record) => (
        <Tag color={record.isActive ? 'green' : 'red'}>
          {record.isActive ? 'Active' : 'Inactive'}
        </Tag>
      ),
    },
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
          {record.isActive && (
            <Popconfirm
              title="Deactivate this user?"
              onConfirm={() => deactivateMutation.mutate(record.id)}
            >
              <a>Deactivate</a>
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  return (
    <PageContainer>
      {contextHolder}
      <ProTable<UserItem>
        headerTitle={<FormattedMessage id="pages.users.title" defaultMessage="Users" />}
        actionRef={actionRef}
        rowKey="id"
        search={false}
        request={queryUsers}
        columns={columns}
        toolBarRender={() => [
          <Button key="create" type="primary" icon={<PlusOutlined />} onClick={() => setCreateOpen(true)}>
            <FormattedMessage id="pages.users.create" defaultMessage="New user" />
          </Button>,
        ]}
      />

      <ModalForm
        title="Create user"
        open={createOpen}
        onOpenChange={setCreateOpen}
        modalProps={{ destroyOnClose: true }}
        onFinish={async (values) => {
          await createMutation.mutateAsync(values as Parameters<typeof createUser>[0]);
          return true;
        }}
      >
        <ProFormText name="email" label="Email" rules={[{ required: true, type: 'email' }]} />
        <ProFormText name="displayName" label="Display name" rules={[{ required: true }]} />
        <ProFormText.Password
          name="password"
          label="Password"
          rules={[{ required: true, min: 8 }]}
        />
        <ProFormSelect
          name="roles"
          label="Roles"
          mode="multiple"
          options={roleOptions}
          fieldProps={{ placeholder: 'Select roles' }}
        />
      </ModalForm>

      <ModalForm
        title="Edit user"
        open={editOpen}
        initialValues={
          editing
            ? {
                displayName: editing.displayName,
                roles: editing.roles,
                isActive: editing.isActive,
              }
            : undefined
        }
        onOpenChange={(open) => {
          setEditOpen(open);
          if (!open) setEditing(null);
        }}
        modalProps={{ destroyOnClose: true }}
        onFinish={async (values) => {
          if (!editing) return false;
          await updateMutation.mutateAsync({
            id: editing.id,
            body: values as Parameters<typeof updateUser>[1],
          });
          return true;
        }}
      >
        <ProFormText name="displayName" label="Display name" rules={[{ required: true }]} />
        <ProFormSelect
          name="roles"
          label="Roles"
          mode="multiple"
          options={roleOptions}
          rules={[{ required: true }]}
        />
        <ProFormSwitch name="isActive" label="Active" />
        <ProFormText.Password
          name="password"
          label="New password"
          placeholder="Leave blank to keep current"
          rules={[{ min: 8 }]}
        />
      </ModalForm>
    </PageContainer>
  );
};

export default UserListPage;
