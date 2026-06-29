import { Column } from '@ant-design/plots';
import type { ProColumns } from '@ant-design/pro-components';
import { PageContainer, ProTable } from '@ant-design/pro-components';
import { useQuery } from '@tanstack/react-query';
import { Card, Col, DatePicker, Row, Segmented, Spin, Statistic, Typography } from 'antd';
import dayjs, { type Dayjs } from 'dayjs';
import React, { useMemo, useState } from 'react';
import type { ReportPeriod, TopBuyerItem, TopProductItem } from './data.d';
import { queryTopBuyers, queryTopProducts } from './service';

const periodOptions: { label: string; value: ReportPeriod }[] = [
  { label: 'Day', value: 'day' },
  { label: 'Week', value: 'week' },
  { label: 'Month', value: 'month' },
  { label: 'Year', value: 'year' },
];

const formatMoney = (value: number) =>
  value.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });

const FinanceStatisticsPage: React.FC = () => {
  const [period, setPeriod] = useState<ReportPeriod>('week');
  const [referenceDate, setReferenceDate] = useState<Dayjs | null>(dayjs());
  const dateParam = referenceDate?.startOf('day').toISOString();

  const buyersQuery = useQuery({
    queryKey: ['finance-top-buyers', period, dateParam],
    queryFn: () => queryTopBuyers(period, 10, dateParam),
  });

  const productsQuery = useQuery({
    queryKey: ['finance-top-products', period, dateParam],
    queryFn: () => queryTopProducts(period, 10, dateParam),
  });

  const buyers = buyersQuery.data ?? [];
  const products = productsQuery.data ?? [];
  const loading = buyersQuery.isLoading || productsQuery.isLoading;

  const totalRevenue = useMemo(
    () => buyers.reduce((sum, item) => sum + item.totalOrderAmount, 0),
    [buyers],
  );

  const totalProductSales = useMemo(
    () => products.reduce((sum, item) => sum + item.totalSalesAmount, 0),
    [products],
  );

  const periodLabel = periodOptions.find((p) => p.value === period)?.label ?? period;
  const periodRange =
    buyers[0]?.periodStartUtc && buyers[0]?.periodEndUtc
      ? `${dayjs(buyers[0].periodStartUtc).format('YYYY-MM-DD')} → ${dayjs(buyers[0].periodEndUtc).format('YYYY-MM-DD')}`
      : '—';

  const buyerChartData = buyers.map((item) => ({
    name: item.displayName || item.email,
    value: item.totalOrderAmount,
  }));

  const productChartData = products.map((item) => ({
    name: item.productName,
    value: item.totalSalesAmount,
  }));

  const buyerColumns: ProColumns<TopBuyerItem>[] = [
    { title: 'Buyer', dataIndex: 'displayName', render: (_, r) => r.displayName || r.email },
    { title: 'Email', dataIndex: 'email', copyable: true },
    {
      title: 'Total spent',
      dataIndex: 'totalOrderAmount',
      render: (_, r) => formatMoney(r.totalOrderAmount),
    },
    { title: 'Orders', dataIndex: 'orderCount' },
  ];

  const productColumns: ProColumns<TopProductItem>[] = [
    { title: 'Product', dataIndex: 'productName' },
    { title: 'Qty sold', dataIndex: 'totalQuantity' },
    {
      title: 'Sales',
      dataIndex: 'totalSalesAmount',
      render: (_, r) => formatMoney(r.totalSalesAmount),
    },
    { title: 'Orders', dataIndex: 'orderCount' },
  ];

  return (
    <PageContainer
      title="Finance statistics"
      subTitle={`Top buyers & products by ${periodLabel.toLowerCase()} · ${periodRange}`}
      extra={
        <div style={{ display: 'flex', gap: 12, alignItems: 'center', flexWrap: 'wrap' }}>
          <Segmented
            options={periodOptions}
            value={period}
            onChange={(value) => setPeriod(value as ReportPeriod)}
          />
          <DatePicker
            value={referenceDate}
            onChange={(value) => setReferenceDate(value)}
            allowClear={false}
          />
        </div>
      }
    >
      <Spin spinning={loading}>
        <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
          <Col xs={24} sm={12}>
            <Card>
              <Statistic title="Top buyers revenue (period)" value={totalRevenue} precision={2} prefix="$" />
            </Card>
          </Col>
          <Col xs={24} sm={12}>
            <Card>
              <Statistic title="Top products sales (period)" value={totalProductSales} precision={2} prefix="$" />
            </Card>
          </Col>
        </Row>

        <Row gutter={[16, 16]}>
          <Col xs={24} lg={12}>
            <Card title="Top buyers" bordered={false}>
              {buyerChartData.length > 0 ? (
                <Column
                  data={buyerChartData}
                  xField="name"
                  yField="value"
                  height={280}
                  axis={{ x: { labelAutoRotate: true } }}
                  tooltip={{ channel: 'y', valueFormatter: (v) => `$${formatMoney(Number(v))}` }}
                />
              ) : (
                <Typography.Text type="secondary">No buyer data for this period.</Typography.Text>
              )}
              <ProTable<TopBuyerItem>
                style={{ marginTop: 16 }}
                rowKey="userId"
                search={false}
                options={false}
                pagination={false}
                dataSource={buyers}
                columns={buyerColumns}
              />
            </Card>
          </Col>
          <Col xs={24} lg={12}>
            <Card title="Top selling products" bordered={false}>
              {productChartData.length > 0 ? (
                <Column
                  data={productChartData}
                  xField="name"
                  yField="value"
                  height={280}
                  axis={{ x: { labelAutoRotate: true } }}
                  tooltip={{ channel: 'y', valueFormatter: (v) => `$${formatMoney(Number(v))}` }}
                />
              ) : (
                <Typography.Text type="secondary">No product sales data for this period.</Typography.Text>
              )}
              <ProTable<TopProductItem>
                style={{ marginTop: 16 }}
                rowKey="productId"
                search={false}
                options={false}
                pagination={false}
                dataSource={products}
                columns={productColumns}
              />
            </Card>
          </Col>
        </Row>
      </Spin>
    </PageContainer>
  );
};

export default FinanceStatisticsPage;
