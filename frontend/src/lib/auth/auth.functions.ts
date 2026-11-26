import { createServerFn } from "@tanstack/solid-start";
import { setResponseHeader } from "@tanstack/solid-start/server";
import { z } from "zod";
import { createSupabaseServerClient } from "../supabase/server";

const loginSchema = z.object({
  email: z.string().trim().email().max(320),
  password: z.string().min(1).max(256)
});

export type AuthenticatedUser = {
  id: string;
  email: string;
};

export const getCurrentUser = createServerFn({ method: "GET" }).handler(
  async (): Promise<AuthenticatedUser | null> => {
    setResponseHeader("Cache-Control", "private, no-store");

    try {
      const supabase = createSupabaseServerClient();
      const { data, error } = await supabase.auth.getUser();
      if (error || !data.user?.email) return null;

      const { data: profile, error: profileError } = await supabase
        .from("profiles")
        .select("role")
        .eq("id", data.user.id)
        .maybeSingle();

      if (profileError || profile?.role !== "administrator") return null;

      return { id: data.user.id, email: data.user.email };
    } catch {
      // An unconfigured local environment should still render the login page.
      return null;
    }
  }
);

export const signInWithPassword = createServerFn({ method: "POST" })
  .validator((input: unknown) => loginSchema.parse(input))
  .handler(async ({ data }): Promise<
    | { ok: true; user: AuthenticatedUser }
    | { ok: false; message: string }
  > => {
    setResponseHeader("Cache-Control", "private, no-store");

    try {
      const supabase = createSupabaseServerClient();
      const { data: authData, error } = await supabase.auth.signInWithPassword({
        email: data.email.toLowerCase(),
        password: data.password
      });

      // Keep the response intentionally generic to avoid account enumeration.
      if (error || !authData.user?.email) {
        return { ok: false, message: "E-Mail-Adresse oder Passwort ist nicht korrekt." };
      }

      const { data: profile, error: profileError } = await supabase
        .from("profiles")
        .select("role")
        .eq("id", authData.user.id)
        .maybeSingle();

      if (profileError || profile?.role !== "administrator") {
        await supabase.auth.signOut();
        return {
          ok: false,
          message: "Für dieses Konto ist der Zugang zur Inventarverwaltung noch nicht freigeschaltet."
        };
      }

      return {
        ok: true,
        user: { id: authData.user.id, email: authData.user.email }
      };
    } catch {
      return {
        ok: false,
        message: "Anmeldung momentan nicht möglich. Bitte Supabase-Konfiguration und Verbindung prüfen."
      };
    }
  });

export const signOut = createServerFn({ method: "POST" }).handler(async () => {
  setResponseHeader("Cache-Control", "private, no-store");

  try {
    const supabase = createSupabaseServerClient();
    const { error } = await supabase.auth.signOut();
    if (error) return { ok: false as const };
    return { ok: true as const };
  } catch {
    return { ok: false as const };
  }
});
