import { PageContainer, ProDescriptions, ProTable } from '@ant-design/pro-components';
import type { ProColumns } from '@ant-design/pro-components';
import { history, useParams } from '@umijs/max';
import { useQuery } from '@tanstack/react-query';
import { Button, Card, Result, Spin, Tag } from 'antd';
import React from 'react';
import type { OrderLine } from '../data.d';
import { getOrderById } from '../service';

const statusColor = (status: string) => {
  if (status === 'Paid') return 'green';
  if (status === 'Cancelled') return 'red';
  return 'gold';
};

const OrderDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();

  const { data: order, isLoading, isError, refetch } = useQuery({
    queryKey: ['admin-order', id],
    queryFn: () => getOrderById(id!),
    enabled: Boolean(id),
  });

  const lineColumns: ProColumns<OrderLine>[] = [
    { title: 'Product', dataIndex: 'productName' },
    { title: 'Unit price', dataIndex: 'unitPrice', valueType: 'money' },
    { title: 'Quantity', dataIndex: 'quantity' },
    { title: 'Line total', dataIndex: 'lineTotal', valueType: 'money' },
  ];

  if (!id) {
    return (
      <PageContainer>
        <Result status="404" title="Order not found" />
      </PageContainer>
    );
  }

  if (isLoading) {
    return (
      <PageContainer>
        <Spin style={{ display: 'block', margin: '48px auto' }} />
      </PageContainer>
    );
  }

  if (isError || !order) {
    return (
      <PageContainer>
        <Result
          status="error"
          title="Failed to load order"
          extra={
            <Button type="primary" onClick={() => refetch()}>
              Retry
            </Button>
          }
        />
      </PageContainer>
    );
  }

  return (
    <PageContainer title={`Order ${order.id.slice(0, 8)}…`} onBack={() => history.push('/orders')}>
      <Card bordered={false} style={{ marginBottom: 16 }}>
        <ProDescriptions
          column={2}
          dataSource={order}
          columns={[
            { title: 'Order ID', dataIndex: 'id', copyable: true },
            { title: 'Customer ID', dataIndex: 'customerId', copyable: true },
            { title: 'Cart ID', dataIndex: 'cartId', copyable: true },
            {
              title: 'Status',
              dataIndex: 'status',
              render: (_, record) => <Tag color={statusColor(record.status)}>{record.status}</Tag>,
            },
            { title: 'Total', dataIndex: 'totalAmount', valueType: 'money' },
            { title: 'Payment ID', dataIndex: 'paymentId', copyable: true },
            { title: 'Created', dataIndex: 'createdAt', valueType: 'dateTime' },
            { title: 'Last updated', dataIndex: 'lastUpdatedAt', valueType: 'dateTime' },
          ]}
        />
      </Card>
      <Card title="Order lines" bordered={false}>
        <ProTable<OrderLine>
          rowKey="productId"
          search={false}
          options={false}
          pagination={false}
          dataSource={order.lines}
          columns={lineColumns}
        />
      </Card>
    </PageContainer>
  );
};

export default OrderDetailPage;
