-- User profile and administrator authorization model.
-- New auth users are deliberately NOT administrators by default. The first
-- administrator must be bootstrapped explicitly; invited users are promoted
-- only by the trusted administrator invite flow.

create table public.profiles (
  id uuid primary key references auth.users (id) on delete cascade,
  email text not null,
  role text null check (role is null or role = 'administrator'),
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

comment on table public.profiles is
  'Application profile. A NULL role means the Auth user has not been provisioned for application access.';
comment on column public.profiles.role is
  'Only administrator exists in v1. NULL means no application access.';

alter table public.profiles enable row level security;

revoke all on table public.profiles from anon, authenticated;
grant select on table public.profiles to authenticated;

create or replace function public.is_admin()
returns boolean
language sql
stable
security definer
set search_path = public, pg_temp
as $$
  select exists (
    select 1
    from public.profiles p
    where p.id = (select auth.uid())
      and p.role = 'administrator'
  );
$$;

revoke all on function public.is_admin() from public, anon;
grant execute on function public.is_admin() to authenticated;

create policy "profiles_select_self_or_admin"
on public.profiles
for select
to authenticated
using (
  id = (select auth.uid())
  or (select public.is_admin())
);

create or replace function public.handle_new_auth_user()
returns trigger
language plpgsql
security definer
set search_path = public, pg_temp
as $$
begin
  insert into public.profiles (id, email, role)
  values (new.id, coalesce(new.email, ''), null)
  on conflict (id) do nothing;

  return new;
end;
$$;

revoke all on function public.handle_new_auth_user() from public, anon, authenticated;

create trigger on_auth_user_created_create_profile
after insert on auth.users
for each row execute function public.handle_new_auth_user();

create or replace function public.sync_auth_user_email()
returns trigger
language plpgsql
security definer
set search_path = public, pg_temp
as $$
begin
  update public.profiles
  set email = coalesce(new.email, ''),
      updated_at = now()
  where id = new.id;

  return new;
end;
$$;

revoke all on function public.sync_auth_user_email() from public, anon, authenticated;

create trigger on_auth_user_email_updated
after update of email on auth.users
for each row
when (old.email is distinct from new.email)
execute function public.sync_auth_user_email();
