-- Keep the Supabase service_role key out of the inventory write path.
-- Auth Admin API operations may use the key server-side, but inventory records
-- must still be changed only through the audited RPCs as an authenticated user.
-- A future, narrowly scoped service-only function can be granted explicitly.

alter default privileges in schema public
  revoke all on tables from service_role;

alter default privileges in schema public
  revoke execute on functions from service_role;

revoke all on all tables in schema public from service_role;
revoke all on all functions in schema public from service_role;

comment on schema public is
  'Inventory tables are RLS protected; service_role has no direct privileges on application tables or functions. Grant future service-only capabilities narrowly.';
