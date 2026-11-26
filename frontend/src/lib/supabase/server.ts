import { createServerClient } from "@supabase/ssr";
import { getCookies, setCookie, setResponseHeader } from "@tanstack/solid-start/server";
import type { SupabaseClient } from "@supabase/supabase-js";

/** Creates a request-scoped Supabase client. Call only from server code. */
export function createSupabaseServerClient(): SupabaseClient {
  const url = process.env.VITE_SUPABASE_URL;
  const publishableKey = process.env.VITE_SUPABASE_PUBLISHABLE_KEY;
  if (!url || !publishableKey) {
    throw new Error("Supabase ist nicht konfiguriert. Bitte VITE_SUPABASE_URL und VITE_SUPABASE_PUBLISHABLE_KEY setzen.");
  }

  return createServerClient(url, publishableKey, {
    cookies: {
      getAll() {
        return Object.entries(getCookies()).map(([name, value]) => ({ name, value }));
      },
      setAll(cookiesToSet, headers) {
        for (const { name, value, options } of cookiesToSet) setCookie(name, value, options);
        for (const [name, value] of Object.entries(headers)) setResponseHeader(name, value);
      }
    }
  });
}
