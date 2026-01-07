import {
  Stack,
  Typography,
  Paper,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  CircularProgress,
  Grid,
} from "@mui/material";
import { usePurchaseForecasts } from "../../../../hooks/usePurchaseForecasts";
import Section from "../../../../components/general/Section";

interface ProductPfcTabProps {
  productId: string | null;
}

export default function ProductPfcTab({ productId }: ProductPfcTabProps) {
  const {
    data: forecasts,
    isLoading,
    isError,
  } = usePurchaseForecasts(productId);

  if (isLoading) return <CircularProgress />;
  if (isError || !forecasts) {
    return <Typography color="error">Failed to load purchase forecast.</Typography>;
  }

  const firstReorder = forecasts.find(f => f.needsReorder);
  const totalRecommendedQty = forecasts.reduce(
    (sum, f) => sum + f.recommendedOrderQuantity,
    0
  );
  const minProjectedStock = Math.min(...forecasts.map(f => f.projectedStock));

  return (
    <Stack spacing={3}>
      {/* --- Summary Signals --- */}
      <Grid container spacing={2}>
        <Grid sx={{xs:3}}>
          <Paper sx={{ p: 2 }}>
            <Typography color="text.secondary">First Reorder Week</Typography>
            <Typography variant="h6">
              {firstReorder ? `Week ${firstReorder.weekIndex}` : "None"}
            </Typography>
          </Paper>
        </Grid>

        <Grid sx={{xs:3}}>
          <Paper sx={{ p: 2 }}>
            <Typography color="text.secondary">Total Recommended Qty</Typography>
            <Typography variant="h6">
              {totalRecommendedQty}
            </Typography>
          </Paper>
        </Grid>

        <Grid sx={{xs:3}}>
          <Paper sx={{ p: 2 }}>
            <Typography color="text.secondary">Lowest Projected Stock</Typography>
            <Typography
              variant="h6"
              color={minProjectedStock < 0 ? "error" : "text.primary"}
            >
              {minProjectedStock}
            </Typography>
          </Paper>
        </Grid>

        <Grid sx={{xs:3}}>
          <Paper sx={{ p: 2 }}>
            <Typography color="text.secondary">Forecast Horizon</Typography>
            <Typography variant="h6">
              {forecasts.length} weeks
            </Typography>
          </Paper>
        </Grid>
      </Grid>

      {/* --- Forecast Timeline --- */}
      <Section title="Purchase Forecast Timeline">
        <Paper sx={{ overflowX: "auto" }}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Week</TableCell>
                <TableCell align="right">Forecast</TableCell>
                <TableCell align="right">Committed</TableCell>
                <TableCell align="right">On Order</TableCell>
                <TableCell align="right">Projected Stock</TableCell>
                <TableCell align="center">Reorder</TableCell>
                <TableCell align="right">Recommended Qty</TableCell>
              </TableRow>
            </TableHead>

            <TableBody>
              {forecasts.map(f => (
                <TableRow key={f.weekIndex}>
                  <TableCell>
                    W{f.weekIndex}
                    <Typography variant="caption" display="block">
                      {new Date(f.weekStartDate).toLocaleDateString()} – {new Date(f.weekEndDate).toLocaleDateString()}
                    </Typography>
                  </TableCell>

                  <TableCell align="right">{f.forecastQuantity}</TableCell>
                  <TableCell align="right">{f.quantityCommitted}</TableCell>
                  <TableCell align="right">{f.quantityOnOrder}</TableCell>

                  <TableCell
                    align="right"
                    sx={{
                      color:
                        f.projectedStock < 0
                          ? "error.main"
                          : f.projectedStock < 10
                          ? "warning.main"
                          : "text.primary",
                    }}
                  >
                    {f.projectedStock}
                  </TableCell>

                  <TableCell align="center">
                    <Chip
                      label={f.needsReorder ? "Yes" : "No"}
                      color={f.needsReorder ? "error" : "success"}
                      size="small"
                      variant="outlined"
                    />
                  </TableCell>

                  <TableCell align="right">
                    {f.recommendedOrderQuantity > 0
                      ? f.recommendedOrderQuantity
                      : "—"}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Paper>
      </Section>
    </Stack>
  );
}
