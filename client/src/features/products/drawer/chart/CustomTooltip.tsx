import type { Payload } from "recharts/types/component/DefaultTooltipContent";

function getPeriodKind(
  periodDate: Date,
  periodType: "Daily" | "Weekly" | "Monthly"
): "Past" | "Current" | "Future" {
  const now = new Date();

  const sameDay = (a: Date, b: Date) =>
    a.getFullYear() === b.getFullYear() &&
    a.getMonth() === b.getMonth() &&
    a.getDate() === b.getDate();

  const startOfWeek = (d: Date) => {
    const date = new Date(d);
    const day = date.getDay() || 7;
    date.setDate(date.getDate() - day + 1);
    date.setHours(0, 0, 0, 0);
    return date;
  };

  const sameWeek = (a: Date, b: Date) =>
    startOfWeek(a).getTime() === startOfWeek(b).getTime();

  const sameMonth = (a: Date, b: Date) =>
    a.getFullYear() === b.getFullYear() &&
    a.getMonth() === b.getMonth();

  let isCurrent = false;

  if (periodType === "Daily") {
    isCurrent = sameDay(periodDate, now);
  } else if (periodType === "Weekly") {
    isCurrent = sameWeek(periodDate, now);
  } else {
    isCurrent = sameMonth(periodDate, now);
  }

  if (isCurrent) return "Current";
  if (periodDate < now) return "Past";
  return "Future";
}

interface CustomTooltipProps {
  active?: boolean;
  payload?: Payload<number, string>[];
  label?: string;
  metricType: "Quantity" | "Revenue";
  periodType: "Daily" | "Weekly" | "Monthly";
}

export default function CustomTooltip({
  active,
  payload,
  label,
  metricType,
  periodType,
}: CustomTooltipProps) {
  if (!active || !payload || payload.length === 0) return null;

  const row = payload[0].payload as any;
  const periodDate = new Date(row.date);

  const periodKind = getPeriodKind(periodDate, periodType);

  const formatValue = (value: number) =>
    metricType === "Quantity"
      ? `${value} units`
      : `$${value.toFixed(2)}`;

  return (
    <div
      style={{
        backgroundColor: "#424242",
        padding: "8px 12px",
        borderRadius: "6px",
        color: "#fff",
      }}
    >
      <div style={{ fontWeight: 600, marginBottom: 4 }}>
        Period: {label}
      </div>

       {periodKind === "Past" && (
       <div>
           Sales:{" "}
           {formatValue(
           metricType === "Quantity"
               ? row.salesQuantity
               : row.salesRevenue
           )}
       </div>
       )}

       {periodKind === "Current" && (
       <>
           <div>
           Sales:{" "}
           {formatValue(
               metricType === "Quantity"
               ? row.salesQuantity
               : row.salesRevenue
           )}
           </div>

           <div>
           Confirmed demand:{" "}
           {formatValue(
               metricType === "Quantity"
               ? row.confirmedQuantity
               : row.confirmedRevenue
           )}
           </div>

           <div>
           Remaining forecast:{" "}
           {formatValue(
               metricType === "Quantity"
               ? row.remainingForecastQuantity
               : row.remainingForecastRevenue
           )}
           </div>
       </>
       )}

       {periodKind === "Future" && (
       <>
           <div>
           Confirmed demand:{" "}
           {formatValue(
               metricType === "Quantity"
               ? row.confirmedQuantity
               : row.confirmedRevenue
           )}
           </div>

           <div>
           Remaining forecast:{" "}
           {formatValue(
               metricType === "Quantity"
               ? row.remainingForecastQuantity
               : row.remainingForecastRevenue
           )}
           </div>
       </>
       )}
    </div>
  );
}
