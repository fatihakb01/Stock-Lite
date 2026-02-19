import { useMemo, useState } from "react";
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  CartesianGrid,
  Legend,
} from "recharts";
import {
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Box,
  Stack,
  Typography
} from "@mui/material";

import type { SalesAggregate } from "../../../../types/salesAggregates/salesaggregateTypes";
import type { Forecast } from "../../../../types/forecasts/forecastTypes";
import type { ConfirmedOrderAggregate } from "../../../../types/confirmedOrderAggregates/confirmedOrderAggregateTypes";
import CustomTooltip from "./CustomTooltip";


interface SalesAndForecastChartProps {
  sales: SalesAggregate[];
  forecast: Forecast[];
  confirmedOrder: ConfirmedOrderAggregate[],
  periodType: "Daily" | "Weekly" | "Monthly";
  onPeriodTypeChange: (periodType: "Daily" | "Weekly" | "Monthly") => void;
}

export default function SalesAndForecastChart({
  sales,
  forecast,
  confirmedOrder,
  periodType,
  onPeriodTypeChange,
}: SalesAndForecastChartProps) {
  const [metricType, setMetricType] = useState<"Quantity" | "Revenue">("Quantity");

  //
  // STEP 1 — MERGE DATA INTO SAME PERIOD BUCKET
  //
  const combinedData = useMemo(() => {
    const map = new Map<number, any>();

    const getKey = (date: string | Date) =>
      new Date(date).getTime();

    // Sales
    if (sales && sales.length > 0) {
      for (const s of sales) {
        const key = getKey(s.periodStartDate);
        map.set(key, {
          date: new Date(s.periodStartDate),
          salesQuantity: s.totalQuantity,
          salesRevenue: s.totalRevenue,
          forecastQuantity: 0,
          forecastRevenue: 0,
          confirmedQuantity: 0,
          confirmedRevenue: 0,
        });
      }
    }

    // Forecast
    if (forecast && forecast.length > 0) {
      for (const f of forecast) {
        const key = getKey(f.periodStartDate);
        const row = map.get(key) ?? {
          date: new Date(f.periodStartDate),
          salesQuantity: 0,
          salesRevenue: 0,
          confirmedQuantity: 0,
          confirmedRevenue: 0,
        };
      

        row.forecastQuantity = f.predictedQuantity;
        row.forecastRevenue = f.predictedRevenue;

        map.set(key, row);
      }
    }

    // Confirmed
    if (confirmedOrder && confirmedOrder.length > 0) {
      for (const c of confirmedOrder) {
        const key = getKey(c.periodStartDate);
        const row = map.get(key) ?? {
          date: new Date(c.periodStartDate),
          salesQuantity: 0,
          salesRevenue: 0,
          forecastQuantity: 0,
          forecastRevenue: 0,
        };

        row.confirmedQuantity = c.totalConfirmedQuantity;
        row.confirmedRevenue = c.totalConfirmedRevenue;

        map.set(key, row);
      }
    }

    // DERIVED VALUES 
    return Array.from(map.values())
      .map((row) => {
        const remainingForecastQuantity = Math.max(
          0,
          (row.forecastQuantity ?? 0) - (row.confirmedQuantity ?? 0)
        );

        const remainingForecastRevenue = Math.max(
          0,
          (row.forecastRevenue ?? 0) - (row.confirmedRevenue ?? 0)
        );

        return {
          ...row,
          remainingForecastQuantity,
          remainingForecastRevenue,
        };
      })
      .sort((a, b) => a.date.getTime() - b.date.getTime());
  }, [sales, forecast, confirmedOrder]);


  //
  // STEP 2 — APPLY LABELS (Daily, Weekly, Monthly)
  //
  const filteredData = useMemo(() => {
    const labeled = combinedData.map((d) => {
      const start = new Date(d.date);
      let label: string;

      if (periodType === "Daily") {
        label = start.toLocaleDateString("en-US", {
          month: "short",
          day: "numeric",
        });
      } else if (periodType === "Weekly") {
        const end = new Date(start);
        end.setDate(start.getDate() + 6);

        label = `${start.toLocaleDateString("en-US", {
          month: "short",
          day: "numeric",
        })}–${end.toLocaleDateString("en-US", {
          month: end.getMonth() === start.getMonth() ? undefined : "short",
          day: "numeric",
        })}`;
      } else {
        label = start.toLocaleDateString("en-US", { month: "short", year: "numeric" });
      }

      return { ...d, label };
    });

    // LIMIT NUMBER OF PERIODS SHOWN BASED ON periodType
    let limit = 30;
    if (periodType === "Weekly") limit = 13;
    else if (periodType === "Monthly") limit = 12;

    return labeled.slice(-limit * 2);

  }, [combinedData, periodType]);

  //
  // STEP 3 — Choose correct metric fields
  //
  const salesKey = metricType === "Quantity" ? "salesQuantity" : "salesRevenue";
  const confirmedKey = metricType === "Quantity" ? "confirmedQuantity" : "confirmedRevenue";
  const remainingForecastKey = metricType === "Quantity" ? "remainingForecastQuantity" : "remainingForecastRevenue";
  const barSize = periodType === "Daily" ? 5 : periodType === "Weekly" ? 13 : 15;

  return (
    <Box>
      {/* Header Row */}
      <Stack
        direction="row"
        alignItems="center"
        justifyContent="space-between"
        mb={2}
        flexWrap="wrap"
        gap={2}
      >
        <Typography variant="body2" color="text.secondary">
          Historical sales & forecast
        </Typography>

        <Stack direction="row" spacing={2}>
          {/* Period Selector */}
          <FormControl size="small" sx={{ mb: 2, minWidth: 160 }}>
            <InputLabel id="period-type-label">Period</InputLabel>
            <Select
              labelId="period-type-label"
              value={periodType}
              label="Period"
              onChange={(e) => onPeriodTypeChange(e.target.value as any)}
            >
              <MenuItem value="Daily">Daily</MenuItem>
              <MenuItem value="Weekly">Weekly</MenuItem>
              <MenuItem value="Monthly">Monthly</MenuItem>
            </Select>
          </FormControl>

          {/* Metric Selector */}
          <FormControl size="small" sx={{ minWidth: 140 }}>
            <InputLabel id="metric-type-label">Metric</InputLabel>
            <Select
              labelId="metric-type-label"
              value={metricType}
              label="Metric"
              onChange={(e) => setMetricType(e.target.value as any)}
            >
              <MenuItem value="Quantity">Quantity</MenuItem>
              <MenuItem value="Revenue">Revenue</MenuItem>
            </Select>
          </FormControl>
        </Stack>
      </Stack>

      {/* CHART */}
      <ResponsiveContainer width="100%" height={300}>
        <BarChart
          data={filteredData}
          margin={{ top: 16, right: 16, left: 0, bottom: 0 }}
        >
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="label" />
          <YAxis />

          <Tooltip content={<CustomTooltip metricType={metricType} periodType={periodType} />} />

          <Legend />

          {/* SALES */}
          <Bar
            dataKey={salesKey}
            fill="rgba(255, 215, 0, 0.7)"
            stroke="rgba(255, 215, 0, 0.7)"
            name="Sales"
            stackId="a"
            barSize={barSize}
          />

          {/* CONFIRMED DEMAND */}
          <Bar
            dataKey={confirmedKey}
            fill="rgba(76, 175, 80, 0.9)"
            stroke="rgba(76, 175, 80, 0.9)"
            name="Confirmed demand"
            stackId="a"
            barSize={barSize}
          />

          {/* REMAINING FORECAST */}
          <Bar
            dataKey={remainingForecastKey}
            fill="rgba(25, 118, 210, 0.6)"
            stroke="rgba(25, 118, 210, 0.6)"
            name="Remaining forecast"
            stackId="a"
            barSize={barSize}
          />
        </BarChart>
      </ResponsiveContainer>
    </Box>
  );
}
