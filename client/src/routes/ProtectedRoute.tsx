import type { JSX } from "react";
import { Navigate } from "react-router-dom";
import { useCompanyRealtime } from "../hooks/useCompanyRealtime";

export default function ProtectedRoute({ children }: { children: JSX.Element }) {
  const token = localStorage.getItem("token");

    if (token) {
      useCompanyRealtime();
      return children;
  }

    return <Navigate to="/login" replace />;
}
