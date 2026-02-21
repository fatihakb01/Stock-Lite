import { Table, TableBody, TableCell, TableHead, TableRow } from "@mui/material";
import type { UserDefinedField } from "../../../../types/userDefinedFields/userDefinedFieldTypes";

export default function UdfTable({ rows }: { rows: UserDefinedField[] }) {
    return (
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Key</TableCell>
            <TableCell>Value</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.map((row, i) => (
            <TableRow key={i}>
              <TableCell>{row.key}</TableCell>
              <TableCell>{row.value}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    )
}