import {
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody
} from "@mui/material";

interface Props {
  rows: any[];
  openQtyLabel: string;
}

export default function OrderLineLookupTable({ rows, openQtyLabel }: Props) {
  return (
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell>Order</TableCell>
          <TableCell>Status</TableCell>
          <TableCell>Order Date</TableCell>
          <TableCell>Expected Date</TableCell>
          <TableCell>Received Date</TableCell>
          <TableCell align="right">Ordered</TableCell>
          <TableCell align="right">Processed</TableCell>
          <TableCell align="right">{openQtyLabel}</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {rows.map((row, i) => (
          <TableRow key={i}>
            <TableCell>{row.orderNumber}</TableCell>
            <TableCell>{row.status}</TableCell>
            <TableCell>{row.orderDate ?? "-"}</TableCell>
            <TableCell>{row.expectedDate ?? "-"}</TableCell>
            <TableCell>{row.receivedDate ?? "-"}</TableCell>
            <TableCell align="right">{row.orderedQty}</TableCell>
            <TableCell align="right">{row.processedQty}</TableCell>
            <TableCell align="right">{row.openQty}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
