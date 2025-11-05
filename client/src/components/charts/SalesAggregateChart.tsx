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
import { FormControl, InputLabel, MenuItem, Select, Box, Stack, Typography } from "@mui/material";
import type { SalesAggregate } from "../../features/salesaggregates/salesaggregateTypes";

interface SalesAggregateChartProps {
  data: SalesAggregate[];
  periodType: "Daily" | "Weekly" | "Monthly";
  onPeriodTypeChange: (periodType: "Daily" | "Weekly" | "Monthly") => void;
}

export default function SalesAggregateChart({
  data,
  periodType,
  onPeriodTypeChange,
}: SalesAggregateChartProps) {
  const [metricType, setMetricType] = useState<"Quantity" | "Revenue">("Quantity");

  // Filter the number of bars shown based on the selected period
  const filteredData = useMemo(() => {
    let count = 30; // Default (Daily)
    if (periodType === "Weekly") count = 13;
    else if (periodType === "Monthly") count = 12;

    return data.slice(-count).map((d) => {
      const start = new Date(d.periodStartDate);

      // Define label depending on periodType
      let label: string;
      if (periodType === "Daily") {
        label = start.toLocaleDateString("en-US", { month: "short", day: "numeric" });
      } else if (periodType === "Weekly") {
        const end = new Date(start);
        end.setDate(start.getDate() + 6); // end of week
        label = `${start.toLocaleDateString("en-US", {
          month: "short",
          day: "numeric",
        })}–${end.toLocaleDateString("en-US", {
          month: end.getMonth() === start.getMonth() ? undefined : "short",
          day: "numeric",
        })}`;
      } else {
        label = start.toLocaleDateString("en-US", { month: "short" });
      }

      return { ...d, label };
    });

  }, [data, periodType]);

  // Select which key and color to display
  const dataKey = metricType === "Quantity" ? "totalQuantity" : "totalRevenue";
  const barColor = metricType === "Quantity" ? "#1976d2" : "#2e7d32";

  return (
    <Box>
        {/* Header Row with Title + Selectors */}
        <Stack
            direction="row"
            alignItems="center"
            justifyContent="space-between"
            mb={2}
            flexWrap="wrap"
            gap={2}
        >
            <Typography variant="body2" color="text.secondary">
            Historical sales & predicted demand
            </Typography>

            <Stack direction="row" spacing={2}>
                {/* Period Type Selector */}
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

        {/* Chart */}
        <ResponsiveContainer width="100%" height={300}>
            <BarChart
            data={filteredData}
            margin={{ top: 16, right: 16, left: 0, bottom: 0 }}
            >
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="label" />
            <YAxis />
            <Tooltip
                cursor={false}
                formatter={(value, name) => {
                if (name === "Quantity") return [`${value} units`, "Quantity"];
                if (name === "Revenue") return [`$${value}`, "Revenue"];
                return [value, name];
                }}
            />
            <Legend />
            <Bar
                dataKey={dataKey}
                fill={barColor}
                name={metricType}
                activeBar={false}
            />
            </BarChart>
        </ResponsiveContainer>
    </Box>
  );
}

