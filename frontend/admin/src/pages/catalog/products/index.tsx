import { PlusOutlined } from '@ant-design/icons';
import type { ActionType, ProColumns } from '@ant-design/pro-components';
import { ModalForm, PageContainer, ProFormDigit, ProFormText, ProTable } from '@ant-design/pro-components';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { FormattedMessage, history, useIntl } from '@umijs/max';
import { Button, message, Popconfirm, Space } from 'antd';
import React, { useMemo, useRef, useState } from 'react';
import type { ProductItem } from './data.d';
import {
  createProduct,
  deleteProduct,
  queryProducts,
  updateProduct,
} from './service';
import { adjustInventory, queryInventory } from '../../orders/service';

const ProductFormFields: React.FC<{ showStock?: boolean }> = ({ showStock }) => (
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
    {showStock && (
      <ProFormDigit
        name="onHand"
        label="On hand"
        min={0}
        fieldProps={{ precision: 0 }}
        rules={[{ required: true, message: 'Stock quantity is required' }]}
      />
    )}
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

  const { data: inventory = [] } = useQuery({
    queryKey: ['admin-inventory'],
    queryFn: queryInventory,
  });

  const stockByProduct = useMemo(
    () => new Map(inventory.map((item) => [item.productId, item])),
    [inventory],
  );

  const invalidate = () => {
    actionRef.current?.reload();
    queryClient.invalidateQueries({ queryKey: ['admin-products'] });
    queryClient.invalidateQueries({ queryKey: ['admin-inventory'] });
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
      title: 'Available',
      dataIndex: 'id',
      search: false,
      width: 100,
      render: (_, record) => stockByProduct.get(record.id)?.available ?? '—',
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
        initialValues={
          editing
            ? {
                ...editing,
                onHand: stockByProduct.get(editing.id)?.onHand ?? 0,
              }
            : undefined
        }
        onFinish={async (values) => {
          if (!editing) return false;
          await updateMutation.mutateAsync({
            id: editing.id,
            body: { name: values.name, price: values.price },
          });
          if (typeof values.onHand === 'number') {
            await adjustInventory(editing.id, values.onHand);
          }
          invalidate();
          return true;
        }}
        modalProps={{ destroyOnClose: true }}
      >
        <ProductFormFields showStock />
      </ModalForm>
    </PageContainer>
  );
};

export default ProductListPage;
