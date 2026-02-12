import axios from "axios";
import toast from "react-hot-toast";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL, 
  withCredentials: true,
  headers: { "Content-Type": "application/json" },
});

// ADD TOKEN
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// ERROR HANDLER
api.interceptors.response.use(
  (res) => res,  
  (error) => {
    // No response → network error
    if (!error.response) {
      toast.error("Network error. Check your internet connection.");
      return Promise.reject(error);
    }

    const { status, data } = error.response;

    // Automatic message extraction
    const serverMessage =
      data?.message ||
      data?.error ||
      data?.title ||
      data?.detail ||
      (typeof data === "string" ? data : null);

    switch (status) {
      case 400:
        // Validation errors → return to React Query so components can show them inline
        if (data?.errors) {
          const flatErrors = Object.values(data.errors).flat();
          return Promise.reject(flatErrors);
        }
        // Otherwise: toast because it's user error
        toast.error(serverMessage || "Invalid request.");
        break;

      case 401:
        toast.error("Session expired. Please sign in again.");
        localStorage.removeItem("token");
        break;

      case 403:
        toast.error("You do not have permission.");
        break;

      case 404:
        toast.error("Not found.");
        break;

      case 500:
        toast.error("Server error. Please try again later.");
        break;

      default:
        toast.error(serverMessage || "Unexpected error occurred.");
    }

    return Promise.reject(error);
  }
);

export default api;
