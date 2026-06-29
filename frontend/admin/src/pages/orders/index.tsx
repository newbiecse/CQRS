import { PlusOutlined } from '@ant-design/icons';
import type { ActionType, ProColumns } from '@ant-design/pro-components';
import {
  ModalForm,
  PageContainer,
  ProFormDigit,
  ProFormList,
  ProFormSelect,
  ProFormText,
  ProTable,
} from '@ant-design/pro-components';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { FormattedMessage, history } from '@umijs/max';
import { Button, Input, message, Popconfirm, Space, Tag } from 'antd';
import React, { useMemo, useRef, useState } from 'react';
import { queryProducts } from '../catalog/products/service';
import type { OrderItem, OrderLineInput } from './data.d';
import {
  cancelOrder,
  createOrder,
  deleteOrder,
  markOrderPaid,
  queryInventory,
  queryOrders,
  updateOrder,
} from './service';

const statusColor = (status: string) => {
  if (status === 'Paid') return 'green';
  if (status === 'Cancelled') return 'red';
  return 'gold';
};

const OrderListPage: React.FC = () => {
  const actionRef = useRef<ActionType | null>(null);
  const queryClient = useQueryClient();
  const [messageApi, contextHolder] = message.useMessage();
  const [createOpen, setCreateOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [editing, setEditing] = useState<OrderItem | null>(null);
  const [cancelOpen, setCancelOpen] = useState(false);
  const [cancelTarget, setCancelTarget] = useState<OrderItem | null>(null);
  const [cancelReason, setCancelReason] = useState('');

  const { data: products = [] } = useQuery({
    queryKey: ['admin-products-options'],
    queryFn: async () => {
      const result = await queryProducts();
      return result.data;
    },
  });

  const { data: inventory = [] } = useQuery({
    queryKey: ['admin-inventory'],
    queryFn: queryInventory,
  });

  const inventoryByProduct = useMemo(
    () => new Map(inventory.map((item) => [item.productId, item])),
    [inventory],
  );

  const productOptions = products.map((p) => ({
    label: `${p.name} (avail: ${inventoryByProduct.get(p.id)?.available ?? '—'})`,
    value: p.id,
    name: p.name,
    price: p.price,
    available: inventoryByProduct.get(p.id)?.available ?? 0,
  }));

  const invalidate = () => {
    actionRef.current?.reload();
    queryClient.invalidateQueries({ queryKey: ['admin-orders'] });
    queryClient.invalidateQueries({ queryKey: ['admin-inventory'] });
  };

  const createMutation = useMutation({
    mutationFn: createOrder,
    onSuccess: () => {
      messageApi.success('Order created');
      setCreateOpen(false);
      invalidate();
    },
    onError: () => messageApi.error('Failed to create order (check stock availability)'),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, lines }: { id: string; lines: OrderLineInput[] }) =>
      updateOrder(id, { lines }),
    onSuccess: () => {
      messageApi.success('Order updated');
      setEditOpen(false);
      setEditing(null);
      invalidate();
    },
    onError: () => messageApi.error('Failed to update order'),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteOrder,
    onSuccess: () => {
      messageApi.success('Order deleted');
      invalidate();
    },
    onError: () => messageApi.error('Failed to delete order'),
  });

  const cancelMutation = useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) => cancelOrder(id, reason),
    onSuccess: () => {
      messageApi.success('Order cancelled');
      setCancelOpen(false);
      setCancelTarget(null);
      setCancelReason('');
      invalidate();
    },
    onError: () => messageApi.error('Failed to cancel order'),
  });

  const markPaidMutation = useMutation({
    mutationFn: (order: OrderItem) =>
      markOrderPaid(order.id, crypto.randomUUID(), order.totalAmount),
    onSuccess: () => {
      messageApi.success('Order marked as paid');
      invalidate();
    },
    onError: () => messageApi.error('Failed to mark order as paid'),
  });

  const columns: ProColumns<OrderItem>[] = [
    {
      title: 'Order ID',
      dataIndex: 'id',
      copyable: true,
      ellipsis: true,
      width: 120,
      render: (_, record) => (
        <Button
          type="link"
          style={{ padding: 0 }}
          onClick={() => history.push(`/orders/${record.id}`)}
        >
          {record.id.slice(0, 8)}…
        </Button>
      ),
    },
    {
      title: 'Customer',
      dataIndex: 'customerId',
      ellipsis: true,
      width: 120,
    },
    {
      title: 'Status',
      dataIndex: 'status',
      width: 130,
      render: (_, record) => <Tag color={statusColor(record.status)}>{record.status}</Tag>,
    },
    {
      title: 'Lines',
      dataIndex: 'lines',
      width: 80,
      render: (_, record) => record.lines?.length ?? 0,
    },
    {
      title: 'Total',
      dataIndex: 'totalAmount',
      valueType: 'money',
      width: 100,
    },
    {
      title: 'Created',
      dataIndex: 'createdAt',
      valueType: 'dateTime',
      width: 170,
    },
    {
      title: 'Actions',
      valueType: 'option',
      width: 260,
      render: (_, record) => (
        <Space>
          <Button type="link" onClick={() => history.push(`/orders/${record.id}`)}>
            View
          </Button>
          {record.status === 'PendingPayment' && (
            <>
              <Button
                type="link"
                onClick={() => {
                  setEditing(record);
                  setEditOpen(true);
                }}
              >
                Edit
              </Button>
              <Button type="link" onClick={() => markPaidMutation.mutate(record)}>
                Mark paid
              </Button>
              <Button
                type="link"
                onClick={() => {
                  setCancelTarget(record);
                  setCancelOpen(true);
                }}
              >
                Cancel
              </Button>
              <Popconfirm
                title="Delete this order?"
                onConfirm={() => deleteMutation.mutate(record.id)}
              >
                <Button type="link" danger>
                  Delete
                </Button>
              </Popconfirm>
            </>
          )}
        </Space>
      ),
    },
  ];

  const lineFields = (
    <ProFormList
      name="lines"
      label="Order lines"
      creatorButtonProps={{ creatorButtonText: 'Add line' }}
      min={1}
      rules={[{ required: true, message: 'At least one line is required' }]}
    >
      <Space align="start" wrap>
        <ProFormSelect
          name="productId"
          label="Product"
          width="md"
          rules={[{ required: true }]}
          options={productOptions}
          fieldProps={{
            onChange: (value, option) => {
              // option typing from ant design
              const opt = option as { name?: string; price?: number };
              if (!opt?.name) return;
            },
          }}
        />
        <ProFormDigit name="quantity" label="Qty" min={1} width="xs" rules={[{ required: true }]} />
      </Space>
    </ProFormList>
  );

  const buildLines = (values: {
    lines: { productId: string; quantity: number }[];
  }): OrderLineInput[] =>
    values.lines.map((line) => {
      const product = products.find((p) => p.id === line.productId);
      if (!product) throw new Error('Product not found');
      return {
        productId: product.id,
        productName: product.name,
        unitPrice: product.price,
        quantity: line.quantity,
      };
    });

  return (
    <PageContainer>
      {contextHolder}
      <ProTable<OrderItem>
        headerTitle={<FormattedMessage id="pages.orders.title" defaultMessage="Orders" />}
        actionRef={actionRef}
        rowKey="id"
        search={false}
        toolBarRender={() => [
          <Button key="create" type="primary" icon={<PlusOutlined />} onClick={() => setCreateOpen(true)}>
            <FormattedMessage id="pages.orders.create" defaultMessage="New order" />
          </Button>,
        ]}
        request={queryOrders}
        columns={columns}
      />

      <ModalForm
        title="Create order"
        open={createOpen}
        onOpenChange={setCreateOpen}
        onFinish={async (values) => {
          await createMutation.mutateAsync({
            customerId: values.customerId,
            lines: buildLines(values),
          });
          return true;
        }}
      >
        <ProFormText
          name="customerId"
          label="Customer ID"
          rules={[{ required: true, message: 'Customer ID is required' }]}
          placeholder="GUID"
        />
        {lineFields}
      </ModalForm>

      <ModalForm
        title="Edit order lines"
        open={editOpen}
        initialValues={{
          lines: editing?.lines.map((l) => ({
            productId: l.productId,
            quantity: l.quantity,
          })),
        }}
        onOpenChange={(open) => {
          setEditOpen(open);
          if (!open) setEditing(null);
        }}
        onFinish={async (values) => {
          if (!editing) return false;
          await updateMutation.mutateAsync({
            id: editing.id,
            lines: buildLines(values),
          });
          return true;
        }}
      >
        {lineFields}
      </ModalForm>

      <ModalForm
        title="Cancel order"
        open={cancelOpen}
        onOpenChange={(open) => {
          setCancelOpen(open);
          if (!open) {
            setCancelTarget(null);
            setCancelReason('');
          }
        }}
        submitter={{
          submitButtonProps: { danger: true },
          searchConfig: { submitText: 'Cancel order' },
        }}
        onFinish={async () => {
          if (!cancelTarget || !cancelReason.trim()) {
            messageApi.error('Reason is required');
            return false;
          }
          await cancelMutation.mutateAsync({ id: cancelTarget.id, reason: cancelReason });
          return true;
        }}
      >
        <Input.TextArea
          rows={3}
          placeholder="Cancellation reason"
          value={cancelReason}
          onChange={(e) => setCancelReason(e.target.value)}
        />
      </ModalForm>
    </PageContainer>
  );
};

export default OrderListPage;
