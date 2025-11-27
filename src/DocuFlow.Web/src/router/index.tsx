import { createBrowserRouter } from "react-router-dom";
import LoginPage from "../features/auth/LoginPage";
import RegisterPage from "../features/auth/RegisterPage";
import ProtectedRoute from "../components/ProtectedRoute";
import DashboardPage from "../features/dashboard/DashBoardPage";
import DocumentsPage from "../features/documents/DocumentsPage";
import DocumentDetailPage from "../features/documents/DocumentDetailPage";

const router = createBrowserRouter([
  {
    path: "/login",
    element: <LoginPage />,
  },
  {
    path: "/register",
    element: <RegisterPage />,
  },
  {
    path: "/",
    element: <ProtectedRoute />,
    children: [
      {
        index: true,
        element: <DashboardPage />,
      },
      {
        path: "documents",
        element: <DocumentsPage />,
      },
      {
        path: "documents/:id",
        element: <DocumentDetailPage />,
      },
    ],
  },
]);

export default router;
