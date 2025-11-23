import { useState } from "react";
import { Box, Typography } from "@mui/material";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { useProducts } from "../../hooks/useProducts";
import type { Product } from "./productTypes";
import ProductDetailDrawer from "./components/ProductDetailDrawer";
import { useCompanies } from "../../hooks/useCompanies";

export default function ProductsPage() {
  // Since we have one company now, this is a temporary solution 
  // until we fixed authentication & authorisation
  const {data: companies} = useCompanies();
  const companyId = companies?.[0]?.id ?? null;

  // Retrieve products data
  const { data: products, isLoading, isError } = useProducts(companyId);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  // Define the columns for the DataGrid
  const columns: GridColDef<Product>[] = [
    { field: "name", headerName: "Name", flex: 1, width: 120 },
    { field: "description", headerName: "Description", flex: 2, minWidth: 200 },
    { field: "sku", headerName: "SKU", width: 120 },
    {
      field: "price",
      headerName: "Price",
      width: 120
    },
    {
      field: "isActive",
      headerName: "Active",
      width: 100,
      renderCell: (params) => (params.value ? "✅" : "❌"),
    },
  ];

  if (isLoading) return <Typography>Loading products...</Typography>;
  if (isError) return <Typography color="error">Failed to load products.</Typography>;

  return (
    <Box
      sx={{
        display: "flex",
        flexDirection: "column",
        flexGrow: 1,
        width: "100vw",      
        maxWidth: "100vw",   
        overflowX: "hidden", 
        p: 3,
      }}
    >
      {/* DataGrid Table */}
      <Box flex={1}>
        <Typography variant="h5" mb={2} ml={5}>
          Products
        </Typography>

        <Box flex={1} sx={{ width: "100%"}}>
          <DataGrid
            rows={products ?? []}
            columns={columns}     
            getRowId={(row) => row.id}
            onRowClick={(params) => setSelectedId(params.row.id)}
            disableRowSelectionOnClick
            pageSizeOptions={[10, 25, 50]}
            initialState={{
              pagination: { paginationModel: { pageSize: 10, page: 0 } },
            }} 
          />
        </Box>
      </Box>

      {/* Detail Drawer */}
      <ProductDetailDrawer productId={selectedId} companyId={companyId} onClose={() => setSelectedId(null)} />
    </Box>
  );
}
