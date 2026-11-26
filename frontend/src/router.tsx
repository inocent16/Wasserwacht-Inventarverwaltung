import { QueryClient } from "@tanstack/solid-query";
import { createRouter } from "@tanstack/solid-router";
import { routeTree } from "./routeTree.gen";

export function getRouter() {
  // A fresh QueryClient is created for each router/request. Never share
  // user-specific server cache state across SSR requests.
  const queryClient = new QueryClient();

  return createRouter({
    routeTree,
    context: { queryClient },
    defaultPreload: "intent",
    scrollRestoration: true
  });
}

declare module "@tanstack/solid-router" {
  interface Register {
    router: ReturnType<typeof getRouter>;
  }
}
