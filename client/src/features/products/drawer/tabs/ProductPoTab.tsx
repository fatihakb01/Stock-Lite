import {
  CircularProgress,
  Stack,
  Typography
} from "@mui/material";
import { usePurchaseOrders } from "../../../../hooks/usePurchaseOrders";
import OrderLineLookupTable from "../tables/OrderLineLookupTable";

interface ProductPoTabProps {
  productId: string | null;
}

export default function ProductPoTab({ productId }: ProductPoTabProps) {
  const { data, isLoading, isError } = usePurchaseOrders(productId);

  if (isLoading) return <CircularProgress />;
  if (isError) return <Typography color="error">Failed to load purchase orders</Typography>;
  if (!data || data.length === 0)
    return <Typography>No purchase orders for this product</Typography>;

  const rows = data.flatMap(po =>
    po.lineItems.map(li => ({
      orderNumber: po.orderNumber,
      status: po.status,
      orderDate: new Date(po.orderDate).toLocaleDateString(),
      expectedDate: new Date(po.expectedDeliveryDate).toLocaleDateString(),
      receivedDate: po.receivedDate == null ? '-' : new Date(po.receivedDate).toLocaleDateString(),
      orderedQty: li.quantity,
      processedQty: li.receivedQuantity,
      openQty: li.quantity - li.receivedQuantity
    }))
  );

  return (
    <Stack spacing={2}>
      <Typography variant="h6">Incoming Supply</Typography>

      <OrderLineLookupTable
        rows={rows}
        openQtyLabel="Open Supply"
      />
    </Stack>
  );
}
