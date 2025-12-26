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

import type { SalesAggregate } from "../../features/salesaggregates/salesaggregateTypes";
import type { Forecast } from "../../features/forecasts/forecastTypes";

interface SalesAndForecastChartProps {
  sales: SalesAggregate[];
  forecast: Forecast[];
  periodType: "Daily" | "Weekly" | "Monthly";
  onPeriodTypeChange: (periodType: "Daily" | "Weekly" | "Monthly") => void;
}

export default function SalesAndForecastChart({
  sales,
  forecast,
  periodType,
  onPeriodTypeChange,
}: SalesAndForecastChartProps) {
  const [metricType, setMetricType] = useState<"Quantity" | "Revenue">("Quantity");

  //
  // STEP 1 — MERGE SALES + FORECAST INTO SAME PERIOD BUCKET
  //
  const combinedData = useMemo(() => {
    const map = new Map<Date, any>();

    // Add sales
    for (const s of sales) {
      const key = s.periodStartDate;

      map.set(key, {
        date: new Date(s.periodStartDate),
        salesQuantity: s.totalQuantity,
        salesRevenue: s.totalRevenue,
        forecastQuantity: 0,
        forecastRevenue: 0,
      });
    }

    // Add forecast
    for (const f of forecast) {
      const key = f.periodStartDate;

      if (!map.has(key)) {
        map.set(key, {
          date: new Date(f.periodStartDate),
          salesQuantity: 0,
          salesRevenue: 0,
          forecastQuantity: f.predictedQuantity,
          forecastRevenue: f.predictedRevenue,
        });
      } else {
        const row = map.get(key);
        row.forecastQuantity = f.predictedQuantity;
        row.forecastRevenue = f.predictedRevenue;
      }
    }

    return Array.from(map.values()).sort(
      (a, b) => a.date.getTime() - b.date.getTime()
    );
  }, [sales, forecast]);

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
  const forecastKey = metricType === "Quantity" ? "forecastQuantity" : "forecastRevenue";
  const barSize = periodType === "Daily" ? 5 : periodType === "Weekly" ? 13 : 15;

  //
  // STEP 4 — Custom tooltip
  //
  const tooltipFormatter = (value: number, name: string) => {
    if (metricType === "Quantity") return [`${value} units`, name];
    return [`$${value.toFixed(2)}`, name];
  };

  const tooltipLabelFormatter = (label: string) => {
    return `Period: ${label}`;
  };

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

          <Tooltip
            formatter={tooltipFormatter}
            labelFormatter={tooltipLabelFormatter}
            contentStyle={{
              backgroundColor: "#424242",   
              border: "none",
              borderRadius: "6px",
            }}
            labelStyle={{
              color: "#ffffff",             
            }}
            itemStyle={{
              color: "#ffffff",             
            }}
          />

          <Legend />

          {/* SALES BAR (Yellow, transparent) */}
          <Bar
            dataKey={salesKey}
            fill="rgba(255, 215, 0, 0.6)"
            name="Sales"
            stroke="rgba(255, 215, 0, 1)"
            stackId="a"
            barSize={barSize}       
          />

          {/* FORECAST BAR (Blue, solid) */}
          <Bar
            dataKey={forecastKey}
            fill="#1976d2"
            name="Forecast"
            stroke="#1976d2"
            stackId="a"
            barSize={barSize}
          />
        </BarChart>
      </ResponsiveContainer>
    </Box>
  );
}
