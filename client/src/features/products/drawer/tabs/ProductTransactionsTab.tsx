import {
  CircularProgress,
  Stack,
  Typography,
  Paper,
} from "@mui/material";
import { useTransactions } from "../../../../hooks/useTransactions";
import TransactionTable from "../tables/TransactionTable";

interface ProductTransactionsTabProps {
  productId: string | null;
}

export default function ProductTransactionsTab({ productId }: ProductTransactionsTabProps) {
  const { data, isLoading, isError } = useTransactions(productId);

  if (isLoading) return <CircularProgress />;
  if (isError) return <Typography color="error">Failed to load transactions</Typography>;
  if (!data || data.length === 0)
    return <Typography>No transactions for this product</Typography>;

  const rows = data.flatMap(t =>
    t.lineItems.map(li => ({
      transactionId: t.transactionId,
      date: new Date(t.transactionDate).toLocaleDateString(),
      quantity: li.quantity,
      unitPrice: li.unitPrice,
      total: li.quantity * li.unitPrice
    }))
  );

  const totalSold = rows.reduce((sum, r) => sum + r.quantity, 0);

  return (
    <Stack spacing={2}>
      {/* KPIs */}
      <Paper sx={{ p: 2 }}>
        <Stack direction="row" spacing={4}>
          <Typography>
            <strong>Total sold:</strong> {totalSold}
          </Typography>
          <Typography>
            <strong>Last transaction:</strong>{" "}
            {rows[0]?.date ?? "-"}
          </Typography>
        </Stack>
      </Paper>

      {/* Table */}
      <TransactionTable rows={rows} />
    </Stack>
  );
}
