import { PageContainer, ProDescriptions } from '@ant-design/pro-components';
import { FormattedMessage, history, useParams } from '@umijs/max';
import { useQuery } from '@tanstack/react-query';
import { Button, Card, Result, Spin } from 'antd';
import React, { useState } from 'react';
import ProductAuditTab from '../components/ProductAuditTab';
import { getProductById } from '../service';

const ProductDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [activeTab, setActiveTab] = useState('details');

  const { data: product, isLoading, isError, refetch } = useQuery({
    queryKey: ['admin-product', id],
    queryFn: () => getProductById(id!),
    enabled: Boolean(id),
  });

  if (!id) {
    return (
      <PageContainer>
        <Result status="404" title="Product not found" />
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

  if (isError || !product) {
    return (
      <PageContainer>
        <Result
          status="error"
          title="Failed to load product"
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
    <PageContainer
      title={product.name}
      onBack={() => history.push('/catalog/products')}
      tabActiveKey={activeTab}
      onTabChange={setActiveTab}
      tabList={[
        { key: 'details', tab: <FormattedMessage id="pages.products.detail" defaultMessage="Product detail" /> },
        { key: 'audit', tab: <FormattedMessage id="pages.products.auditTab" defaultMessage="Audit history" /> },
      ]}
    >
      {activeTab === 'audit' ? (
        <Card bordered={false}>
          <ProductAuditTab productId={product.id} />
        </Card>
      ) : (
        <Card bordered={false}>
          <ProDescriptions
            column={2}
            title="Product information"
            dataSource={product}
            columns={[
              { title: 'ID', dataIndex: 'id', copyable: true },
              { title: 'Name', dataIndex: 'name' },
              {
                title: 'Price',
                dataIndex: 'price',
                valueType: 'money',
              },
              {
                title: 'Created',
                dataIndex: 'createdAt',
                valueType: 'dateTime',
              },
              {
                title: 'Last updated',
                dataIndex: 'lastUpdatedAt',
                valueType: 'dateTime',
              },
            ]}
          />
        </Card>
      )}
    </PageContainer>
  );
};

export default ProductDetailPage;
