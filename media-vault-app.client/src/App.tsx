import { createBrowserRouter, RouterProvider } from "react-router-dom";
import Layout from "./Shared/Layout";
import HomePage from "./Components/Pages/HomePage";
import UsersApiTest from "./Components/Pages/UsersApiTest";
import Dashboard from "./Components/Pages/Dashboard";
import "./App.css";
import { UserProvider } from "./Shared/UserContext";

const router = createBrowserRouter([
  {
    path: "/",
    element: <Layout />,
    children: [
      { path: "/", element: <HomePage /> },
      { path: "/users-api-test", element: <UsersApiTest /> },
    ],
  },
  {
    path: "/dashboard",
    element: <Dashboard />,
  },
]);

export default function App() {
  return (
    <UserProvider>
      <RouterProvider router={router} />
    </UserProvider>
  );
}
