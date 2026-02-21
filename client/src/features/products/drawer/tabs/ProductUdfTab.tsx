import { Stack, Typography } from "@mui/material";
import { useUserDefinedFields } from "../../../../hooks/useUserDefinedFields";
import UdfTable from "../tables/UdfTable";

interface ProductUdfTabProps {
  productId: string | null;
}

export default function ProductUdfTab({ productId } : ProductUdfTabProps) {
  debugger;
  const {data} = useUserDefinedFields(productId);

  const rows = data && data.map(r => ({
      key: r.key,
      value: r.value || ""
    })
  );

  return (
    <Stack spacing={2}>
      <Typography variant="h6">Additional Information</Typography>
      <UdfTable rows={rows ?? []} />
    </Stack>
  )
}