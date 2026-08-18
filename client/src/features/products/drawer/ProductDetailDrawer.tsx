
import { CircularProgress, Drawer, Stack, Typography } from "@mui/material";
import { useProduct } from "../../../hooks/useProducts";
import ProductOverviewTab from "./tabs/ProductOverviewTab";
import { useRef, useState } from "react";
import { Tabs, Tab } from "@mui/material";
import TabPanel from "../../../components/tabs/TabPanel";
import ProductTransactionsTab from "./tabs/ProductTransactionsTab";
import ProductPoTab from "./tabs/ProductPoTab";
import ProductPfcTab from "./tabs/ProductPfcTab";
import ProductCoTab from "./tabs/ProductCoTab";
import DrawerResizeHandle from "./handle/DrawerResizeHandle";
import ProductUdfTab from "./tabs/ProductUdfTab";
import { useProductRealtime } from "../../../hooks/useProductRealtime";

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

  useProductRealtime(productId);

  const [tab, setTab] = useState(0);
  const [drawerWidthPct, setDrawerWidthPct] = useState(50);
  const isDraggingRef = useRef(false);

  return (
    <Drawer
      variant="temporary"
      anchor="right"
      open={!!productId}
      onClose={onClose}
      sx={{
        "& .MuiDrawer-paper": {
          width: `${drawerWidthPct}%`,
          p: 3,
          boxSizing: "border-box",
          transition: "width 300ms ease",
        },
      }}
    >
      {/* Resize Handle */}
      <DrawerResizeHandle
        onResize={(w) => {
          isDraggingRef.current = true;
          setDrawerWidthPct(w);
        }}
      />

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
        <Tab label="Additional Information" />
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

          <TabPanel value={tab} index={5}>
            <ProductUdfTab productId={product.id} />
          </TabPanel>
        </>
      )}

 
    </Drawer>
  );
}
