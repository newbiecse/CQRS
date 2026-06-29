import type { ProColumns } from '@ant-design/pro-components';
import { ProTable } from '@ant-design/pro-components';
import { Tag, Typography } from 'antd';
import React from 'react';
import type { ProductAuditEntry } from '../data.d';
import { queryProductAudit } from '../service';

type ProductAuditTabProps = {
  productId: string;
};

const eventColor = (eventType: string) => {
  if (eventType.includes('created')) return 'green';
  if (eventType.includes('deleted')) return 'red';
  if (eventType.includes('price') || eventType.includes('updated')) return 'blue';
  return 'default';
};

const ProductAuditTab: React.FC<ProductAuditTabProps> = ({ productId }) => {
  const columns: ProColumns<ProductAuditEntry>[] = [
    {
      title: 'When',
      dataIndex: 'occurredAtUtc',
      valueType: 'dateTime',
      width: 180,
    },
    {
      title: 'Action',
      dataIndex: 'action',
      width: 120,
    },
    {
      title: 'Event',
      dataIndex: 'eventType',
      width: 200,
      render: (_, record) => (
        <Tag color={eventColor(record.eventType)}>{record.eventType}</Tag>
      ),
    },
    {
      title: 'Summary',
      dataIndex: 'summary',
      ellipsis: true,
    },
    {
      title: 'Service',
      dataIndex: 'service',
      width: 140,
      search: false,
    },
  ];

  return (
    <ProTable<ProductAuditEntry>
      rowKey="id"
      search={false}
      options={false}
      pagination={{ pageSize: 10 }}
      request={async () => queryProductAudit(productId)}
      columns={columns}
      expandable={{
        expandedRowRender: (record) => {
          let formatted = record.payloadJson;
          try {
            formatted = JSON.stringify(JSON.parse(record.payloadJson || '{}'), null, 2);
          } catch {
            formatted = record.payloadJson;
          }
          return (
            <Typography.Paragraph>
              <pre style={{ margin: 0, whiteSpace: 'pre-wrap', fontSize: 12 }}>
                {formatted}
              </pre>
            </Typography.Paragraph>
          );
        },
        rowExpandable: (record) => Boolean(record.payloadJson),
      }}
    />
  );
};

export default ProductAuditTab;
