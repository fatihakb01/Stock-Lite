import { createBrowserRouter, RouterProvider } from "react-router";
import LoginPage from "../features/users/LoginPage";
import HomePage from "../features/home/HomePage";
import ProductsPage from "../features/products/ProductsPage";
import CustomersPage from "../features/customers/CustomersPage";
import TransactionsPage from "../features/transactions/TransactionsPage";
import AuthLayout from "../components/layout/AuthLayout";
import ProtectedRoute from "./ProtectedRoute";
import AppLayout from "../components/layout/AppLayout";
import UsersPage from "../features/users/UsersPage";
import ConfirmEmailPage from "../features/users/ConfirmEmailPage";
import ChangePasswordPage from "../features/users/ChangePasswordPage";
import ForgotPasswordPage from "../features/users/ForgotPasswordPage";

const router = createBrowserRouter([
  // Public routes (no sidebar)
  {
    element: <AuthLayout />,
    children: [
      { path: "/", element: <LoginPage />},
      { path: "/login", element: <LoginPage /> },
      { path: "/confirm-email", element: <ConfirmEmailPage /> },
      { path: "/change-password", element: <ChangePasswordPage /> },
      { path: "/forgot-password", element: <ForgotPasswordPage /> }
    ]
  },

  // Private routes (with sidebar)
  {
    element: (
    <ProtectedRoute>
      <AppLayout />
    </ProtectedRoute>
    ),
    children: [
      { path: "/home", element: <HomePage /> },
      { path: "/products", element: <ProductsPage /> },
      { path: "/customers", element: <CustomersPage /> },
      { path: "/transactions", element: <TransactionsPage /> },
      { path: "/users", element: <UsersPage /> }
    ]
  }
]);

export default function AppRoutes() {
  return <RouterProvider router={router} />;
}
