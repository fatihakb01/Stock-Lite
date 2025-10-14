import { useState } from "react";
import { Box, Drawer, Typography, Divider } from "@mui/material";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { useProducts } from "../../hooks/useProducts";
import type { Product } from "../products/productTypes";

export default function ProductsPage() {
  const { data: products, isLoading, isError } = useProducts();

  const [selected, setSelected] = useState<Product | null>(null);

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
        <Typography variant="h5" mb={2}>
          Products
        </Typography>

        <Box flex={1} sx={{ width: "100%"}}>
          <DataGrid
            rows={products ?? []}
            columns={columns}
            getRowId={(row) => row.id}
            onRowClick={(params) => setSelected(params.row)}
            disableRowSelectionOnClick
            pageSizeOptions={[10, 25, 50]}
            initialState={{
              pagination: { paginationModel: { pageSize: 10, page: 0 } },
            }} 
          />
        </Box>
      </Box>

      {/* Detail Drawer */}
      <Drawer
        variant="temporary"
        anchor="right"
        open={!!selected}
        onClose={() => setSelected(null)}
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
        {selected && (
          <Box>
            <Typography variant="h6" gutterBottom>
              {selected.name}
            </Typography>
            <Divider sx={{ mb: 2 }} />
            <Typography variant="body1" gutterBottom>
              <strong>Description:</strong> {selected.description || "—"}
            </Typography>
            <Typography variant="body1" gutterBottom>
              <strong>SKU:</strong> {selected.sku || "—"}
            </Typography>
            <Typography variant="body1" gutterBottom>
              <strong>Price:</strong> ${selected.price.toFixed(2)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              <strong>Status:</strong> {selected.isActive ? "Active" : "Inactive"}
            </Typography>

            <Box mt={3}>
              <Typography
                color="primary"
                sx={{ cursor: "pointer" }}
                onClick={() => setSelected(null)}
              >
                Close
              </Typography>
            </Box>
          </Box>
        )}
      </Drawer>
    </Box>
  );
}
