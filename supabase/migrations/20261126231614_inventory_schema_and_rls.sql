-- Core inventory schema and read-only RLS boundary.
-- Future public-schema functions are not executable by API roles unless explicitly granted.
alter default privileges revoke execute on functions from public;
alter default privileges in schema public revoke execute on functions from anon, authenticated;
-- All writes are intentionally routed through SECURITY DEFINER RPC functions
-- in the following migration, where each business action writes one audit row.

create table public.articles (
  id uuid primary key default gen_random_uuid(),
  name varchar(200) not null check (length(btrim(name)) > 0),
  category varchar(100) not null check (length(btrim(category)) > 0),
  description varchar(2000),
  manufacturer varchar(200),
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create index articles_name_idx on public.articles (name);

create table public.locations (
  id uuid primary key default gen_random_uuid(),
  name varchar(200) not null unique check (length(btrim(name)) > 0),
  description varchar(2000),
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create table public.stock_items (
  id uuid primary key default gen_random_uuid(),
  article_id uuid not null references public.articles (id) on delete restrict,
  location_id uuid not null references public.locations (id) on delete restrict,
  quantity integer not null check (quantity > 0),
  expiration_date date,
  batch_number varchar(200),
  manufacturer varchar(200),
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create index stock_items_article_id_idx on public.stock_items (article_id);
create index stock_items_location_id_idx on public.stock_items (location_id);

-- PostgreSQL 15+ NULLS NOT DISTINCT means two stock items with the same
-- article/location/expiry/batch/manufacturer identity cannot coexist even
-- when one or more optional identity fields are NULL.
create unique index stock_items_identity_ux
  on public.stock_items (
    article_id,
    location_id,
    expiration_date,
    batch_number,
    manufacturer
  )
  nulls not distinct;

create table public.audit_log (
  id uuid primary key default gen_random_uuid(),
  occurred_at_utc timestamptz not null default now(),
  actor_user_id uuid not null,
  actor_name_snapshot varchar(200) not null,
  event_type varchar(100) not null,
  before_state jsonb,
  after_state jsonb,
  metadata jsonb not null default '{}'::jsonb,
  constraint audit_log_has_state
    check (before_state is not null or after_state is not null)
);

comment on table public.audit_log is
  'Single append-only source of history. One row per successful business action; no StockMovement table exists.';
comment on column public.audit_log.actor_user_id is
  'Immutable Auth user UUID snapshot; deliberately has no FK so history survives account deletion.';
comment on column public.audit_log.actor_name_snapshot is
  'Email/actor display snapshot at action time; never joined back to mutable profile data.';
comment on column public.audit_log.before_state is
  'Complete JSONB snapshot of all affected records immediately before the business action.';
comment on column public.audit_log.after_state is
  'Complete JSONB snapshot of all affected records immediately after the business action.';
comment on column public.audit_log.metadata is
  'Non-state action metadata such as an optional user note. Does not contain a second history stream.';

create index audit_log_occurred_at_idx on public.audit_log (occurred_at_utc desc);
create index audit_log_actor_user_id_idx on public.audit_log (actor_user_id);
create index audit_log_event_type_idx on public.audit_log (event_type);

-- Refresh the helper with a controlled search_path. It is SECURITY DEFINER so
-- the profile lookup is not blocked by profiles RLS; it returns only a boolean.
create or replace function public.is_admin()
returns boolean
language sql
stable
security definer
set search_path = ''
as $$
  select exists (
    select 1
    from public.profiles p
    where p.id = auth.uid()
      and p.role = 'administrator'
  );
$$;

revoke all on function public.is_admin() from public, anon;
grant execute on function public.is_admin() to authenticated;

alter table public.articles enable row level security;
alter table public.locations enable row level security;
alter table public.stock_items enable row level security;
alter table public.audit_log enable row level security;

create policy articles_select_admin
  on public.articles for select to authenticated
  using ((select public.is_admin()));

create policy locations_select_admin
  on public.locations for select to authenticated
  using ((select public.is_admin()));

create policy stock_items_select_admin
  on public.stock_items for select to authenticated
  using ((select public.is_admin()));

create policy audit_log_select_admin
  on public.audit_log for select to authenticated
  using ((select public.is_admin()));

-- No direct DML through PostgREST for the authenticated role. Mutations go
-- through audited RPC functions only. The anon role gets no table privileges.
revoke all on table public.articles from anon, authenticated;
revoke all on table public.locations from anon, authenticated;
revoke all on table public.stock_items from anon, authenticated;
revoke all on table public.audit_log from anon, authenticated;

grant select on table public.articles to authenticated;
grant select on table public.locations to authenticated;
grant select on table public.stock_items to authenticated;
grant select on table public.audit_log to authenticated;

create or replace function public.prevent_audit_log_mutation()
returns trigger
language plpgsql
set search_path = pg_catalog
as $$
begin
  raise exception 'audit_log is append-only; UPDATE and DELETE are forbidden'
    using errcode = '55000';
end;
$$;

revoke all on function public.prevent_audit_log_mutation() from public, anon, authenticated;

create trigger audit_log_append_only
  before update or delete on public.audit_log
  for each row execute function public.prevent_audit_log_mutation();
