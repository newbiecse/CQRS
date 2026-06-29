import { PlusOutlined } from '@ant-design/icons';
import type { ActionType, ProColumns } from '@ant-design/pro-components';
import { ModalForm, PageContainer, ProFormDigit, ProFormText, ProTable } from '@ant-design/pro-components';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { FormattedMessage, history, useIntl } from '@umijs/max';
import { Button, message, Popconfirm, Space } from 'antd';
import React, { useRef, useState } from 'react';
import type { ProductItem } from './data.d';
import {
  createProduct,
  deleteProduct,
  queryProducts,
  updateProduct,
} from './service';

const ProductFormFields: React.FC = () => (
  <>
    <ProFormText
      name="name"
      label="Name"
      rules={[{ required: true, message: 'Product name is required' }]}
    />
    <ProFormDigit
      name="price"
      label="Price"
      min={0}
      fieldProps={{ precision: 2 }}
      rules={[{ required: true, message: 'Price is required' }]}
    />
  </>
);

const ProductListPage: React.FC = () => {
  const actionRef = useRef<ActionType | null>(null);
  const queryClient = useQueryClient();
  const intl = useIntl();
  const [messageApi, contextHolder] = message.useMessage();
  const [createOpen, setCreateOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [editing, setEditing] = useState<ProductItem | null>(null);

  const invalidate = () => {
    actionRef.current?.reload();
    queryClient.invalidateQueries({ queryKey: ['admin-products'] });
  };

  const createMutation = useMutation({
    mutationFn: createProduct,
    onSuccess: () => {
      messageApi.success('Product created');
      setCreateOpen(false);
      invalidate();
    },
    onError: () => messageApi.error('Failed to create product'),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, body }: { id: string; body: { name: string; price: number } }) =>
      updateProduct(id, body),
    onSuccess: () => {
      messageApi.success('Product updated');
      setEditOpen(false);
      setEditing(null);
      invalidate();
    },
    onError: () => messageApi.error('Failed to update product'),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteProduct,
    onSuccess: () => {
      messageApi.success('Product deleted');
      invalidate();
    },
    onError: () => messageApi.error('Failed to delete product'),
  });

  const columns: ProColumns<ProductItem>[] = [
    {
      title: 'Name',
      dataIndex: 'name',
      ellipsis: true,
      render: (_, record) => (
        <Button
          type="link"
          style={{ padding: 0 }}
          onClick={() => history.push(`/catalog/products/${record.id}`)}
        >
          {record.name}
        </Button>
      ),
    },
    {
      title: 'Price',
      dataIndex: 'price',
      valueType: 'money',
      search: false,
    },
    {
      title: 'Created',
      dataIndex: 'createdAt',
      valueType: 'dateTime',
      search: false,
    },
    {
      title: 'Updated',
      dataIndex: 'lastUpdatedAt',
      valueType: 'dateTime',
      search: false,
    },
    {
      title: 'Actions',
      valueType: 'option',
      render: (_, record) => (
        <Space>
          <Button
            type="link"
            onClick={() => history.push(`/catalog/products/${record.id}`)}
          >
            View
          </Button>
          <Button
            type="link"
            onClick={() => {
              setEditing(record);
              setEditOpen(true);
            }}
          >
            Edit
          </Button>
          <Popconfirm
            title="Delete this product?"
            onConfirm={() => deleteMutation.mutate(record.id)}
          >
            <Button type="link" danger loading={deleteMutation.isPending}>
              Delete
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <PageContainer>
      {contextHolder}
      <ProTable<ProductItem>
        headerTitle={intl.formatMessage({
          id: 'pages.products.title',
          defaultMessage: 'Products',
        })}
        actionRef={actionRef}
        rowKey="id"
        search={{ labelWidth: 'auto' }}
        toolBarRender={() => [
          <Button
            key="create"
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => setCreateOpen(true)}
          >
            <FormattedMessage id="pages.products.create" defaultMessage="New product" />
          </Button>,
        ]}
        request={queryProducts}
        columns={columns}
        pagination={{ pageSize: 10 }}
      />

      <ModalForm
        title="New product"
        open={createOpen}
        onOpenChange={setCreateOpen}
        onFinish={async (values) => {
          await createMutation.mutateAsync(values as { name: string; price: number });
          return true;
        }}
        modalProps={{ destroyOnClose: true }}
      >
        <ProductFormFields />
      </ModalForm>

      <ModalForm
        title="Edit product"
        open={editOpen}
        onOpenChange={(open) => {
          setEditOpen(open);
          if (!open) setEditing(null);
        }}
        initialValues={editing ?? undefined}
        onFinish={async (values) => {
          if (!editing) return false;
          await updateMutation.mutateAsync({
            id: editing.id,
            body: values as { name: string; price: number },
          });
          return true;
        }}
        modalProps={{ destroyOnClose: true }}
      >
        <ProductFormFields />
      </ModalForm>
    </PageContainer>
  );
};

export default ProductListPage;
