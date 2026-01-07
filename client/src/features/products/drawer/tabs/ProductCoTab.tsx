import {
  CircularProgress,
  Stack,
  Typography
} from "@mui/material";
import { useConfirmedOrders } from "../../../../hooks/useConfirmedOrders";
import OrderLineLookupTable from "../tables/OrderLineLookupTable";
import { coStatusMap } from "../../../../types/confirmedOrders/coStatusMap";

interface ProductCoTabProps {
  productId: string | null;
}

export default function ProductCoTab({ productId }: ProductCoTabProps) {
  const { data, isLoading, isError } = useConfirmedOrders(productId);

  if (isLoading) return <CircularProgress />;
  if (isError) return <Typography color="error">Failed to load confirmed orders</Typography>;
  if (!data || data.length === 0)
    return <Typography>No confirmed orders for this product</Typography>;

  const rows = data.flatMap(co =>
    co.lineItems.map(li => ({
      orderNumber: co.orderNumber,
      status: coStatusMap[Number(co.status)],
      orderDate: new Date(co.orderDate).toLocaleDateString(),
      expectedDate: new Date(co.expectedDeliveryDate).toLocaleDateString(),
      receivedDate: new Date(co.receivedDate!).toLocaleDateString(),
      orderedQty: li.quantity,
      processedQty: li.soldQuantity,
      openQty: li.quantity - li.soldQuantity
    }))
  );

  return (
    <Stack spacing={2}>
      <Typography variant="h6">Committed Demand</Typography>

      <OrderLineLookupTable
        rows={rows}
        openQtyLabel="Open Demand"
      />
    </Stack>
  );
}
