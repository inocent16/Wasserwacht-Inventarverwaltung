import * as Solid from "solid-js";
import {
  createRootRouteWithContext,
  HeadContent,
  Outlet,
  Scripts
} from "@tanstack/solid-router";
import { QueryClientProvider, type QueryClient } from "@tanstack/solid-query";
import { HydrationScript } from "solid-js/web";
import appCss from "../styles.css?url";

type RouterContext = { queryClient: QueryClient };

export const Route = createRootRouteWithContext<RouterContext>()({
  head: () => ({
    meta: [
      { charSet: "utf-8" },
      { name: "viewport", content: "width=device-width, initial-scale=1" },
      { name: "theme-color", content: "#123b55" },
      { name: "description", content: "Wasserwacht Inventarverwaltung" },
      { title: "Wasserwacht · Inventarverwaltung" }
    ],
    links: [{ rel: "stylesheet", href: appCss }]
  }),
  component: RootComponent,
  shellComponent: RootDocument
});

function RootComponent() {
  const { queryClient } = Route.useRouteContext();
  return (
    <QueryClientProvider client={queryClient}>
      <Outlet />
    </QueryClientProvider>
  );
}

function RootDocument(props: { children: Solid.JSX.Element }) {
  return (
    <html lang="de">
      <head>
        <HydrationScript />
        <HeadContent />
      </head>
      <body>
        <Solid.Suspense>{props.children}</Solid.Suspense>
        <Scripts />
      </body>
    </html>
  );
}
