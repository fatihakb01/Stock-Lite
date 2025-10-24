import {
  Drawer,
  Box,
  Typography,
  Divider,
  Chip,
  Stack,
  Button,
  CircularProgress,
} from "@mui/material";
import { useProduct } from "../../../hooks/useProducts";

interface ProductDetailDrawerProps {
  productId: string | null;
  onClose: () => void;
}

export default function ProductDetailDrawer({ productId, onClose }: ProductDetailDrawerProps) {
  const { data: product, isLoading, isError } = useProduct(productId);

  return (
    <Drawer
      variant="temporary"
      anchor="right"
      open={!!productId}
      onClose={onClose}
      sx={{
        width: "40%",
        flexShrink: 0,
        "& .MuiDrawer-paper": {
          width: "40%",
          p: 3,
          boxSizing: "border-box",
        },
      }}
    >
      <Box>
        {isLoading && (
          <Stack alignItems="center" justifyContent="center" height="100%">
            <CircularProgress />
            <Typography variant="body2" mt={2}>
              Loading product details...
            </Typography>
          </Stack>
        )}

        {isError && (
          <Typography color="error">Failed to load product details.</Typography>
        )}

        {product && (
          <>
            <Box mb={2}>
              <Typography variant="h5" fontWeight="bold">
                {product.name}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {product.sku ? `SKU: ${product.sku}` : "No SKU"}
              </Typography>
            </Box>

            <Divider sx={{ mb: 2 }} />

            <Stack spacing={2}>
              <Box>
                <Typography variant="subtitle2" color="text.secondary">
                  Description
                </Typography>
                <Typography variant="body1">
                  {product.description || "No description provided"}
                </Typography>
              </Box>

              <Box>
                <Typography variant="subtitle2" color="text.secondary">
                  Price
                </Typography>
                <Typography variant="body1">${product.price.toFixed(2)}</Typography>
              </Box>

              <Box>
                <Typography variant="subtitle2" color="text.secondary">
                  Status
                </Typography>
                <Chip
                  label={product.isActive ? "Active" : "Inactive"}
                  color={product.isActive ? "success" : "default"}
                  variant="outlined"
                />
              </Box>

              {product.stock && (
                <Box>
                  <Typography variant="subtitle2" color="text.secondary">
                    Stock Information
                  </Typography>
                  <Typography variant="body1">
                    Quantity: <strong>{product.stock.quantity}</strong>
                  </Typography>
                  <Typography variant="body1">
                    Reorder Point: <strong>{product.stock.reorderPoint}</strong>
                  </Typography>
                </Box>
              )}
            </Stack>

            <Divider sx={{ my: 3 }} />

            <Stack direction="row" spacing={2}>
              <Button variant="outlined" color="primary" onClick={onClose}>
                Close
              </Button>
            </Stack>
          </>
        )}
      </Box>
    </Drawer>
  );
}
