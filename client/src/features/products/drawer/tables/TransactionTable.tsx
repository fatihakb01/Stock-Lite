import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
} from "@mui/material";

export default function TransactionTable({ rows }: { rows: any[] }) {
  return (
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell>Transaction Id</TableCell>
          <TableCell>Date</TableCell>
          <TableCell align="right">Quantity</TableCell>
          <TableCell align="right">Unit Price</TableCell>
          <TableCell align="right">Total</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {rows.map((r, i) => (
          <TableRow key={i}>
            <TableCell>{r.transactionId}</TableCell>
            <TableCell>{r.date}</TableCell>
            <TableCell align="right">{r.quantity}</TableCell>
            <TableCell align="right">${r.unitPrice.toFixed(2)}</TableCell>
            <TableCell align="right">${r.total.toFixed(2)}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
