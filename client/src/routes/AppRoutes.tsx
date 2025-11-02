import { createBrowserRouter, RouterProvider } from "react-router";
import Layout from "../components/layout/Layout";
import HomePage from "../features/home/HomePage";
import ProductsPage from "../features/products/ProductsPage";
import CustomersPage from "../features/customers/CustomersPage";
import TransactionsPage from "../features/transactions/TransactionsPage";

const router = createBrowserRouter([
  {
    path: "/",
    element: <Layout />,
    children: [
      /* Right now, I only actively use the products page */
      { index: true, element: <HomePage /> },
      { path: "products", element: <ProductsPage /> },
      { path: "customers", element: <CustomersPage /> },
      { path: "transactions", element: <TransactionsPage /> },
    ],
  },
]);

export default function AppRoutes() {
  return <RouterProvider router={router} />;
}
