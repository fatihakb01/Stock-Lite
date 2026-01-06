
import { CircularProgress, Drawer, Stack, Typography } from "@mui/material";
import { useProduct } from "../../../hooks/useProducts";
import ProductOverviewTab from "./tabs/ProductOverviewTab";
import { useState } from "react";
import { Tabs, Tab } from "@mui/material";
import TabPanel from "../../../components/tabs/TabPanel";
import ProductTransactionsTab from "./tabs/ProductTransactionsTab";
import ProductPoTab from "./tabs/ProductPoTab";
import ProductPfcTab from "./tabs/ProductPfcTab";
import ProductCoTab from "./tabs/ProductCoTab";

interface ProductDetailDrawerProps {
  productId: string | null;
  onClose: () => void;
}

export default function ProductDetailDrawer({
  productId,
  onClose,
}: ProductDetailDrawerProps) {
  const { 
    data: product, 
    isLoading: isLoadingProduct, 
    isError: isErrorProduct 
  } = useProduct(productId);

  const [tab, setTab] = useState(0);

  return (
    <Drawer
      variant="temporary"
      anchor="right"
      open={!!productId}
      onClose={onClose}
      sx={{
        width: "50%",
        flexShrink: 0,
        "& .MuiDrawer-paper": {
          width: "50%",
          p: 3,
          boxSizing: "border-box",
        },
      }}
    >
      {/* Tabs */}
      <Tabs
        value={tab}
        onChange={(_, newValue) => setTab(newValue)}
        variant="scrollable"
        scrollButtons="auto"
      >
        <Tab label="Overview" />
        <Tab label="Transactions" />
        <Tab label="Purchase Orders" />
        <Tab label="Purchase Forecast" />
        <Tab label="Confirmed Orders" />
      </Tabs>


      {/* Check whether product data is valid */}
      {isLoadingProduct && (
        <Stack alignItems="center" justifyContent="center" height="100%">
          <CircularProgress />
          <Typography variant="body2" mt={2}>
            Loading product details...
          </Typography>
        </Stack>
      )}

      {isErrorProduct && (
        <Typography color="error">Failed to load product details.</Typography>
      )}

      {product && (
        <>
          <TabPanel value={tab} index={0}>
            <ProductOverviewTab product={product} />
          </TabPanel>

          <TabPanel value={tab} index={1}>
            <ProductTransactionsTab productId={product.id} />
          </TabPanel>

          <TabPanel value={tab} index={2}>
            <ProductPoTab productId={product.id} />
          </TabPanel>

          <TabPanel value={tab} index={3}>
            <ProductPfcTab productId={product.id} />
          </TabPanel>

          <TabPanel value={tab} index={4}>
            <ProductCoTab productId={product.id} />
          </TabPanel>
        </>
      )}

 
    </Drawer>
  );
}
