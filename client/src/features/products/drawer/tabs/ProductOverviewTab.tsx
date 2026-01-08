import {
  Box,
  Typography,
  Stack,
  Chip,
  Grid,
  Paper,
  CircularProgress,
} from "@mui/material";
import SalesAndForecastChart from "../chart/SalesAndForecastChart";
import { type ProductDetail } from "../../productTypes"; 
import { useSalesAggregates } from "../../../../hooks/useSalesAggregates";
import { useForecasts } from "../../../../hooks/useForecasts";
import { useConfirmedOrderAggregates } from "../../../../hooks/useConfirmedOrderAggregates";
import { useState } from "react";
import Section from "../../../../components/general/Section";
import InfoRow from "../../../../components/general/InfoRow";

interface Props {
  product: ProductDetail;
}

export default function ProductOverviewTab({ product }: Props) {
  const [periodType, setPeriodType] =
    useState<"Daily" | "Weekly" | "Monthly">("Monthly");

  const { data: sales, isLoading: isLoadingSales } =
    useSalesAggregates(periodType, product.id);

  const { data: forecast, isLoading: isLoadingForecast } =
    useForecasts(periodType, product.id);

  const { data: confirmedOrders, isLoading: isLoadingCO } =
    useConfirmedOrderAggregates(periodType, product.id);

  const stock = product.stockLevel;
  const policy = product.inventoryPolicy;

  return (
    <Stack spacing={3}>
      {/* --- Product Header --- */}
      <Box>
        <Typography variant="h5" fontWeight="bold">
          {product.name}
        </Typography>
        <Stack direction="row" spacing={2} alignItems="center">
          <Typography color="text.secondary">
            {product.sku || "No SKU"}
          </Typography>
          <Chip
            label={product.isActive ? "Active" : "Inactive"}
            color={product.isActive ? "success" : "default"}
            size="small"
            variant="outlined"
          />
        </Stack>
        <Typography mt={1}>
          Price: <strong>${product.price.toFixed(2)}</strong>
        </Typography>
        {product.description && (
          <Typography mt={1} color="text.secondary">
            {product.description}
          </Typography>
        )}
      </Box>

      {/* --- Key Stock Signals --- */}
      <Grid container spacing={2}>
        <Grid sx={{xs:4}}>
          <Paper sx={{ p: 2 }}>
            <Typography color="text.secondary">Forecast Quantity</Typography>
            <Typography variant="h6">
              {stock.forecastQuantity}
            </Typography>
          </Paper>
        </Grid>
        <Grid sx={{xs:4}}>
          <Paper sx={{ p: 2 }}>
            <Typography color="text.secondary">
              Projected stock @ Lead Time
            </Typography>
            <Typography variant="h6">
              {stock.netStockPosition}
            </Typography>
          </Paper>
        </Grid>
        <Grid sx={{xs:4}}>
          <Paper sx={{ p: 2 }}>
            <Typography color="text.secondary">Reorder</Typography>
            <Chip
              label={stock.needsReorder ? "Yes" : "No"}
              color={stock.needsReorder ? "error" : "success"}
              variant="outlined"
            />
          </Paper>
        </Grid>
      </Grid>

      {/* --- Sales & Forecast Chart --- */}
      <Section title="Sales & Forecast">
        {(isLoadingSales || isLoadingForecast || isLoadingCO) && (
          <CircularProgress size={24} />
        )}
        {sales && forecast && confirmedOrders && (
          <SalesAndForecastChart
            sales={sales}
            forecast={forecast}
            confirmedOrder={confirmedOrders}
            periodType={periodType}
            onPeriodTypeChange={setPeriodType}
          />
        )}
      </Section>

      {/* --- Stock Levels --- */}
      <Section title="Stock Levels">
        <Paper sx={{ p: 2 }}>
          <Stack spacing={1}>
            <InfoRow
              label="Quantity on hand"
              value={stock.quantityOnHand}
            />
            <InfoRow
              label="Quantity on order"
              value={stock.quantityOnOrder}
            />
            <InfoRow
              label="Quantity committed"
              value={stock.quantityCommitted}
            />
            <InfoRow
              label="Reorder point"
              value={stock.reorderPoint}
            />
            <InfoRow
              label="Recommended order qty"
              value={stock.recommendedOrderQuantity}
            />
          </Stack>
        </Paper>
      </Section>

      {/* --- Inventory Policy --- */}
      <Section title="Inventory Policy">
        <Paper sx={{ p: 2 }}>
          <Stack spacing={1}>
            <InfoRow
              label="Lead time (days)"
              value={policy.leadTimeDays}
            />
            <InfoRow
              label="Safety stock"
              value={policy.safetyStock}
            />
            <InfoRow
              label="MOQ"
              value={policy.minimumOrderQuantity}
            />
            <InfoRow
              label="Order increment"
              value={policy.orderIncrementQuantity}
            />
            <InfoRow
              label="Stock adjustment"
              value={policy.stockAdjustment}
            />
          </Stack>
        </Paper>
      </Section>
    </Stack>
  );
}
