import {
  Drawer,
  Box,
  Typography,
  Divider,
  Chip,
  Stack,
  Button,
  CircularProgress,
} from "@mui/material";
import { useProduct } from "../../hooks/useProducts";
import { useSalesAggregates } from "../../hooks/useSalesAggregates";
import SalesAndForecastChart from "../charts/SalesAndForecastChart"
import { useState } from "react";
import { useForecasts } from "../../hooks/useForecasts";
import { useConfirmedOrderAggregates } from "../../hooks/useConfirmedOrderAggregates";

// Small reusable section wrapper for consistent layout
function Section({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <Box>
      <Typography
        variant="h6"
        sx={{ mb: 1, borderBottom: "1px solid rgba(0,0,0,0.1)", pb: 0.5 }}
      >
        {title}
      </Typography>
      {children}
    </Box>
  );
}

interface ProductDetailDrawerProps {
  productId: string | null;
  onClose: () => void;
}

export default function ProductDetailDrawer({
  productId,
  onClose,
}: ProductDetailDrawerProps) {
  const [periodType, setPeriodType] = useState<"Daily" | "Weekly" | "Monthly">("Monthly");

  const { 
    data: product, 
    isLoading: isLoadingProduct, 
    isError: isErrorProduct 
  } = useProduct(productId);

  const {
    data: salesaggregate,
    isLoading: isLoadingSales,
    isError: isErrorSales,
  } = useSalesAggregates(periodType, productId);

  const {
    data: forecast,
    isLoading: isLoadingForecasts,
    isError: isErrorForecasts,
  } = useForecasts(periodType, productId);

  const {
    data: confirmedOrderAggregate,
    isLoading: isLoadingCoAggregate,
    isError: isErrorCoAggregate,
  } = useConfirmedOrderAggregates(periodType, productId);

  return (
    <Drawer
      variant="temporary"
      anchor="right"
      open={!!productId}
      onClose={onClose}
      sx={{
        width: "40%",
        flexShrink: 0,
        "& .MuiDrawer-paper": {
          width: "40%",
          p: 3,
          boxSizing: "border-box",
        },
      }}
    >
      <Box
        sx={{
          display: "flex",
          flexDirection: "column",
          height: "100%",
          overflowY: "auto",
          gap: 3,
        }}
      >
        {/* Loading State */}
        {isLoadingProduct && (
          <Stack alignItems="center" justifyContent="center" height="100%">
            <CircularProgress />
            <Typography variant="body2" mt={2}>
              Loading product details...
            </Typography>
          </Stack>
        )}

        {/* Error State */}
        {isErrorProduct && (
          <Typography color="error">Failed to load product details.</Typography>
        )}

        {/* Product Details */}
        {product && (
          <>
            {/* --- General Section --- */}
            <Section title="General">
              <Typography variant="h5" fontWeight="bold">
                {product.name}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {product.sku ? `SKU: ${product.sku}` : "No SKU"}
              </Typography>
              <Typography variant="body1" mt={1}>
                Price: ${product.price.toFixed(2)}
              </Typography>
              <Typography variant="body1" mt={1}>
                {product.description || "No description provided"}
              </Typography>
            </Section>

            {/* --- Forecast Section --- */}
            <Section title="Forecast">
              {(isLoadingSales || isLoadingForecasts || isLoadingCoAggregate) && <Typography>Loading chart...</Typography>}
              {isErrorSales && <Typography color="error">Failed to load sales data.</Typography>}
              {isErrorForecasts && <Typography color="error">Failed to load forecast data.</Typography>}
              {isErrorCoAggregate && <Typography color="error">Failed to load confirmed order data.</Typography>}
              {(
                salesaggregate && salesaggregate.length > 0 && 
                forecast && forecast.length > 0 &&
                confirmedOrderAggregate && confirmedOrderAggregate.length > 0) ? 
              (
                <SalesAndForecastChart
                  sales={salesaggregate}
                  forecast={forecast}
                  confirmedOrder={confirmedOrderAggregate}
                  periodType={periodType}
                  onPeriodTypeChange={setPeriodType}
                />
              ) : (
                !isLoadingSales && <Typography>No sales data available.</Typography>
              )}
            </Section>

            {/* --- Stock Section --- */}
            {product.stock && (
              <Section title="Stock">
                <Typography variant="body1">
                  Quantity on hand: <strong>{product.stock.quantityOnHand}</strong>
                </Typography>
                <Typography variant="body1">
                  Quantity on order: <strong>{product.stock.quantityOnOrder}</strong>
                </Typography>
                <Typography variant="body1">
                  Reorder Point: <strong>{product.stock.reorderPoint}</strong>
                </Typography>
              </Section>
            )}

            {/* --- Status Section --- */}
            <Section title="Status">
              <Chip
                label={product.isActive ? "Active" : "Inactive"}
                color={product.isActive ? "success" : "default"}
                variant="outlined"
              />
            </Section>

            <Divider />

            {/* --- Actions --- */}
            <Stack direction="row" spacing={2}>
              <Button variant="outlined" color="primary" onClick={onClose}>
                Close
              </Button>
            </Stack>
          </>
        )}
      </Box>
    </Drawer>
  );
}
