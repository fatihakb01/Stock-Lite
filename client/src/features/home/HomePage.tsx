import { Box, Typography, Card, CardContent } from "@mui/material";
import { useProductKpi } from "../../hooks/useProducts";
import { useNavigate } from "react-router-dom";

export default function HomePage() {
  const { data: data } = useProductKpi();
  const navigate = useNavigate();

  const kpis = [
    { 
      label: "Total Products", 
      value: data?.totalProductCount || 0, 
      filter: "" 
    },
    { 
      label: "Products Below Safety Stock", 
      value: data?.belowSafetyStockCount || 0, 
      filter: "BelowSafetyStock" 
    },
    { 
      label: "Products With Recommended Orders", 
      value: data?.recommendedOrdersCount || 0, 
      filter: "WithRecommendedOrders" 
    },
    { 
      label: "Products Without Stock at Lead Time", 
      value: data?.withoutStockAtLeadTimeCount || 0, 
      filter: "WithoutStockAtLeadTime" 
    },
    { 
      label: "Products Without Forecast", 
      value: data?.withoutForecastCount || 0, 
      filter: "WithoutForecast" 
    },
    { 
      label: "Scheduler succesful?", 
      value: (data?.schedulerFailed || false) ? "❌" : "✅", 
      filter: "FailedJobs" 
    }
  ];

  return (
    <Box
      sx={{
        display: "flex",
        flexDirection: "column",
        flexGrow: 1,
        width: "95vw",
        maxWidth: "95vw",
        overflowX: "hidden",
        p: 2,
      }}
    >
      {/* Header */}
      <Typography variant="h5" mb={2} ml={5}>
        Dashboard
      </Typography>

      {/* KPI Grid */}
      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: {
            xs: "1fr",
            sm: "1fr 1fr",
            md: "1fr 1fr 1fr",
          },
          gap: 3,
          px: 5,
        }}
      >
        {kpis.map((kpi) => (
          <Card
            key={kpi.label}
            onClick={() => {
              if (!kpi.filter) {
                navigate("/products");
              } else {
                if (kpi.filter !== "FailedJobs") {
                  navigate(`/products?filter=${kpi.filter}`);
                }
              }
            }}
            sx={{
              cursor: "pointer",
              transition: "0.2s",
              "&:hover": {
                boxShadow: 6,
                transform: "translateY(-2px)",
              },
            }}
          >
            <CardContent>
              <Typography variant="body2" color="text.secondary">
                {kpi.label}
              </Typography>

              <Typography variant="h4" mt={1}>
                {kpi.value}
              </Typography>
            </CardContent>
          </Card>
        ))}
      </Box>
    </Box>
  );
}
