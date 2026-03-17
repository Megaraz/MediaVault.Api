import { createBrowserRouter, RouterProvider } from "react-router-dom";
import Layout from "./Shared/Layout";
import HomePage from "./Components/Pages/HomePage";
import UsersApiTest from "./Components/Pages/UsersApiTest";
import "./App.css";

const router = createBrowserRouter([
  {
    path: "/",
    element: <Layout />,
    children: [
      { path: "/", element: <HomePage /> },
      { path: "/users-api-test", element: <UsersApiTest /> },
    ],
  },
]);

export default function App() {
  return <RouterProvider router={router} />;
}
